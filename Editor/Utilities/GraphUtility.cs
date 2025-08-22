using System.Runtime.CompilerServices;
using UnityEngine;

namespace Misaki.TextureMaker
{
    internal static class GraphUtility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int GetTexturePixelPosition(int width, int height, Vector2 uv)
        {
            var x = Mathf.RoundToInt(uv.x * (width - 1));
            var y = Mathf.RoundToInt(uv.y * (height - 1));

            x = Mathf.Clamp(x, 0, width - 1);
            y = Mathf.Clamp(y, 0, height - 1);
            return new Vector2Int(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int GetTexturePixelPosition(Texture texture, Vector2 uv)
        {
            if (texture == null)
            {
                throw new System.ArgumentNullException(nameof(texture), "Texture cannot be null.");
            }

            return GetTexturePixelPosition(texture.width, texture.height, uv);
        }
    }
}