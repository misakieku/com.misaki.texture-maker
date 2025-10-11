using System;
using System.Collections.Generic;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace Misaki.TextureMaker
{
    internal interface ICodeGenContext
    {
        public IReadOnlyList<Instruction> InstructionSet
        {
            get;
        }

        public string GetBuiltInVariableName(BuiltInVariable var);
        public void AddInstruction(Instruction instruction);
    }
}
