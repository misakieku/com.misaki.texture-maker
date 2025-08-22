using Misaki.GraphProcessor.Editor;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace Misaki.TextureMaker
{
    internal interface ITextureExecutable : INode
    {
        /// <summary>
        /// Executes the node with the given UV coordinates.
        /// </summary>
        /// <param name="uv">The UV coordinates to execute the node with.</param>
        /// <remarks>
        /// It's not recommended to pull data from input ports in this method. Use <see cref="PullData"/> instead for better performance.
        /// </remarks>
        public void Execute(Vector2 uv);
    }

    internal abstract class TextureExecutableNode : DataNode, ITextureExecutable
    {
        public abstract void Execute(Vector2 uv);
    }
}