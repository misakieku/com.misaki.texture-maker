using UnityEngine;
using static Unity.Burst.Intrinsics.X86;

namespace Misaki.TextureMaker
{
    internal class Brightness : TextureExecutableNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<Color>("Input").Build();
            context.AddInputPort<float>("Brightness").WithDefaultValue(1).Build();

            context.AddOutputPort<Color>("Output").Build();
        }

        public override void Execute(Vector2 uv)
        {
            var input = GetInputPortValue<Color>("Input");
            var brightness = GetInputPortValue<float>("Brightness");

            var vInput = input.ToV128();
            var vMultiplyer = Sse.set1_ps(brightness);

            SetPortValue("Output", Sse.mul_ps(vInput, vMultiplyer).ToColor());
        }
    }
}