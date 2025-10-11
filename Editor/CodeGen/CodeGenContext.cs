using System;
using System.Collections.Generic;

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

        public CodeGenContext(IShaderLibrary shaderLibrary)
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

        public void AddInstruction(Instruction instruction)
        {
            _instructionSet.Add(instruction);
        }
    }
}