/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.HugeTexture
{
    /// <summary>
    /// Class for handling Texture2DArray as Texture2D
    /// </summary>
    public class HugeTexture2D
    {
        /// <summary>
        /// Total width of the texture in pixels
        /// </summary>
        public readonly int width;

        /// <summary>
        /// Total height of the texture in pixels
        /// </summary>
        public readonly int height;

        /// <summary>
        /// Number of pages horizontally
        /// </summary>
        public int cols { get; }

        /// <summary>
        /// Number of pages vertically
        /// </summary>
        public int rows { get; }

        /// <summary>
        /// Texture can be read
        /// </summary>
        public bool isReadable { get; private set; }

        /// <summary>
        /// Size of Texture2DArray page
        /// </summary>
        public int pageSize { get; }

        /// <summary>
        /// Reference to Texture2DArray
        /// </summary>
        public Texture2DArray textureArray { get; private set; }

        private Color32[][] pixels;
        private bool hasChanges = false;

        /// <summary>
        /// Creates a new empty readable Texture2DArray
        /// </summary>
        /// <param name="pageSize">Size of Texture2DArray page</param>
        /// <param name="cols">Number of pages horizontally</param>
        /// <param name="rows">Number of pages vertically</param>
        public HugeTexture2D(int pageSize, int cols, int rows)
        {
            if (cols <= 0) throw new Exception("CountX cannot be less than 1.");
            if (rows <= 0) throw new Exception("CountY cannot be less than 1.");
            if (pageSize < 256) throw new Exception("PageSize cannot be less than 256.");
            if (pageSize != Mathf.ClosestPowerOfTwo(pageSize)) throw new Exception("PageSize should be 2 ^ N.");

            textureArray = new Texture2DArray(pageSize, pageSize, cols * rows, TextureFormat.ARGB32, false);
            this.cols = cols;
            this.rows = rows;
            this.pageSize = pageSize;
            isReadable = true;
            width = cols * pageSize;
            height = rows * pageSize;
        }

        /// <summary>
        /// Creates a wrapper for an existing Texture2DArray. If Texture2DArray is in ARGB32 or RGB24 format, this will be readable
        /// </summary>
        /// <param name="textureArray">Texture2DArray</param>
        /// <param name="cols">Number of pages horizontally</param>
        /// <param name="rows">Number of pages vertically</param>
        public HugeTexture2D(Texture2DArray textureArray, int cols, int rows)
        {
            if (textureArray == null) throw new Exception("Texture2DArray cannot be null.");
            if (cols <= 0) throw new Exception("Rows cannot be less than 1.");
            if (rows <= 0) throw new Exception("Cols cannot be less than 1.");
            if (cols * rows != textureArray.depth) throw new Exception("Rows * cols != textureArray.depth. Check parameter values.");

            isReadable = textureArray.format == TextureFormat.ARGB32 || textureArray.format == TextureFormat.RGB24;

            this.textureArray = textureArray;
            this.cols = cols;
            this.rows = rows;
            pageSize = this.textureArray.width;

            width = pageSize * cols;
            height = pageSize * rows;
        }

        /// <summary>
        /// Actually apply all previous SetPixel and SetPixels changes.
        /// </summary>
        /// <param name="makeNoLongerReadable">When set to true, system memory copy of a texture is released.</param>
        public void Apply(bool makeNoLongerReadable = false)
        {
            if (!hasChanges) return;

            if (pixels != null)
            {
                for (int i = 0; i < pixels.Length; i++)
                {
                    if (pixels[i] == null) continue;
                    textureArray.SetPixels32(pixels[i], i);
                }

                textureArray.Apply(false, makeNoLongerReadable);
                if (makeNoLongerReadable)
                {
                    pixels = null;
                    isReadable = false;
                }
            }

            hasChanges = false;
        }

        private void CheckReadable()
        {
            if (!isReadable) throw new Exception("Huge Texture is not readable! Use MakeReadable first.");
        }

        /// <summary>
        /// Encodes this texture into JPG format
        /// </summary>
        /// <param name="quality">JPG quality to encode with, 1..100 (default 75)</param>
        /// <returns>Byte array is the JPG file</returns>
        public byte[] EncodeToJPG(int quality = 75)
        {
            CheckReadable();

            Texture2D texture = GetTexture2D();
            byte[] bytes = texture.EncodeToJPG(quality);
            Object.Destroy(texture);
            return bytes;
        }

        /// <summary>
        /// Encodes this texture into PNG format
        /// </summary>
        /// <returns>Byte array is the PNG file</returns>
        public byte[] EncodeToPNG()
        {
            CheckReadable();

            Texture2D texture = GetTexture2D();
            byte[] bytes = texture.EncodeToPNG();
            Object.Destroy(texture);
            return bytes;
        }

        /// <summary>
        /// Returns pixel color at coordinates (x, y)
        /// </summary>
        /// <param name="x">X (from 0 to width - 1)</param>
        /// <param name="y">Y (from 0 to height - 1)</param>
        /// <returns>Pixel color</returns>
        public Color GetPixel(int x, int y) => GetPixel32(x, y);

        /// <summary>
        /// Returns pixel color at coordinates (x, y)
        /// </summary>
        /// <param name="x">X (from 0 to width - 1)</param>
        /// <param name="y">Y (from 0 to height - 1)</param>
        /// <returns>Pixel color</returns>
        public Color32 GetPixel32(int x, int y)
        {
            CheckReadable();

            int px = x / pageSize;
            int py = y / pageSize;
            if (pixels == null) pixels = new Color32[cols * rows][];
            int pageIndex = py * cols + px;
            if (pixels[pageIndex] == null) pixels[pageIndex] = textureArray.GetPixels32(pageIndex);
            int lx = x - px * pageSize;
            int ly = y - py * pageSize;
            return pixels[pageIndex][ly * pageSize + lx];
        }

        /// <summary>
        /// Returns filtered pixel color at normalized coordinates (u, v)
        /// </summary>
        /// <param name="u">U (from 0.0 to 1.0)</param>
        /// <param name="v">V (from 0.0 to 1.0)</param>
        /// <returns>Pixel color</returns>
        public Color GetPixelBilinear(float u, float v)
        {
            CheckReadable();

            if (u < 0) u = 0;
            if (u > 1) u = 1;
            if (v < 0) v = 0;
            if (v > 1) v = 1;

            float x = u * width;
            float y = v * height;
            int px = (int)x / pageSize;
            int py = (int)y / pageSize;
            if (pixels == null) pixels = new Color32[cols * rows][];
            int pageIndex = py * cols + px;
            if (pixels[pageIndex] == null) pixels[pageIndex] = textureArray.GetPixels32(pageIndex);
            float lx = x - px * pageSize;
            float ly = y - py * pageSize;

            int ilx1 = (int) lx;
            int ily1 = (int) ly;

            int ilx2 = ilx1 + 1;
            int ily2 = ily1 + 1;

            if (ilx2 >= pageSize) ilx2 = pageSize - 1;
            if (ily2 >= pageSize) ily2 = pageSize - 1;

            float rx = lx - ilx1;
            float ry = ly - ily1;

            Color32[] page = pixels[pageIndex];

            Color32 c1 = page[ily1 * pageSize + ilx1];
            Color32 c2 = page[ily1 * pageSize + ilx2];
            Color32 c3 = page[ily2 * pageSize + ilx2];
            Color32 c4 = page[ily2 * pageSize + ilx1];

            c1 = Color32.Lerp(c1, c2, rx);
            c3 = Color32.Lerp(c3, c4, rx);

            return Color32.Lerp(c1, c3, ry);
        }

        /// <summary>
        /// Get the pixel colors from the texture
        /// </summary>
        /// <returns>Pixel colors</returns>
        public Color[] GetPixels()
        {
            CheckReadable();

            Color[] colors = new Color[width * height];

            if (pixels == null) pixels = new Color32[cols * rows][];

            for (int i = 0; i < cols * rows; i++)
            {
                if (pixels[i] == null) pixels[i] = textureArray.GetPixels32(i);
                Color32[] px = pixels[i];
                int sx = i % cols * pageSize;
                int sy = i / cols * pageSize;

                for (int j = 0; j < pageSize; j++)
                {
                    int pxRow = j * pageSize;
                    int cRow = (sy + j) * width + sx;
                    for (int k = 0; k < pageSize; k++) colors[cRow + k] = px[pxRow + k];
                }
            }

            return colors;
        }

        /// <summary>
        /// Get a block of pixel colors in Color32 format
        /// </summary>
        /// <returns>Pixel colors</returns>
        public Color32[] GetPixels32()
        {
            CheckReadable();

            Color32[] colors = new Color32[width * height];

            if (pixels == null) pixels = new Color32[cols * rows][];

            for (int i = 0; i < cols * rows; i++)
            {
                if (pixels[i] == null) pixels[i] = textureArray.GetPixels32(i);
                Color32[] px = pixels[i];
                int sx = i % cols * pageSize;
                int sy = i / cols * pageSize;

                for (int j = 0; j < pageSize; j++)
                {
                    int pxRow = j * pageSize;
                    int cRow = (sy + j) * width + sx;
                    for (int k = 0; k < pageSize; k++) colors[cRow + k] = px[pxRow + k];
                }
            }

            return colors;
        }

        /// <summary>
        /// Converts current Texture2DArray to Texture2D
        /// </summary>
        /// <returns></returns>
        public Texture2D GetTexture2D()
        {
            if (!isReadable) return GetTexture2DFromNonReadable();

            Texture2D texture = new Texture2D(width, height);
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    Graphics.CopyTexture(textureArray, y * cols + x, 0, 0, 0, pageSize, pageSize, texture, 0, 0, x * pageSize, y * pageSize);
                }
            }
            texture.Apply(true);
            return texture;
        }

        private Texture2D GetTexture2DFromNonReadable()
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                width,
                height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;

            Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            texture.wrapMode = TextureWrapMode.Clamp;

            Material mat = new Material(Shader.Find("Huge Texture/Unlit Array"));
            mat.SetInt("_Cols", cols);
            mat.SetInt("_Rows", rows);

            Graphics.Blit(textureArray, renderTex, mat, 0, 0);
            texture.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            texture.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);

            return texture;
        }

        /// <summary>
        /// Converts the current Texture2DArray in DXT1 and DXT5 format to ARGB32 and makes it readable. Important: this creates a new Texture2DArray
        /// </summary>
        /// <param name="destroyOldTexture2DArray">Destroy the old Texture2DArray?</param>
        public void MakeReadable(bool destroyOldTexture2DArray = false)
        {
            if (isReadable) return;

            RenderTexture renderTex = RenderTexture.GetTemporary(
                pageSize,
                pageSize,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            Texture2D texture = new Texture2D(pageSize, pageSize, TextureFormat.ARGB32, false);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;

            Texture2DArray newArray = new Texture2DArray(pageSize, pageSize, textureArray.depth, TextureFormat.ARGB32, false);
            newArray.wrapMode = TextureWrapMode.Clamp;

            for (int i = 0; i < cols * rows; i++)
            {
                Graphics.Blit(textureArray, renderTex, i, 0);
                texture.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
                texture.Apply();
                newArray.SetPixelData(texture.GetRawTextureData(), 0, i);
            }

            newArray.Apply();
            if (destroyOldTexture2DArray) Object.Destroy(textureArray);
            textureArray = newArray;

            Object.Destroy(texture);

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);

            isReadable = true;
        }

        /// <summary>
        /// Sets pixel color at coordinates (x,y)
        /// </summary>
        /// <param name="x">X (from 0 to width - 1)</param>
        /// <param name="y">Y (from 0 to height - 1)</param>
        /// <param name="color">Pixel color</param>
        public void SetPixel(int x, int y, Color color) => SetPixel32(x, y, color);

        /// <summary>
        /// Sets pixel color at coordinates (x,y)
        /// </summary>
        /// <param name="x">X (from 0 to width - 1)</param>
        /// <param name="y">Y (from 0 to height - 1)</param>
        /// <param name="color">Pixel color</param>
        public void SetPixel32(int x, int y, Color32 color)
        {
            CheckReadable();

            int px = x / pageSize;
            int py = y / pageSize;
            if (pixels == null) pixels = new Color32[cols * rows][];
            int pageIndex = py * cols + px;
            if (pixels[pageIndex] == null) pixels[pageIndex] = textureArray.GetPixels32(pageIndex);
            int lx = x - px * pageSize;
            int ly = y - py * pageSize;
            pixels[pageIndex][ly * pageSize + lx] = color;
            hasChanges = true;
        }

        /// <summary>
        /// Set a block of pixel colors
        /// </summary>
        /// <param name="colors">The array of pixel colors to assign</param>
        public void SetPixels(Color[] colors)
        {
            CheckReadable();

            if (colors == null) throw new Exception("Colors cannot be null.");
            if (colors.Length != width * height) throw new Exception("The texture size does not match the size of the array of colors.");

            if (pixels == null) pixels = new Color32[cols * rows][];
            for (int i = 0; i < cols * rows; i++)
            {
                if (pixels[i] == null) pixels[i] = new Color32[pageSize * pageSize];
                Color32[] px = pixels[i];
                int sx = i % cols * pageSize;
                int sy = i / cols * pageSize;

                for (int j = 0; j < pageSize; j++)
                {
                    int pxRow = j * pageSize;
                    int cRow = (sy + j) * width + sx;
                    for (int k = 0; k < pageSize; k++) px[pxRow + k] = colors[cRow + k];
                }
            }

            hasChanges = true;
        }

        /// <summary>
        /// Set a block of pixel colors
        /// </summary>
        /// <param name="x">X (from 0 to width - 1)</param>
        /// <param name="y">Y (from 0 to height - 1)</param>
        /// <param name="blockWidth">Width of th block</param>
        /// <param name="blockHeight">Height of the block</param>
        /// <param name="colors">The array of pixel colors to assign</param>
        public void SetPixels(int x, int y, int blockWidth, int blockHeight, Color[] colors)
        {
            CheckReadable();

            if (blockWidth <= 0 || blockHeight <= 0) return;
            if (colors == null) throw new Exception("Colors cannot be null.");
            if (colors.Length != blockWidth * blockHeight) throw new Exception("The block size does not match the size of the array of colors.");
            if (x + blockWidth > width) throw new Exception("x + blockWidth is larger than the texture size.");
            if (y + blockHeight > height) throw new Exception("y + blockHeight is larger than the texture size.");

            for (int j = 0; j < blockHeight; j++)
            {
                int cj = j + y;
                int row = j * blockWidth;
                for (int i = 0; i < blockWidth; i++) SetPixel32(i + x, cj, colors[row + i]);
            }
        }

        /// <summary>
        /// Set a block of pixel colors
        /// </summary>
        /// <param name="colors">The array of pixel colors to assign</param>
        public void SetPixels32(Color32[] colors)
        {
            CheckReadable();

            if (colors == null) throw new Exception("Colors cannot be null.");
            if (colors.Length != width * height) throw new Exception("The texture size does not match the size of the array of colors.");

            if (pixels == null) pixels = new Color32[cols * rows][];
            for (int i = 0; i < cols * rows; i++)
            {
                if (pixels[i] == null) pixels[i] = new Color32[pageSize * pageSize];
                Color32[] px = pixels[i];
                int sx = i % cols * pageSize;
                int sy = i / cols * pageSize;

                for (int j = 0; j < pageSize; j++)
                {
                    int pxRow = j * pageSize;
                    int cRow = (sy + j) * width + sx;
                    for (int k = 0; k < pageSize; k++) px[pxRow + k] = colors[cRow + k];
                }
            }

            hasChanges = true;
        }

        /// <summary>
        /// Set a block of pixel colors
        /// </summary>
        /// <param name="x">X (from 0 to width - 1)</param>
        /// <param name="y">Y (from 0 to height - 1)</param>
        /// <param name="blockWidth">Width of th block</param>
        /// <param name="blockHeight">Height of the block</param>
        /// <param name="colors">The array of pixel colors to assign</param>
        public void SetPixels32(int x, int y, int blockWidth, int blockHeight, Color32[] colors)
        {
            CheckReadable();

            if (blockWidth <= 0 || blockHeight <= 0) return;
            if (colors == null) throw new Exception("Colors cannot be null.");
            if (colors.Length != blockWidth * blockHeight) throw new Exception("The block size does not match the size of the array of colors.");
            if (x + blockWidth > width) throw new Exception("x + blockWidth is larger than the texture size.");
            if (y + blockHeight > height) throw new Exception("y + blockHeight is larger than the texture size.");

            for (int j = 0; j < blockHeight; j++)
            {
                int cj = j + y;
                int row = j * blockWidth;
                for (int i = 0; i < blockWidth; i++) SetPixel32(i + x, cj, colors[row + i]);
            }
        }
    }
}