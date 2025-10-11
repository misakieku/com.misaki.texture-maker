using Misaki.GraphProcessor.Editor;
using System;
using System.IO;
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

        public override void OnGraphChanged(GraphLogger graphLogger)
        {
            ValidateGraph(graphLogger);
        }

        private bool ValidateGraph(GraphLogger graphLogger)
        {
            foreach (var node in GetNodes())
            {
                if (node is WriteTexture2D writeNode)
                {
                    var path = writeNode.OutputPath;
                    var dir = Path.GetDirectoryName(path);

                    if (!Directory.Exists(dir))
                    {
                        graphLogger?.LogWarning($"Directory does not exist: {dir}", writeNode);
                        return false;
                    }
                }
            }

            return true;
        }

        public void Execute(string path)
        {
            if (!ValidateGraph(null))
            {
                throw new InvalidOperationException("Graph is not valid. Please check the warnings in the console.");
            }

            if (_processor == null)
            {
                return;
            }

            _processor.BuildGraph<TextureMakerGraphProcessor.BuildOption>(new()
            {
                outputName = path
            });
            _processor.ExecuteGraph();
        }
    }
}