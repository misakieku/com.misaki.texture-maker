using Misaki.TextureMaker.CodeGen;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace Misaki.TextureMaker
{
    internal class Combine : TextureExecutableNode
    {
        private IPort _rPort;
        private IPort _gPort;
        private IPort _bPort;
        private IPort _aPort;
        private IPort _outputPort;

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            _rPort = context.AddInputPort<float>("R").Build();
            _gPort = context.AddInputPort<float>("G").Build();
            _bPort = context.AddInputPort<float>("B").Build();
            _aPort = context.AddInputPort<float>("A").Build();

            _outputPort = context.AddOutputPort<Color>("Output").Build();
        }

        public override void GenerateCode(ICodeGenContext context, string nodeId)
        {
            context.AddUsing("UnityEngine");

            var rVar = context.GetInputVariable(_rPort);
            var gVar = context.GetInputVariable(_gPort);
            var bVar = context.GetInputVariable(_bPort);
            var aVar = context.GetInputVariable(_aPort);
            var outputVar = context.GetOutputVariable(_outputPort);

            context.AddLine($"// Combine Node {nodeId}");
            
            // Declare input variables if not connected
            if (!_rPort.isConnected)
                context.DeclareVariable("float", rVar, "0f");
            if (!_gPort.isConnected)
                context.DeclareVariable("float", gVar, "0f");
            if (!_bPort.isConnected)
                context.DeclareVariable("float", bVar, "0f");
            if (!_aPort.isConnected)
                context.DeclareVariable("float", aVar, "1f"); // Alpha defaults to 1

            context.AddLine($"var {outputVar} = new Color({rVar}, {gVar}, {bVar}, {aVar});");

            // Register output variable
            context.RegisterOutputVariable(_outputPort, outputVar);
        }

        public override void GenerateDataFields(ICodeGenContext context, string nodeId)
        {
            // Combine doesn't need cached data - simple component combination
        }

        public override void GenerateDataInitialization(ICodeGenContext context, string nodeId)
        {
            // No initialization needed for combine
        }
    }
}
