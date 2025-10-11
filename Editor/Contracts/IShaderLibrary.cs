using System;
using System.Collections.Generic;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace Misaki.TextureMaker
{
    internal struct ShaderVariableDeclaration
    {
        public VariableDeclaration declaration;
        public Action<ComputeShader, int, string> bindingCallback;

        public readonly override int GetHashCode()
        {
            return declaration.GetHashCode();
        }
    }

    internal enum FunctionFlag
    {
        None = 0,
        Inlineable = 1 << 0,
    }

    internal enum ParameterModifier
    {
        None = 0,
        In = 1 << 0,
        Out = 1 << 1,
        InOut = In | Out,
    }

    internal struct ParameterDeclaration
    {
        public ShaderVariableType type;
        public ParameterModifier modifier;
        public string name;
    }

    internal struct FunctionDeclaration
    {
        public string name;
        public string code;
        public List<ParameterDeclaration> signature;
        public ShaderVariableType returnType;
        public FunctionFlag flags;
    }

    /// <summary>
    /// Represents a collection of shader code elements, including includes, definitions, variables, and functions, for
    ///     use in shader generation or composition.
    /// </summary>
    interface IShaderLibrary
    {
        public IReadOnlyCollection<string> Includes
        {
            get;
        }

        public IReadOnlyCollection<string> Definitions
        {
            get;
        }

        public IReadOnlyCollection<ShaderVariableDeclaration> Variables
        {
            get;
        }

        public IReadOnlyCollection<FunctionDeclaration> Functions
        {
            get;
        }

        public void AddInclude(string include);
        public void AddDefinition(string definition);
        public string AddVariable(ShaderVariableType type, string namePrefix, Action<ComputeShader, int, string> bindingCallback);
        public string AddVariableExactName(ShaderVariableType type, string name, Action<ComputeShader, int, string> bindingCallback);
        public string AddPortVariable(ShaderVariableType type, IPort port);
        public void AddFunction(FunctionDeclaration function);

        public void Clear();
    }
}