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
        public string GetInputVariableName<T>(IPort port, Func<T, Expression> fallback);
        public string GetInputVariableName(IPort port, ShaderVariableType variableType, Func<object, Expression> fallback);
        public string GetOutputVariableName(IPort port);
        public void AddInstruction(Instruction instruction);
    }
}
