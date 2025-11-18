using System;
using Unity.GraphToolkit.Editor;
using Unity.Mathematics;

namespace Misaki.TextureMaker
{
    [Serializable]
    internal class ColorSpaceConversion : CodeGenerationNode
    {
        internal enum ConversionType
        {
            sRGB_to_Linear,
            Linear_to_sRGB,
        }

        private IPort _inputPort;
        private IPort _outputPort;

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            _inputPort = context.AddInputPort<float4>("Input").Build();
            _outputPort = context.AddOutputPort<float4>("Result").Build();
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<ConversionType>("Conversion Type").Build();
            context.AddOption<bool>("Preserve Alpha").WithDefaultValue(true).Build();
        }

        public override void Initialize(IShaderLibrary shaderLibrary)
        {
            var functionTemplate = new FunctionDeclaration
            {
                returnType = ShaderVariableType.Float4,
                flags = FunctionFlag.Inlineable,
                signature = new()
                {
                    new ParameterDeclaration
                    {
                        name = "color",
                        type = ShaderVariableType.Float4
                    },
                    new ParameterDeclaration
                    {
                        name = "preserveAlpha",
                        type = ShaderVariableType.Bool
                    }
                }
            };

            var srgbToLinearFunc = functionTemplate;
            srgbToLinearFunc.name = "ColorSpaceConversion_sRGB_Linear";
            srgbToLinearFunc.code = @"
    float4 linearRGBLo = {0} / 12.92;;
    float4 linearRGBHi = pow(max(abs(({0} + 0.055) / 1.055), 1.192092896e-07), 2.4);
    float4 result = float4({0} <= 0.04045) ? linearRGBLo : linearRGBHi;
    return {1} ? float4(result.x, result.y, result.z, {0}.w) : result;";

            var linearTosrgbFunc = functionTemplate;
            linearTosrgbFunc.name = "ColorSpaceConversion_Linear_sRGB";
            linearTosrgbFunc.code = @"
    float4 sRGBLo = {0} * 12.92;
    float4 sRGBHi = (pow(max(abs({0}), 1.192092896e-07), 0.4166667) * 1.055) - 0.055;
    float4 result = float4({0} <= 0.0031308) ? sRGBLo : sRGBHi;
    return {1} ? float4(result.x, result.y, result.z, {0}.w) : result;";

            shaderLibrary.AddFunction(srgbToLinearFunc);
            shaderLibrary.AddFunction(linearTosrgbFunc);
        }

        public override void GenerateCode(ICodeGenContext ctx)
        {
            var conversionType = GetOptionValue<ConversionType>("Conversion Type");
            var preserveAlpha = GetOptionValue<bool>("Preserve Alpha");

            var inputVar = ctx.GetInputVariableName<float4>(_inputPort, color =>
            {
                return new ConstantExpr($"float4({color.x}, {color.y}, {color.z}, {color.w})");
            });

            var outputVar = ctx.GetOutputVariableName(_outputPort);

            var functionName = conversionType switch
            {
                ConversionType.sRGB_to_Linear => "ColorSpaceConversion_sRGB_Linear",
                ConversionType.Linear_to_sRGB => "ColorSpaceConversion_Linear_sRGB",
                _ => throw new NotSupportedException($"Unexpected conversion type {conversionType}")
            };

            ctx.AddInstruction(new Instruction
            {
                result = new VariableDeclaration
                {
                    type = ShaderVariableType.Float4,
                    name = outputVar
                },
                expression = new FunctionCallExpr(functionName, new()
                {
                    new VariableExpr(inputVar),
                    new ConstantExpr(preserveAlpha ? "true" : "false")
                })
            });
        }
    }
}
