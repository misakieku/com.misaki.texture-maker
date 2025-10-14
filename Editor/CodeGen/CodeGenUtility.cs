using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Unity.GraphToolkit.Editor;
using Unity.Mathematics;

namespace Misaki.TextureMaker
{
    /// <summary>
    /// Provides utility methods for code generation tasks, including generating unique identifiers and variable names
    ///     for nodes and ports, retrieving input variable names, and formatting code with indentation.
    /// </summary>
    internal static class CodeGenUtility
    {
        public static ConstantExpr ToConstantExpr(object data, ShaderVariableType dataType)
        {
            var expr = data switch
            {
                float f when dataType == ShaderVariableType.Float => new ConstantExpr(f.ToString("F")),
                float2 v2 when dataType == ShaderVariableType.Float2 => new ConstantExpr($"float2({v2.x}, {v2.y})"),
                float3 v3 when dataType == ShaderVariableType.Float3 => new ConstantExpr($"float3({v3.x}, {v3.y}, {v3.z})"),
                float4 v4 when dataType == ShaderVariableType.Float4 => new ConstantExpr($"float4({v4.x}, {v4.y}, {v4.z}, {v4.w})"),
                bool b when dataType == ShaderVariableType.Bool => new ConstantExpr(b ? "true" : "false"),
                _ => throw new InvalidOperationException($"Invalid data type {data.GetType()} with PortValueType {dataType}"),
            };

            return expr;
        }

        public static VariableExpr ToVariableExpr(object data, ShaderVariableType dataType)
        {
            var expr = data switch
            {
                float f when dataType == ShaderVariableType.Float => new VariableExpr(f.ToString("F")),
                float2 v2 when dataType == ShaderVariableType.Float2 => new VariableExpr($"float2({v2.x}, {v2.y})"),
                float3 v3 when dataType == ShaderVariableType.Float3 => new VariableExpr($"float3({v3.x}, {v3.y}, {v3.z})"),
                float4 v4 when dataType == ShaderVariableType.Float4 => new VariableExpr($"float4({v4.x}, {v4.y}, {v4.z}, {v4.w})"),
                bool b when dataType == ShaderVariableType.Bool => new VariableExpr(b ? "true" : "false"),
                _ => throw new InvalidOperationException($"Invalid data type {data.GetType()} with PortValueType {dataType}"),
            };

            return expr;
        }

        /// <summary>
        /// Returns a unique identifier for the specified node based on its hash code.
        /// </summary>
        /// <param name="node">The node for which to retrieve the unique identifier. Cannot be null.</param>
        /// <returns>A 32-bit unsigned integer representing the unique identifier of the node.</returns>
        public static uint GetNodeID(INode node)
        {
            var hash = node.GetHashCode();
            return Unsafe.As<int, uint>(ref hash);
        }

        /// <summary>
        /// Generates a function name for the specified subgraph node.
        /// </summary>
        /// <param name="subgraph">The subgraph node for which to generate the function name. Cannot be null.</param>
        /// <returns>A string containing the generated function name for the subgraph node.</returns>
        public static string GetSubGraphFunctionName(ISubgraphNode subgraph)
        {
            return $"Generated_SubGraph_{subgraph.GetSubgraph().name}";
        }

        /// <summary>
        /// Converts a display name to a valid variable name by replacing non-word characters with underscores.
        /// </summary>
        /// <param name="displayName">The display name to convert. Can be any string; if null or empty, an empty string is returned.</param>
        /// <returns>A string representing the variable name derived from the display name, with non-word characters replaced by
        ///     underscores and leading or trailing underscores removed. Returns an empty string if the input is null or empty.</returns>
        public static string DisplayNameToVariableName(string displayName)
        {
            if (string.IsNullOrEmpty(displayName))
            {
                return string.Empty;
            }

            var result = Regex.Replace(displayName, @"\W+", "_", RegexOptions.Compiled);
            return result.Trim('_');
        }

        /// <summary>
        /// Generates a unique variable name for the specified port based on its associated node and port name.
        /// </summary>
        /// <param name="port">The port for which to generate a unique variable name. Cannot be null.</param>
        /// <returns>A string representing a unique variable name for the given port. The name is constructed using the node type, node identifier, and port name.</returns>
        public static string GetUniqueVariableName(IPort port)
        {
            var node = port.GetNode();
            return $"{node.GetType().Name}_{GetNodeID(node)}_{DisplayNameToVariableName(port.displayName)}";
        }

