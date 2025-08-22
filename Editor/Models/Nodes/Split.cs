using Misaki.TextureMaker.CodeGen;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace Misaki.TextureMaker
{
    internal class Split : TextureExecutableNode
    {
        private IPort _inputPort;
        private IPort _rPort;
        private IPort _gPort;
        private IPort _bPort;
        private IPort _aPort;

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            _inputPort = context.AddInputPort<Color>("Input").Build();

            _rPort = context.AddOutputPort<float>("R").Build();
            _gPort = context.AddOutputPort<float>("G").Build();
            _bPort = context.AddOutputPort<float>("B").Build();
            _aPort = context.AddOutputPort<float>("A").Build();
        }

        public override void GenerateCode(ICodeGenContext context, string nodeId)
        {
            context.AddUsing("UnityEngine");

            var inputVar = context.GetInputVariable(_inputPort);
            var rVar = context.GetOutputVariable(_rPort);
            var gVar = context.GetOutputVariable(_gPort);
            var bVar = context.GetOutputVariable(_bPort);
            var aVar = context.GetOutputVariable(_aPort);

            context.AddLine($"// Split Node {nodeId}");

            // Declare input variable if not connected
            if (!_inputPort.isConnected)
            {
                var inputType = GetPortTypeName(_inputPort.dataType);
                context.DeclareVariable(inputType, inputVar, "Color.black");
            }

            context.AddLine($"var {rVar} = {inputVar}.r;");
            context.AddLine($"var {gVar} = {inputVar}.g;");
            context.AddLine($"var {bVar} = {inputVar}.b;");
            context.AddLine($"var {aVar} = {inputVar}.a;");

            // Register output variables
            context.RegisterOutputVariable(_rPort, rVar);
            context.RegisterOutputVariable(_gPort, gVar);
            context.RegisterOutputVariable(_bPort, bVar);
            context.RegisterOutputVariable(_aPort, aVar);
        }

        public override void GenerateDataFields(ICodeGenContext context, string nodeId)
        {
            // Split doesn't need cached data - simple component extraction
        }

        public override void GenerateDataInitialization(ICodeGenContext context, string nodeId)
        {
            // No initialization needed for split
        }
    }
}