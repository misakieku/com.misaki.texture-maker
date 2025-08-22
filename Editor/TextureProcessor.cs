using System;
using static Unity.Burst.Intrinsics.X86;

namespace Misaki.TextureMaker
{
    internal unsafe class TextureProcessor : ITextureProcessor
    {
        private TextureData _textureData;

        public TextureProcessor(TextureData textureData)
        {
            _textureData = textureData;
        }

        public void Brightness(float value)
        {
            var vMultiplyer = Sse.set1_ps(value);
            for (var y = 0; y < _textureData.Height; y++)
            {
                for (var x = 0; x < _textureData.Width; x++)
                {
                    var pixel = _textureData.GetPixel(x, y);
                    var result = Sse.mul_ps(pixel, vMultiplyer);
                    result.Float3 = pixel.Float3;
                    _textureData.SetPixel(x, y, result);
                }
            }
        }

        public void Shuffle(int x, int y, int z, int w)
        {
            var imm8 = (x & 0x3) | (((y & 0x3) << 2) | ((z & 0x3) << 4) | ((w & 0x3) << 6));
            for (var i = 0; i < _textureData.Height; i++)
            {
                for (var j = 0; j < _textureData.Width; j++)
                {
                    var pixel = _textureData.GetPixel(j, i);
                    var result = Sse.shuffle_ps(pixel, pixel, imm8);
                    _textureData.SetPixel(j, i, result);
                }
            }
        }

        public unsafe void Resize(int width, int height, bool resampling)
        {
            if (width <= 0 || height <= 0)
            {
                throw new ArgumentException("Width and height must be greater than zero.");
            }

            if (_textureData.Width == width && _textureData.Height == height)
            {
                return;
            }

            var newTexData = new TextureData(width, height, _textureData.Format);
            var oldTexData = _textureData;

            var pNewData = newTexData.GetUnsafePtr();
            var pOldData = oldTexData.GetUnsafePtr();

            if (!resampling)
            {
                var minWidth = Math.Min(oldTexData.Width, width);
                var minHeight = Math.Min(oldTexData.Height, height);
                for (var y = 0; y < minHeight; y++)
                {
                    for (var x = 0; x < minWidth; x++)
                    {
                        var oldIndex = y * oldTexData.Width + x;
                        var newIndex = y * width + x;

                        pNewData[newIndex] = pOldData[oldIndex];
                    }
                }
            }
            else
            {
                var x_ratio = (oldTexData.Width - 1.0f) / (width - 1.0f);
                var y_ratio = (oldTexData.Height - 1.0f) / (height - 1.0f);

                var useSIMD = Sse.IsSseSupported;

                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        var srcX = x * x_ratio;
                        var srcY = y * y_ratio;

                        var x1 = (int)srcX;
                        var y1 = (int)srcY;
                        var x2 = Math.Min(x1 + 1, oldTexData.Width - 1);
                        var y2 = Math.Min(y1 + 1, oldTexData.Height - 1);

                        var pA = Sse.loadu_ps(pOldData + (y1 * oldTexData.Width + x1));
                        var pB = Sse.loadu_ps(pOldData + (y1 * oldTexData.Width + x2));
                        var pC = Sse.loadu_ps(pOldData + (y2 * oldTexData.Width + x1));
                        var pD = Sse.loadu_ps(pOldData + (y2 * oldTexData.Width + x2));

                        var xf = srcX - x1;
                        var yf = srcY - y1;

                        var vTop = V128Utility.Lerp(pA, pB, xf);
                        var vBottom = V128Utility.Lerp(pC, pD, xf);

                        var finalColor = V128Utility.Lerp(vTop, vBottom, yf);

                        pNewData[y * width + x] = *(float*)&finalColor;
                    }
                }
            }

            _textureData = newTexData;
            oldTexData.Dispose();
        }

        public void Contrast(float value)
        {
            var vContrast = Sse.set1_ps(value);
            var vMid = Sse.set1_ps(0.5f);
            for (var y = 0; y < _textureData.Height; y++)
            {
                for (var x = 0; x < _textureData.Width; x++)
                {
                    var pixel = _textureData.GetPixel(x, y);
                    var result = Sse.add_ps(Sse.mul_ps(Sse.sub_ps(pixel, vMid), vContrast), vMid);
                    _textureData.SetPixel(x, y, result);
                }
            }
        }

        public void Saturation(float value)
        {
            throw new NotImplementedException();
        }

        public void Gamma(float value)
        {
            throw new NotImplementedException();
        }

        public void HueShift(float degrees)
        {
            throw new NotImplementedException();
        }

        public void Invert()
        {
            throw new NotImplementedException();
        }

        public void Posterize(int levels)
        {
            throw new NotImplementedException();
        }

        public void ColorBalance(float shadows, float midtones, float highlights)
        {
            throw new NotImplementedException();
        }

        public void LevelsAdjust(float blackPoint, float whitePoint, float midPoint)
        {
            throw new NotImplementedException();
        }

        public void GaussianBlur(float radius)
        {
            throw new NotImplementedException();
        }

        public void BoxBlur(int radius)
        {
            throw new NotImplementedException();
        }

        public void Sharpen(float strength)
        {
            throw new NotImplementedException();
        }

        public void UnsharpMask(float amount, float radius, float threshold)
        {
            throw new NotImplementedException();
        }

        public void EdgeDetect(float threshold)
        {
            throw new NotImplementedException();
        }

        public void Emboss(float strength)
        {
            throw new NotImplementedException();
        }

        public void FlipHorizontal()
        {
            throw new NotImplementedException();
        }

        public void FlipVertical()
        {
            throw new NotImplementedException();
        }

        public void Rotate90()
        {
            throw new NotImplementedException();
        }

        public void Rotate180()
        {
            throw new NotImplementedException();
        }

        public void Rotate270()
        {
            throw new NotImplementedException();
        }

        public void Scale(float scaleX, float scaleY)
        {
            throw new NotImplementedException();
        }

        public void Threshold(float threshold)
        {
            throw new NotImplementedException();
        }

        public void HistogramEqualization()
        {
            throw new NotImplementedException();
        }

        public void NoiseReduction(float strength)
        {
            throw new NotImplementedException();
        }

        public void Dilate(int kernelSize)
        {
            throw new NotImplementedException();
        }

        public void Erode(int kernelSize)
        {
            throw new NotImplementedException();
        }

        public void Grayscale()
        {
            throw new NotImplementedException();
        }

        public void ExtractChannel(int channel)
        {
            throw new NotImplementedException();
        }

        public void ReplaceChannel(int targetChannel, float value)
        {
            throw new NotImplementedException();
        }

        public void Swirl(float angle, float radius)
        {
            throw new NotImplementedException();
        }

        public void Ripple(float amplitude, float frequency)
        {
            throw new NotImplementedException();
        }

        public void Bulge(float strength, float radius)
        {
            throw new NotImplementedException();
        }
    }
}