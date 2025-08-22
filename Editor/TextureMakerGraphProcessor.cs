using Misaki.GraphProcessor.Editor;
using Misaki.TextureMaker.CodeGen;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace Misaki.TextureMaker
{
    internal class TextureMakerGraphProcessor : IGraphProcessor
    {
        public struct BuildOption : IBuildOption
        {
        }

        private readonly Graph _graph;
        private readonly Dictionary<OutputNode, List<ITextureExecutable>> _processed;
        private readonly List<IPort> _portsPool;

        public TextureMakerGraphProcessor(Graph graph)
        {
            _graph = graph;
            _processed = new();
            _portsPool = new();
        }

        private IEnumerable<INode> GetDependentNodes(INode node)
        {
            var ports = node.GetInputPorts();

            if (ports == null)
            {
                yield break;
            }

            foreach (var port in ports)
            {
                if (!port.isConnected)
                {
                    continue;
                }

                port.GetConnectedPorts(_portsPool);
                foreach (var connectedPort in _portsPool)
                {
                    yield return connectedPort.GetNode();
                }
            }
        }

        private void ProcessTopologicalOrder(OutputNode masterNode, List<ITextureExecutable> nodeContainer)
        {
            var visited = new HashSet<ITextureExecutable>();
            var visiting = new HashSet<ITextureExecutable>();

            // Need manual traversal state tracking in a iterative dfs
            var stack = new Stack<(ITextureExecutable node, bool isPostProcessing)>();

            stack.Push((masterNode, false));

            while (stack.Count > 0)
            {
                var (currentNode, isPostProcessing) = stack.Pop();

                if (isPostProcessing)
                {
                    // Post-processing: add to processed list and mark as visited
                    visiting.Remove(currentNode);
                    visited.Add(currentNode);

                    nodeContainer.Add(currentNode);
                }
                else
                {
                    // Pre-processing: check for cycles and add dependencies
                    if (visiting.Contains(currentNode))
                    {
                        throw new System.InvalidOperationException($"Circular dependency detected in graph involving node: {currentNode}");
                    }

                    if (visited.Contains(currentNode))
                    {
                        continue;
                    }

                    visiting.Add(currentNode);

                    // Push post-processing entry for this node
                    stack.Push((currentNode, true));

                    INode[] dependencies;
                    if (currentNode is ICustomDependency dependent)
                    {
                        dependencies = dependent.GetDependentNodes(GraphFlow.Backward);
                    }
                    else
                    {
                        dependencies = GetDependentNodes(currentNode).ToArray();
                    }

                    // Push all dependencies for pre-processing (in reverse order to maintain proper traversal order)
                    for (var i = dependencies.Length - 1; i >= 0; i--)
                    {
                        var dependency = dependencies[i];
                        if (!visited.Contains(dependency) && dependency is ITextureExecutable executable)
                        {
                            stack.Push((executable, false));
                        }
                    }
                }
            }
        }

        public void BuildGraph<T>(in T buildOption)
            where T : IBuildOption
        {
            var nodes = _graph.GetNodes().ToArray();

            _processed.Clear();
            foreach (var node in nodes.Where(n => n is OutputNode))
            {
                var processedNodes = new List<ITextureExecutable>();
                ProcessTopologicalOrder((OutputNode)node, processedNodes);
                _processed[(OutputNode)node] = processedNodes;
            }
        }

        public void ExecuteGraph()
        {
            foreach (var kvp in _processed)
            {
                var outputNode = kvp.Key;

                try
                {
                    // Generate optimized execution code
                    var codeGenerator = new TextureCodeGenerator();
                    var generatedCode = codeGenerator.GenerateExecutionCode(kvp.Value);

                    // TODO: Implement runtime compilation and execution
                    // For now, we just generate the code for inspection
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        /// <summary>
        /// Generate code for debugging without executing the graph
        /// </summary>
        public void GenerateCodeOnly()
        {
            foreach (var kvp in _processed)
            {
                var codeGenerator = new TextureCodeGenerator();
                var generatedCode = codeGenerator.GenerateExecutionCode(kvp.Value);

                Debug.Log($"Generated code for output node {kvp.Key}:");
                Debug.Log(generatedCode);
            }
        }
    }
}