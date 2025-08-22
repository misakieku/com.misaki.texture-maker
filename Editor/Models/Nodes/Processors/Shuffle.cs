using UnityEngine;
using static Unity.Burst.Intrinsics.X86;

namespace Misaki.TextureMaker
{
    internal class Shuffle : TextureExecutableNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<Color>("Input").Build();

            context.AddOutputPort<Color>("Output").Build();
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<TextureChannel>("R Channel").WithDefaultValue(TextureChannel.R).Build();
            context.AddOption<TextureChannel>("G Channel").WithDefaultValue(TextureChannel.G).Build();
            context.AddOption<TextureChannel>("B Channel").WithDefaultValue(TextureChannel.B).Build();
            context.AddOption<TextureChannel>("A Channel").WithDefaultValue(TextureChannel.A).Build();
        }

        public override void Execute(Vector2 uv)
        {
            var input = GetInputPortValue<Color>("Input");

            var rChannel = GetOptionValue<TextureChannel>("R Channel");
            var gChannel = GetOptionValue<TextureChannel>("G Channel");
            var bChannel = GetOptionValue<TextureChannel>("B Channel");
            var aChannel = GetOptionValue<TextureChannel>("A Channel");

            var vInput = input.ToV128();
            var imm8 = ((int)rChannel & 0x3) | ((((int)gChannel & 0x3) << 2) | (((int)bChannel & 0x3) << 4) | (((int)aChannel & 0x3) << 6));

            SetPortValue("Output", Sse.shuffle_ps(vInput, vInput, imm8).ToColor());
        }
    }
}