        /// <summary>
        /// Gets the name of the input variable associated with the specified port, using a fallback function if necessary.
        /// </summary>
        /// <param name="port">The port for which to retrieve the input variable name. Cannot be null.</param>
        /// <param name="fallback">A function that provides a variable name when a direct mapping cannot be determined. Cannot be null.</param>
        /// <returns>A string containing the name of the input variable for the specified port.
        ///     If the port is not connected or does not map directly to a variable, the fallback function is used to determine the name.</returns>
        public static string GetInputVariableName(IPort port, Func<IPort, string> fallback)
        {
            var firstConnectedPort = port.firstConnectedPort;
            if (firstConnectedPort == null)
            {
                return fallback(port);
            }

            return firstConnectedPort.GetNode() switch
            {
                IConstantNode or IVariableNode => fallback(firstConnectedPort),
                _ => GetUniqueVariableName(firstConnectedPort),
            };
        }

        /// <summary>
        /// Gets the name of the variable representing the input value for the specified port, generating a new variable if necessary.
        /// </summary>
        /// <typeparam name="T">The type of the input value associated with the port.</typeparam>
        /// <param name="port">The port for which to retrieve the input variable name.</param>
        /// <param name="ctx">The code generation context used to add instructions and manage variable declarations.</param>
        /// <param name="fallback">A function that generates an expression from the input value if a new variable must be created. The function is invoked with the input value of type T.</param>
        /// <returns>The name of the variable representing the input value for the specified port.</returns>
        public static string GetInputVariableName<T>(IPort port, ICodeGenContext ctx, Func<T, Expression> fallback)
        {
            return GetInputVariableName(port, connectedPort =>
            {
                var value = GraphUtility.GetPortValue<T>(connectedPort);
                var varName = GetUniqueVariableName(connectedPort);

                ctx.AddInstruction(new Instruction
                {
                    result = new VariableDeclaration
                    {
                        type = typeof(T) switch
                        {
                            Type t when t == typeof(float) => ShaderVariableType.Float,
                            Type t when t == typeof(float2) => ShaderVariableType.Float2,
                            Type t when t == typeof(float3) => ShaderVariableType.Float3,
                            Type t when t == typeof(float4) => ShaderVariableType.Float4,
                            Type t when t == typeof(int) => ShaderVariableType.Int,
                            Type t when t == typeof(int2) => ShaderVariableType.Int2,
                            Type t when t == typeof(int3) => ShaderVariableType.Int3,
                            Type t when t == typeof(int4) => ShaderVariableType.Int4,
                            Type t when t == typeof(uint) => ShaderVariableType.UInt,
                            Type t when t == typeof(uint2) => ShaderVariableType.UInt2,
                            Type t when t == typeof(uint3) => ShaderVariableType.UInt3,
                            Type t when t == typeof(uint4) => ShaderVariableType.UInt4,
                            _ => throw new InvalidOperationException($"Unsupported type: {typeof(T)}"),
                        },
                        name = varName
                    },
                    expression = fallback(value)
                });

                return varName;
            });
        }

        /// <summary>
        /// Gets the name of the input variable associated with the specified port, generating a variable declaration if
        /// necessary.
        /// </summary>
        /// <param name="port">The input port for which to retrieve the variable name.</param>
        /// <param name="variableType">The type of the shader variable to declare if a new variable is generated.</param>
        /// <param name="ctx">The code generation context used to add instructions and manage variable declarations.</param>
        /// <param name="instrictionCallback">A callback function that receives the input value and returns an expression to be used in the variable declaration.</param>
        /// <returns>The name of the input variable associated with the specified port.</returns>
        public static string GetInputVariableName(IPort port, ShaderVariableType variableType, ICodeGenContext ctx, Func<object, Expression> instrictionCallback)
        {
            return GetInputVariableName(port, connectedPort =>
            {
                var value = GraphUtility.GetPortValue<object>(connectedPort);
                var varName = GetUniqueVariableName(connectedPort);

                ctx.AddInstruction(new Instruction
                {
                    result = new VariableDeclaration
                    {
                        type = variableType,
                        name = varName
                    },
                    expression = instrictionCallback(value)
                });

                return varName;
            });
        }

        /// <summary>
        /// Returns a new string with the specified number of indentation levels applied to the beginning of the input
        /// string.
        /// </summary>
        /// <param name="str">The string to which indentation will be applied.</param>
        /// <param name="IndentLevel">The number of indentation levels to add. Each level consists of four spaces. If 0, no indentation is added.</param>
        /// <returns>A new string with the specified indentation applied. If <paramref name="IndentLevel"/> is 0, the original string is returned.</returns>
        public static string Indent(this string str, int IndentLevel)
        {
            if (IndentLevel == 0)
            {
                return str;
            }

            return new string(' ', IndentLevel * 4) + str;
        }
    }
}