using System;
using Unity.GraphToolkit.Editor;

namespace Misaki.TextureMaker
{
    internal static class GraphUtility
    {
        public static T GetPortValue<T>(IPort port)
        {
            T value;

            var success = (port.firstConnectedPort?.GetNode()) switch
            {
                IConstantNode constantNode => constantNode.TryGetValue(out value),
                IVariableNode variableNode => variableNode.variable.TryGetDefaultValue(out value),
                _ => port.GetNode() switch
                {
                    IConstantNode constantNode => constantNode.TryGetValue(out value),
                    IVariableNode variableNode => variableNode.variable.TryGetDefaultValue(out value),
                    _ => port.TryGetValue(out value),
                }
            };

            if (success)
            {
                return value;
            }

            throw new ArgumentException($"Port '{port.name}' not found or value is not of type {typeof(T).Name}.");
        }

        public static T GetOptionValue<T>(INodeOption option)
        {
            if (option.TryGetValue<T>(out var value))
            {
                return value;
            }

            throw new System.ArgumentException($"Option '{option.name}' not found or value is not of type {typeof(T).Name}.");
        }
    }
}
