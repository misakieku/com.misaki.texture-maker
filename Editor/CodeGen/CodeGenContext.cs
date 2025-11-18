using System;
using System.Collections.Generic;
using Unity.GraphToolkit.Editor;

namespace Misaki.TextureMaker
{
    internal enum BuiltInVariable
    {
        DispatchThreadID,
        GroupID,
        GroupIndex,
        GroupThreadID,
        PixelCoordinate,
        UV,
    }

    internal class CodeGenContext : ICodeGenContext
    {
        private readonly List<Instruction> _instructionSet;

        public IReadOnlyList<Instruction> InstructionSet => _instructionSet;

        public CodeGenContext()
        {
            _instructionSet = new List<Instruction>();
        }

        public string GetBuiltInVariableName(BuiltInVariable var)
        {
            return var switch
            {
                BuiltInVariable.DispatchThreadID => "dispatchThreadID",
                BuiltInVariable.GroupID => "groupID",
                BuiltInVariable.GroupIndex => "groupIndex",
                BuiltInVariable.GroupThreadID => "groupThreadID",
                BuiltInVariable.PixelCoordinate => "pixelCoordinate",
                BuiltInVariable.UV => "uv",
                _ => throw new ArgumentOutOfRangeException(nameof(var), var, null)
            };
        }

        public string GetInputVariableName<T>(IPort port, Func<T, Expression> fallback)
        {
            if (port.Direction != PortDirection.Input)
            {
                throw new ArgumentException("Port must be an input port", nameof(port));
            }

            return CodeGenUtility.GetInputVariableName(port, this, fallback);
        }

        public string GetInputVariableName(IPort port, ShaderVariableType variableType, Func<object, Expression> fallback)
        {
            if (port.Direction != PortDirection.Input)
            {
                throw new ArgumentException("Port must be an input port", nameof(port));
            }

            return CodeGenUtility.GetInputVariableName(port, variableType, this, fallback);
        }

        public string GetOutputVariableName(IPort port)
        {
            if (port.Direction != PortDirection.Output)
            {
                throw new ArgumentException("Port must be an output port", nameof(port));
            }

            return CodeGenUtility.GetUniqueVariableName(port);
        }

        public void AddInstruction(Instruction instruction)
        {
            _instructionSet.Add(instruction);
        }
    }

    internal class SubGraphCodeGenContext : ICodeGenContext
    {
        private readonly List<Instruction> _instructionSet;
        private readonly Dictionary<IVariable, string> _inputArgs;
        private readonly Dictionary<IVariable, string> _outputArgs;

        public IReadOnlyList<Instruction> InstructionSet => _instructionSet;

        public SubGraphCodeGenContext(Dictionary<IVariable, string> inputArgs, Dictionary<IVariable, string> outoutArgs)
        {
            _instructionSet = new();
            _inputArgs = inputArgs;
            _outputArgs = outoutArgs;
        }

        public string GetBuiltInVariableName(BuiltInVariable var)
        {
            //return var switch
            //{
            //    BuiltInVariable.DispatchThreadID => "dispatchThreadID",
            //    BuiltInVariable.GroupID => "groupID",
            //    BuiltInVariable.GroupIndex => "groupIndex",
            //    BuiltInVariable.GroupThreadID => "groupThreadID",
            //    BuiltInVariable.PixelCoordinate => "pixelCoordinate",
            //    BuiltInVariable.UV => "uv",
            //    _ => throw new ArgumentOutOfRangeException(nameof(var), var, null)
            //};

            // TODO: Support built-in variables in sub-graphs.
            throw new NotSupportedException("Built-in variables are not supported in sub-graphs.");
        }

        public string GetInputVariableName<T>(IPort port, Func<T, Expression> fallback)
        {
            if (port.Direction != PortDirection.Input)
            {
                throw new ArgumentException("Port must be an input port", nameof(port));
            }

            if (port.FirstConnectedPort?.GetNode() is IVariableNode variableNode)
            {
                if (_inputArgs.TryGetValue(variableNode.Variable, out var connectedVariableName))
                {
                    return connectedVariableName;
                }
            }

            return CodeGenUtility.GetInputVariableName(port, this, fallback);
        }

        public string GetInputVariableName(IPort port, ShaderVariableType variableType, Func<object, Expression> fallback)
        {
            if (port.Direction != PortDirection.Input)
            {
                throw new ArgumentException("Port must be an input port", nameof(port));
            }

            if (port.FirstConnectedPort?.GetNode() is IVariableNode variableNode)
            {
                if (_inputArgs.TryGetValue(variableNode.Variable, out var connectedVariableName))
                {
                    return connectedVariableName;
                }
            }

            return CodeGenUtility.GetInputVariableName(port, variableType, this, fallback);
        }

        public string GetOutputVariableName(IPort port)
        {
            if (port.Direction != PortDirection.Output)
            {
                throw new ArgumentException("Port must be an output port", nameof(port));
            }

            return CodeGenUtility.GetUniqueVariableName(port);
        }

        public void AddInstruction(Instruction instruction)
        {
            _instructionSet.Add(instruction);
        }
    }
}