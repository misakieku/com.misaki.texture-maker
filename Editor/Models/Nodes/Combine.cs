using Misaki.GraphProcessor.Editor;
using UnityEngine;

namespace Misaki.TextureMaker
{
    internal class Combine : ExecutableNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<float>("R").Build();
            context.AddInputPort<float>("G").Build();
            context.AddInputPort<float>("B").Build();
            context.AddInputPort<float>("A").Build();

            context.AddOutputPort<Color>("Output").Build();
        }

        public override void Execute()
        {
            var r = GetInputPortValue<float>("R");
            var g = GetInputPortValue<float>("G");
            var b = GetInputPortValue<float>("B");
            var a = GetInputPortValue<float>("A");

            SetPortValue("Output", new Color(r, g, b, a));
        }
    }
}
