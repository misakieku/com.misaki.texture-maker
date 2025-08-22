using Misaki.GraphProcessor.Editor;
using System;
using System.Runtime.CompilerServices;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86;

namespace Misaki.TextureMaker
{
    internal enum SupportedTextureFormat
    {
        R8 = TextureFormat.R8,
        Alpha8 = TextureFormat.Alpha8,
        RGB24 = TextureFormat.RGB24,
        RGBA32 = TextureFormat.RGBA32,
        RGBAFloat = TextureFormat.RGBAFloat
    }

    internal unsafe sealed class TextureData : IBranchUniqueData, IDisposable
    {
        private NativeArray<float> _data;
        private readonly int _width;
        private readonly int _height;
        private readonly int _channelCount;
        private readonly SupportedTextureFormat _format;

        private bool _disposed;

        public int Width => _width;
        public int Height => _height;
        public int ChannelCount => _channelCount;
        public SupportedTextureFormat Format => _format;
        public TextureFormat TextureFormat => (TextureFormat)_format;
        public NativeArray<float> Data => _data;

        public TextureData(int width, int height, SupportedTextureFormat format)
        {
            GetChannelCount(format, out _channelCount);

            _data = new NativeArray<float>(width * height * _channelCount, Allocator.Persistent);
            _width = width;
            _height = height;
            _format = format;
        }

        public TextureData(Texture2D texture)
        {
            var format = (SupportedTextureFormat)texture.format;
            GetChannelCount(format, out _channelCount);

            _width = texture.width;
            _height = texture.height;
            _format = format;

            ConvertToFloat(texture, out _data);
        }

        private static void ConvertToFloat(Texture2D texture, out NativeArray<float> result)
        {
            switch (texture.format)
            {
                case TextureFormat.R8:
                case TextureFormat.Alpha8:
                case TextureFormat.RGB24:
                case TextureFormat.RGBA32:
                    var byteData = texture.GetPixelData<byte>(0);
                    result = new NativeArray<float>(byteData.Length, Allocator.Persistent);
                    for (var i = 0; i < byteData.Length; i++)
                    {
                        result[i] = byteData[i] * 0.00392156862745098f; // Convert byte to float (0-255 to 0.0-1.0)
                    }
                    break;

                case TextureFormat.RGBAFloat:
                    var floatData = texture.GetPixelData<float>(0);
                    result = new NativeArray<float>(floatData.Length, Allocator.Persistent);
                    floatData.CopyTo(result);
                    break;
                default:
                    throw new NotSupportedException($"Texture format '{texture.format}' is not supported.");
            }
        }

        private static void GetChannelCount(SupportedTextureFormat format, out int count)
        {
            count = format switch
            {
                SupportedTextureFormat.R8 or SupportedTextureFormat.Alpha8 => 1,
                SupportedTextureFormat.RGB24 => 3,
                SupportedTextureFormat.RGBA32 => 4,
                SupportedTextureFormat.RGBAFloat => 4,
                _ => throw new NotSupportedException($"Texture format '{format}' is not supported."),
            };
        }

        ~TextureData()
        {
            Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(int x, int y)
        {
            return (y * _width + x) * _channelCount;
        }

        public object MakeUniqueForWrite()
        {
            var newData = new TextureData(_width, _height, _format);

            _data.CopyTo(newData._data);
            return newData;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPixel(int x, int y, v128 value)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                throw new ArgumentOutOfRangeException($"Coordinates ({x}, {y}) are out of bounds for texture size {Width}x{Height}.");
            }

            var index = GetIndex(x, y);
            var pixelSize = _channelCount * sizeof(float);
            UnsafeUtility.MemCpy(GetUnsafePtr() + index, &value, pixelSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public v128 GetPixel(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                throw new ArgumentOutOfRangeException($"Coordinates ({x}, {y}) are out of bounds for texture size {Width}x{Height}.");
            }

            var index = GetIndex(x, y);
            var pixelSize = _channelCount * sizeof(float);
            var pSrc = GetUnsafePtr() + index;
            var temp = stackalloc float[4];

            UnsafeUtility.MemClear(temp, 4 * sizeof(float));
            UnsafeUtility.MemCpy(temp, pSrc, pixelSize);

            return Sse.loadu_ps(temp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Process(Action<ITextureProcessor> processAction)
        {
            processAction(new TextureProcessor(this));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float* GetUnsafePtr()
        {
            return (float*)_data.GetUnsafePtr();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float PackBytes(byte a, byte b, byte c, byte d)
        {
            return a | (b << 8) | (c << 16) | (d << 24);
        }

        public NativeArray<float> Convert(TextureFormat format, Allocator allocator)
        {
            switch (format)
            {
                case TextureFormat.R8:
                case TextureFormat.Alpha8:
                case TextureFormat.RGB24:
                case TextureFormat.RGBA32:
                    // For byte formats, we need to convert float data back to bytes
                    // Unity expects the raw byte data as a float array for SetPixelData
                    var byteResult = new NativeArray<float>(_data.Length / 4 + (_data.Length % 4 > 0 ? 1 : 0), allocator);
                    var bytes = stackalloc byte[4];
                    for (var i = 0; i < _data.Length; i += 4)
                    {
                        for (var j = 0; j < 4; j++)
                        {
                            if (i + j < _data.Length)
                            {
                                bytes[j] = (byte)(Mathf.Clamp01(_data[i + j]) * 255);
                            }
                            else
                            {
                                bytes[j] = 0;
                            }
                        }

                        byteResult[i / 4] = *(float*)bytes;
                    }

                    return byteResult;

                case TextureFormat.RGBAFloat:
                    var floatResult = new NativeArray<float>(_data.Length, allocator);
                    _data.CopyTo(floatResult);
                    return floatResult;

                default:
                    throw new NotSupportedException($"Texture format '{format}' is not supported.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _data.Dispose();

            _disposed = true;
        }
    }
}