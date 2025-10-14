using System;
using System.Collections.Generic;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace Misaki.TextureMaker
{
    internal struct ShaderVariableDeclaration : IEquatable<ShaderVariableDeclaration>
    {
        public VariableDeclaration declaration;
        public Action<ComputeShader, int, string> bindingCallback;

        public readonly bool Equals(ShaderVariableDeclaration other)
        {
            return declaration.Equals(other.declaration);
        }

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

    internal struct ParameterDeclaration : IEquatable<ParameterDeclaration>
    {
        public ShaderVariableType type;
        public ParameterModifier modifier;
        public string name;

        public readonly bool Equals(ParameterDeclaration other)
        {
            return type == other.type && modifier == other.modifier && name == other.name;
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(type, modifier, name);
        }
    }

    internal struct FunctionDeclaration : IEquatable<FunctionDeclaration>
    {
        public string name;
        public string code;
        public List<ParameterDeclaration> signature;
        public ShaderVariableType returnType;
        public FunctionFlag flags;

        public readonly bool Equals(FunctionDeclaration other)
        {
            if (name == other.name)
            {
                if (signature == null || other.signature == null)
                {
                    return signature == other.signature;
                }

                if (signature.Count == other.signature.Count)
                {
                    var sigEq = true;
                    for (int i = 0; i < signature.Count; i++)
                    {
                        if (!signature[i].Equals(other.signature[i]))
                        {
                            sigEq = false;
                            break;
                        }
                    }

                    return sigEq;
                }
            }

            return false;
        }

        public readonly override int GetHashCode()
        {
            var nameHash = name.GetHashCode();
            var signatureHash = 0;

            if (signature != null)
            {
                foreach (var param in signature)
                {
                    signatureHash = HashCode.Combine(signatureHash, param.GetHashCode());
                }
            }

            return HashCode.Combine(nameHash, signatureHash);
        }
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
        public bool HasInclude(string include);
        public void AddDefinition(string definition);
        public bool HasDefinition(string definition);
        public string AddVariable(ShaderVariableType type, string namePrefix, Action<ComputeShader, int, string> bindingCallback);
        public string AddVariableExactName(ShaderVariableType type, string name, Action<ComputeShader, int, string> bindingCallback);
        public string AddVariable(ShaderVariableType type, IPort port);
        public bool HasVariable(ShaderVariableType type, string name);
        public void AddFunction(FunctionDeclaration function);
        public bool HasFunction(string name);

        public void Clear();
    }
}