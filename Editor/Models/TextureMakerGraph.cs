using Misaki.GraphProcessor.Editor;
using System;
using Unity.GraphToolkit.Editor;
using UnityEditor;

namespace Misaki.TextureMaker
{
    [Graph(EXTENSION, GraphOptions.Default | GraphOptions.SupportsSubgraphs)]
    [Serializable]
    internal class TextureMakerGraph : Graph
    {
        public const string EXTENSION = "texmk";

        private IGraphProcessor _processor;

        [MenuItem("Assets/Create/Texture Maker/Graph Asset", false)]
        private static void CreateAssetFile()
        {
            GraphDatabase.PromptInProjectBrowserToCreateNewAsset<TextureMakerGraph>();
        }

        public override void OnEnable()
        {
            _processor ??= new TextureMakerGraphProcessor(this);
        }

        public void Execute()
        {
            if (_processor == null)
            {
                return;
            }

            _processor.BuildGraph<TextureMakerGraphProcessor.BuildOption>(default);
            _processor.ExecuteGraph();
        }
    }
}