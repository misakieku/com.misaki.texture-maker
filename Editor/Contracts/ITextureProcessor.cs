namespace Misaki.TextureMaker
{
    public interface ITextureProcessor
    {
        // Existing methods
        public void Brightness(float value);
        public void Shuffle(int x, int y, int z, int w);
        public void Resize(int width, int height, bool resampling);
        
        // Color manipulation
        public void Contrast(float value);
        public void Saturation(float value);
        public void Gamma(float value);
        public void HueShift(float degrees);
        public void Invert();
        public void Posterize(int levels);
        public void ColorBalance(float shadows, float midtones, float highlights);
        public void LevelsAdjust(float blackPoint, float whitePoint, float midPoint);
        
        // Filters and effects
        public void GaussianBlur(float radius);
        public void BoxBlur(int radius);
        public void Sharpen(float strength);
        public void UnsharpMask(float amount, float radius, float threshold);
        public void EdgeDetect(float threshold);
        public void Emboss(float strength);
        
        // Geometric operations
        public void FlipHorizontal();
        public void FlipVertical();
        public void Rotate90();
        public void Rotate180();
        public void Rotate270();
        public void Scale(float scaleX, float scaleY);
        
        // Advanced processing
        public void Threshold(float threshold);
        public void HistogramEqualization();
        public void NoiseReduction(float strength);
        public void Dilate(int kernelSize);
        public void Erode(int kernelSize);
        
        // Channel operations
        public void Grayscale();
        public void ExtractChannel(int channel);
        public void ReplaceChannel(int targetChannel, float value);
        
        // Distortion effects
        public void Swirl(float angle, float radius);
        public void Ripple(float amplitude, float frequency);
        public void Bulge(float strength, float radius);
    }
}