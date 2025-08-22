using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Misaki.TextureMaker
{
    internal class ExportTexture2d : OutputNode
    {
        private Texture2D _outputTexture;

        protected override void OnDefineInputPorts(IPortDefinitionContext context)
        {
            context.AddInputPort<Color>("Input").Build();
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<SupportedTextureFormat>("Format").WithDefaultValue(SupportedTextureFormat.RGBA32).Build();
            context.AddOption<string>("Output Path").WithDefaultValue("Assets/Output.png").ShowInInspectorOnly().Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsValidPath(string path)
        {
            return !string.IsNullOrEmpty(path) && (path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".jpeg"));
        }

        public override void Initialize()
        {
            var width = GetInputPortValue<int>(WIDTH_PORT_NAME);
            var height = GetInputPortValue<int>(HEIGHT_PORT_NAME);
            var format = GetOptionValue<SupportedTextureFormat>("Format");

            var path = GetOptionValue<string>("Output Path");
            if (!IsValidPath(path))
            {
                throw new System.ArgumentException($"Invalid output path '{path}'. Supported formats are .png, .jpg, and .jpeg.");
            }

            _outputTexture = new Texture2D(width, height, (TextureFormat)format, false);
        }

        public override void Execute(Vector2 uv)
        {
            var input = GetInputPortValue<Color>("Input");

            var pos = GraphUtility.GetTexturePixelPosition(_outputTexture, uv);
            _outputTexture.SetPixel(pos.x, pos.y, input);
        }

        public override void Complete()
        {
            var path = GetOptionValue<string>("Output Path");

            _outputTexture.Apply();

            var extension = System.IO.Path.GetExtension(path).ToLowerInvariant();
            switch (extension)
            {
                case ".png":
                    System.IO.File.WriteAllBytes(path, _outputTexture.EncodeToPNG());
                    break;
                case ".jpg":
                case ".jpeg":
                    System.IO.File.WriteAllBytes(path, _outputTexture.EncodeToJPG());
                    break;
                default:
                    Debug.LogWarning($"Unsupported output format '{extension}'. Only .png, .jpg, and .jpeg are supported.");
                    goto CleanGraphResource;
            }

            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();

        CleanGraphResource:
            Object.DestroyImmediate(_outputTexture);
            _outputTexture = null;
        }
    }
}