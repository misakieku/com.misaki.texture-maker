using Misaki.TextureMaker.CodeGen;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace Misaki.TextureMaker
{
    internal enum MathOperationType
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Power,
        Min,
        Max,
        Abs,
        Floor,
        Ceil,
        Round,
        Fract,
        Sin,
        Cos,
        Sqrt,
        Clamp01
    }

    internal class MathOperation : TextureExecutableNode
    {
        private IPort _aPort;
        private IPort _bPort;
        private IPort _outputPort;

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            _aPort = context.AddInputPort<Color>("A").Build();
            _bPort = context.AddInputPort<Color>("B").Build();

            _outputPort = context.AddOutputPort<Color>("Output").Build();
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<MathOperationType>("Operation").WithDefaultValue(MathOperationType.Add).Build();
        }

        public override void GenerateCode(ICodeGenContext context, string nodeId)
        {
            context.AddUsing("Unity.Mathematics");
            context.AddUsing("UnityEngine");

            var aVar = context.GetInputVariable(_aPort);
            var bVar = context.GetInputVariable(_bPort);
            var outputVar = context.GetOutputVariable(_outputPort);
            var operationField = context.GetDataFieldName(nodeId, "operation");

            context.AddLine($"// MathOperation Node {nodeId}");
            
            // Declare input variables if not connected
            if (!_aPort.isConnected)
                context.DeclareVariable("Color", aVar, "Color.black");
            if (!_bPort.isConnected)
                context.DeclareVariable("Color", bVar, "Color.black");

            context.AddLine($"var vA_{nodeId} = new Unity.Mathematics.float4({aVar}.r, {aVar}.g, {aVar}.b, {aVar}.a);");
            context.AddLine($"var vB_{nodeId} = new Unity.Mathematics.float4({bVar}.r, {bVar}.g, {bVar}.b, {bVar}.a);");
            context.AddLine($"Unity.Mathematics.float4 result_{nodeId};");

            context.AddLine($"switch (data.{operationField})");
            context.AddLine("{");
            context.AddLine("    case MathOperationType.Add:");
            context.AddLine($"        result_{nodeId} = vA_{nodeId} + vB_{nodeId}; break;");
            context.AddLine("    case MathOperationType.Subtract:");
            context.AddLine($"        result_{nodeId} = vA_{nodeId} - vB_{nodeId}; break;");
            context.AddLine("    case MathOperationType.Multiply:");
            context.AddLine($"        result_{nodeId} = vA_{nodeId} * vB_{nodeId}; break;");
            context.AddLine("    case MathOperationType.Divide:");
            context.AddLine($"        result_{nodeId} = vA_{nodeId} / vB_{nodeId}; break;");
            context.AddLine("    case MathOperationType.Power:");
            context.AddLine($"        result_{nodeId} = GeneratedCodeHelpers.Power(vA_{nodeId}, vB_{nodeId}); break;");
            context.AddLine("    case MathOperationType.Min:");
            context.AddLine($"        result_{nodeId} = Unity.Mathematics.math.min(vA_{nodeId}, vB_{nodeId}); break;");
            context.AddLine("    case MathOperationType.Max:");
            context.AddLine($"        result_{nodeId} = Unity.Mathematics.math.max(vA_{nodeId}, vB_{nodeId}); break;");
            context.AddLine("    case MathOperationType.Abs:");
            context.AddLine($"        result_{nodeId} = GeneratedCodeHelpers.Abs(vA_{nodeId}); break;");
            context.AddLine("    case MathOperationType.Floor:");
            context.AddLine($"        result_{nodeId} = Unity.Mathematics.math.floor(vA_{nodeId}); break;");
            context.AddLine("    case MathOperationType.Ceil:");
            context.AddLine($"        result_{nodeId} = Unity.Mathematics.math.ceil(vA_{nodeId}); break;");
            context.AddLine("    case MathOperationType.Round:");
            context.AddLine($"        result_{nodeId} = Unity.Mathematics.math.round(vA_{nodeId}); break;");
            context.AddLine("    case MathOperationType.Fract:");
            context.AddLine($"        result_{nodeId} = GeneratedCodeHelpers.Fract(vA_{nodeId}); break;");
            context.AddLine("    case MathOperationType.Sin:");
            context.AddLine($"        result_{nodeId} = GeneratedCodeHelpers.Sin(vA_{nodeId}); break;");
            context.AddLine("    case MathOperationType.Cos:");
            context.AddLine($"        result_{nodeId} = GeneratedCodeHelpers.Cos(vA_{nodeId}); break;");
            context.AddLine("    case MathOperationType.Sqrt:");
            context.AddLine($"        result_{nodeId} = Unity.Mathematics.math.sqrt(vA_{nodeId}); break;");
            context.AddLine("    case MathOperationType.Clamp01:");
            context.AddLine($"        result_{nodeId} = GeneratedCodeHelpers.Clamp01(vA_{nodeId}); break;");
            context.AddLine("    default:");
            context.AddLine($"        result_{nodeId} = vA_{nodeId}; break;");
            context.AddLine("}");

            context.AddLine($"var {outputVar} = new UnityEngine.Color(result_{nodeId}.x, result_{nodeId}.y, result_{nodeId}.z, result_{nodeId}.w);");

            // Register output variable
            context.RegisterOutputVariable(_outputPort, outputVar);
        }

        public override void GenerateDataFields(ICodeGenContext context, string nodeId)
        {
            var operationField = context.GetDataFieldName(nodeId, "operation");
            context.AddDataField("MathOperationType", operationField, $"Math operation type for {nodeId}");
        }

        public override void GenerateDataInitialization(ICodeGenContext context, string nodeId)
        {
            var operation = GetOptionValue<MathOperationType>("Operation");
            var operationField = context.GetDataFieldName(nodeId, "operation");
            context.AddInitializationLine($"data.{operationField} = MathOperationType.{operation};");
        }
    }
}