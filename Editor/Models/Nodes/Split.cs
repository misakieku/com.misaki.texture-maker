using Misaki.GraphProcessor.Editor;
using UnityEngine;

namespace Misaki.TextureMaker
{
    internal unsafe class Split : ExecutableNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<Color>("Input").Build();

            context.AddOutputPort<float>("R").Build();
            context.AddOutputPort<float>("G").Build();
            context.AddOutputPort<float>("B").Build();
            context.AddOutputPort<float>("A").Build();
        }

        public override void Execute()
        {
            var input = GetInputPortValue<Color>("Input");

            SetPortValue("R", input.r);
            SetPortValue("G", input.g);
            SetPortValue("B", input.b);
            SetPortValue("A", input.a);
        }
    }
}