using Misaki.GraphProcessor.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

namespace Misaki.TextureMaker
{
    internal class TextureMakerGraphProcessor : IGraphProcessor
    {
        public struct BuildOption : IBuildOption
        {
            public string outputName;
        }

        private readonly Graph _graph;
        private readonly ISorter _sorter;
        private readonly Dictionary<OutputNode, List<INode>> _processed;
        private readonly HashSet<ICodeGenerationNode> _visitedGenNodes;

        private BuildOption _option;

        public TextureMakerGraphProcessor(Graph graph)
        {
            _graph = graph;
            _sorter = new TopologicalSorter();
            _processed = new();
            _visitedGenNodes = new();
        }

        public void BuildGraph<T>(in T buildOption)
            where T : IBuildOption
        {
            if (buildOption is not BuildOption option)
            {
                throw new ArgumentException($"Invalid build option type: {typeof(T)}");
            }

            _option = option;

            var nodes = _graph.GetNodes().OfType<OutputNode>();

            _processed.Clear();
            _visitedGenNodes.Clear();

            foreach (var node in nodes)
            {
                var processedNodes = _sorter.Sort(node);
                _processed[node] = processedNodes;

                foreach (var n in processedNodes.OfType<ICodeGenerationNode>())
                {
                    _visitedGenNodes.Add(n);
                }
            }
        }

        public void ExecuteGraph()
        {
            ValidateExecutionState();

            var library = new ShaderLibrary();
            library.AddInclude("Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl");

            foreach (var node in _visitedGenNodes)
            {
                node.Initialize(library);
            }

            var compiler = new InstructionCompiler(library);

            PopulateInstructions(library, compiler);

            var code = compiler.Compile();
            if (string.IsNullOrEmpty(code))
            {
                throw new InvalidOperationException("Compile failed.");
            }

            var computeShader = CreateComputeShader(code);

            var index = 0;
            foreach (var kvp in _processed)
            {
                DispatchShaderKernel(computeShader, index, kvp.Key, library);
                index++;
            }

            foreach (var node in _visitedGenNodes)
            {
                node.Cleanup(computeShader);
            }
        }

        private void ValidateExecutionState()
        {
            if (string.IsNullOrEmpty(_option.outputName))
            {
                throw new InvalidOperationException("Output path is not specified.");
            }

            if (_processed.Count == 0 || _visitedGenNodes.Count == 0)
            {
                throw new InvalidOperationException("Graph is not built. Please call BuildGraph() before ExecuteGraph().");
            }
        }

        private void PopulateInstructions(ShaderLibrary library, InstructionCompiler compiler)
        {
            var index = 0;
            foreach (var kvp in _processed)
            {
                var ctx = new CodeGenContext(library);
                foreach (var node in kvp.Value)
                {
                    if (node is ICodeGenerationNode codeGenNode)
                    {
                        codeGenNode.GenerateCode(ctx);
                    }
                }

                compiler.AddContext(ctx, index++);
            }
        }

        private ComputeShader CreateComputeShader(string code)
        {
            const string outputDir = "Assets/Editor/TextureMaker/Generated/";
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            var hlslPath = Path.Combine(outputDir, _option.outputName + ".compute");
            File.WriteAllText(hlslPath, code);
            AssetDatabase.ImportAsset(hlslPath);
            AssetDatabase.Refresh();

            return AssetDatabase.LoadAssetAtPath<ComputeShader>(hlslPath);
        }

        private void DispatchShaderKernel(ComputeShader computeShader, int kernelIndex, OutputNode outputNode, IShaderLibrary library)
        {
            var width = outputNode.Width;
            var height = outputNode.Height;

            computeShader.SetVector("textureSize", new Vector4(width, height, 1.0f / width, 1.0f / height));

            foreach (var variable in library.Variables)
            {
                variable.bindingCallback?.Invoke(computeShader, kernelIndex, variable.declaration.name);
            }

            // Dispatch the compute shader
            var threadGroupsX = (width + InstructionCompiler.threadGroupSize.x - 1) / InstructionCompiler.threadGroupSize.x;
            var threadGroupsY = (height + InstructionCompiler.threadGroupSize.y - 1) / InstructionCompiler.threadGroupSize.y;
            computeShader.Dispatch(kernelIndex, threadGroupsX, threadGroupsY, InstructionCompiler.threadGroupSize.z);
        }
    }
}