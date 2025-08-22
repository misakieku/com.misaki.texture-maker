using Misaki.GraphProcessor.Editor;
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

        private void PushPortData(INode sourceNode)
        {
            object value = null;

            switch (sourceNode)
            {
                case IConstantNode constantNode:
                    constantNode.TryGetValue<object>(out value);
                    break;
                case IVariableNode variableNode:
                    variableNode.variable.TryGetDefaultValue<object>(out value);
                    break;
                default:
                    // value will be set per port below
                    break;
            }

            foreach (var outputPort in sourceNode.GetOutputPorts())
            {
                if (!outputPort.isConnected)
                    continue;

                // For IPortContainer, get value per port
                var portValue = (sourceNode is IPortValueContainer container)
                    ? container.GetPortValue(outputPort.name)
                    : value;

                var shouldDispose = false;
                outputPort.GetConnectedPorts(_portsPool);
                if (portValue is IBranchUniqueData immutableData)
                {
                    foreach (var connectedPort in _portsPool)
                    {
                        if (connectedPort.GetNode() is IPortValueContainer connectedContainer)
                        {
                            var toSend = _portsPool.Count == 1 ? immutableData : immutableData.MakeUniqueForWrite();
                            connectedContainer.SetPortValue(connectedPort.name, toSend);
                        }
                    }

                    if (_portsPool.Count > 1)
                    {
                        shouldDispose = true;
                    }
                }
                else
                {
                    foreach (var connectedPort in _portsPool)
                    {
                        if (connectedPort.GetNode() is IPortValueContainer connectedContainer)
                        {
                            connectedContainer.SetPortValue(connectedPort.name, portValue);
                        }
                    }
                }

                if ((_portsPool.Count == 0 || shouldDispose) && portValue is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        public void ExecuteGraph()
        {
            foreach (var kvp in _processed)
            {
                var outputNode = kvp.Key;
                var width = outputNode.GetInputPortValue<int>(OutputNode.WIDTH_PORT_NAME);
                var height = outputNode.GetInputPortValue<int>(OutputNode.HEIGHT_PORT_NAME);

                try
                {
                    outputNode.Initialize();

                    for (var y = 0; y < height; y++)
                    {
                        for (var x = 0; x < width; x++)
                        {
                            var uv = new Vector2((float)x / (width - 1), (float)y / (height - 1));

                            foreach (var node in kvp.Value)
                            {
                                node.Execute(uv);
                                PushPortData(node);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
                finally
                {
                    outputNode.Complete();
                }
            }
        }
    }
}