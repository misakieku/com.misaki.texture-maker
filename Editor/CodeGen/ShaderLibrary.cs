using Misaki.GraphProcessor.Editor;
using System;
using System.Collections.Generic;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace Misaki.TextureMaker
{
    internal class ShaderLibrary : IShaderLibrary
    {
        private readonly HashSet<string> _includedFiles = new();
        private readonly HashSet<string> _definitions = new();
        private readonly HashSet<ShaderVariableDeclaration> _variables = new();
        private readonly HashSet<FunctionDeclaration> _functions = new();

        private uint _variableCounter = 0;

        public IReadOnlyCollection<string> Includes => _includedFiles;
        public IReadOnlyCollection<string> Definitions => _definitions;
        public IReadOnlyCollection<ShaderVariableDeclaration> Variables => _variables;
        public IReadOnlyCollection<FunctionDeclaration> Functions => _functions;

        public void AddDefinition(string definition)
        {
            _definitions.Add(definition);
        }

        public void AddInclude(string include)
        {
            _includedFiles.Add(include);
        }

        public string AddVariable(ShaderVariableType type, string namePrefix, Action<ComputeShader, int, string> bindingCallback)
        {
            var name = $"{namePrefix}_var{_variableCounter++}";
            _variables.Add(new ShaderVariableDeclaration
            {
                declaration = new VariableDeclaration
                {
                    type = type,
                    name = name
                },

                bindingCallback = bindingCallback
            });

            return name;
        }

        public string AddVariableExactName(ShaderVariableType type, string name, Action<ComputeShader, int, string> bindingCallback)
        {
            _variables.Add(new ShaderVariableDeclaration
            {
                declaration = new VariableDeclaration
                {
                    type = type,
                    name = name
                },
                bindingCallback = bindingCallback
            });

            return name;
        }

        public string AddPortVariable(ShaderVariableType type, IPort port)
        {
            var node = port.GetNode();
            if (node is not DataNode dataNode)
            {
                throw new InvalidOperationException("Port's node is not a DataNode");
            }

            var name = CodeGenUtility.GetUniqueVariableName(port);
            return AddVariableExactName(type, name, (shader, index, name) =>
            {
                switch (type)
                {
                    case ShaderVariableType.Float:
                        var floatValue = dataNode.GetInputPortValue<float>(port.name);
                        shader.SetFloat(name, floatValue);
                        break;
                    case ShaderVariableType.Int:
                        var intValue = dataNode.GetInputPortValue<int>(port.name);
                        shader.SetInt(name, intValue);
                        break;
                    case ShaderVariableType.UInt:
                        var uintValue = dataNode.GetInputPortValue<uint>(port.name);
                        shader.SetInt(name, (int)uintValue);
                        break;
                    case ShaderVariableType.Float2:
                    case ShaderVariableType.Float3:
                    case ShaderVariableType.Float4:
                    case ShaderVariableType.Int2:
                    case ShaderVariableType.Int3:
                    case ShaderVariableType.Int4:
                    case ShaderVariableType.UInt2:
                    case ShaderVariableType.UInt3:
                    case ShaderVariableType.UInt4:
                        var vector = dataNode.GetInputPortValue<Vector4>(port.name);
                        shader.SetVector(name, vector);
                        break;
                    case ShaderVariableType.RWTexture2D:
                    case ShaderVariableType.Texture2D:
                        var texture = dataNode.GetInputPortValue<Texture2D>(port.name);
                        shader.SetTexture(index, name, texture);
                        break;
                    default:
                        break;
                }
            });
        }

        public void AddFunction(FunctionDeclaration function)
        {
            _functions.Add(function);
        }

        public void Clear()
        {
            _includedFiles.Clear();
            _definitions.Clear();
            _variables.Clear();
            _functions.Clear();

            _variableCounter = 0;
        }
    }
}