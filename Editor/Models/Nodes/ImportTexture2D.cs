using UnityEngine;

namespace Misaki.TextureMaker
{
    internal class ImportTexture2D : TextureExecutableNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<Texture2D>("Input").Build();
            context.AddOutputPort<Color>("Output").Build();
        }

        public override void Execute(Vector2 uv)
        {
            var input = GetInputPortValue<Texture2D>("Input");
            if (input == null)
            {
                Debug.LogWarning("Input texture is null.");
                return;
            }

            if (!input.isReadable)
            {
                Debug.LogWarning($"Input texture '{input.name}' is not readable. Please enable 'Read/Write Enabled' in the texture import settings.");
                return;
            }

            SetPortValue("Output", input.GetPixelBilinear(uv.x, uv.y));
        }
    }
}