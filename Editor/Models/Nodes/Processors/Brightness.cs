using Misaki.TextureMaker.CodeGen;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace Misaki.TextureMaker
{
    internal class Brightness : TextureExecutableNode
    {
        private IPort _inputPort;
        private IPort _brightnessPort;
        private IPort _outputPort;

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            _inputPort = context.AddInputPort<Color>("Input").Build();
            _brightnessPort = context.AddInputPort<float>("Brightness").WithDefaultValue(1).Build();
            _outputPort = context.AddOutputPort<Color>("Output").Build();
        }

        public override void GenerateCode(ICodeGenContext context, string nodeId)
        {
            context.AddUsing("Unity.Mathematics");
            context.AddUsing("UnityEngine");

            var inputVar = context.GetInputVariable(_inputPort);
            var brightnessVar = context.GetInputVariable(_brightnessPort);
            var outputVar = context.GetOutputVariable(_outputPort);

            context.AddLine($"// Brightness Node {nodeId}");

            // Declare input variables if they don't come from connected ports
            if (!_inputPort.isConnected)
            {
                var inputType = GetPortTypeName(_inputPort.dataType);
                context.DeclareVariable(inputType, inputVar, "Color.black");
            }

            if (!_brightnessPort.isConnected)
            {
                var brightnessType = GetPortTypeName(_brightnessPort.dataType);
                context.DeclareVariable(brightnessType, brightnessVar, "1f");
            }

            context.AddLine($"var inputFloat4_{nodeId} = new Unity.Mathematics.float4({inputVar}.r, {inputVar}.g, {inputVar}.b, {inputVar}.a);");
            context.AddLine($"var brightened_{nodeId} = inputFloat4_{nodeId} * {brightnessVar};");
            context.AddLine($"var {outputVar} = new UnityEngine.Color(brightened_{nodeId}.x, brightened_{nodeId}.y, brightened_{nodeId}.z, brightened_{nodeId}.w);");

            // Register the output variable for other nodes to use
            context.RegisterOutputVariable(_outputPort, outputVar);
        }

        public override void GenerateDataFields(ICodeGenContext context, string nodeId)
        {
            // Brightness doesn't need cached data - values come from inputs
            // This is a simple operation node
        }

        public override void GenerateDataInitialization(ICodeGenContext context, string nodeId)
        {
            // No initialization needed for brightness
        }
    }
}