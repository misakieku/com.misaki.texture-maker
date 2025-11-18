using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace Misaki.TextureMaker
{
    internal interface ICodeGenerationNode : INode
    {
        /// <summary>
        /// Called once at the start of shader generation.
        /// </summary>
        /// <param name="shaderLibrary">The shader library to register variables and functions.</param>
        public void Initialize(IShaderLibrary shaderLibrary);

        /// <summary>
        /// Called to generate the HLSL code for this node. This can be called multiple times if the node is used in multiple shader kernal.
        /// </summary>
        /// <param name="ctx">The code generation context. Unique per kernal.</param>
        public void GenerateCode(ICodeGenContext ctx);

        /// <summary>
        /// Called after the shader has been executed.
        /// </summary>
        /// <param name="shader">The compute shader that was executed.</param>
        public void Cleanup(ComputeShader shader);
    }

    [Serializable]
    internal abstract class CodeGenerationNode : Node, ICodeGenerationNode
    {
        protected T GetInputPortValue<T>(string portName)
        {
            return GraphUtility.GetPortValue<T>(GetInputPortByName(portName));
        }

        protected T GetOptionValue<T>(string optionName)
        {
            return GraphUtility.GetOptionValue<T>(GetNodeOptionByName(optionName));
        }

        public virtual void Initialize(IShaderLibrary shaderLibrary)
        {
        }

        public abstract void GenerateCode(ICodeGenContext ctx);

        public virtual void Cleanup(ComputeShader shader)
        {
        }
    }
}