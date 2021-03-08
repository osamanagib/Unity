/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.IO;
using UnityEditor;

using UnityEngine;

namespace InfinityCode.HugeTexture.Editors
{
    [UnityEditor.AssetImporters.ScriptedImporter(1, new[] { "hugeraw" })]
    public class HugeRawImporter : HugeImporter
    {
        [SerializeField]
        private int depth = 8;

        [SerializeField]
        private ByteOrder byteOrder = ByteOrder.Windows;

        [SerializeField]
        private RawPixelFormat colorFormat = RawPixelFormat.RGB;

        [SerializeField]
        private long filesize;

        public static int GetPixelSize(RawPixelFormat format)
        {
            if (format == RawPixelFormat.Grayscale) return 1;
            if (format == RawPixelFormat.RGB) return 3;
            if (format == RawPixelFormat.RGBA) return 4;

            throw new Exception("Unknown RawPixelFormat");
        }

        private Texture2DArray InitTexture2DArray(Stream stream, UnityEditor.AssetImporters.AssetImportContext ctx)
        {
            TextureFormat format = GetTextureFormat(ctx.selectedBuildTarget, compressed, transparent);

            Texture2DArray array = null;

            EditorUtility.DisplayProgressBar("Import Huge Texture", ctx.assetPath, 0);

            try
            {
                array = new Texture2DArray(pageSize, pageSize, cols * rows, format, false);
                array.wrapMode = TextureWrapMode.Clamp;
                TextureFormat textureFormat = transparent ? TextureFormat.RGBA32 : TextureFormat.RGB24;
                Texture2D texture = new Texture2D(pageSize, pageSize, textureFormat, false);

                BinaryReader reader = new BinaryReader(stream);

                int colorSize = GetPixelSize(colorFormat);
                int bufferSize = pageSize * colorSize * depth / 8;
                Color32[] colors = new Color32[pageSize * pageSize];

                int lastY = rows * pageSize - 1;

                for (int row = 0; row < rows; row++)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        EditorUtility.DisplayProgressBar("Import Huge Texture", ctx.assetPath, (row * cols + col) / (float)(cols * rows));

                        for (int y = 0; y < pageSize; y++)
                        {
                            int srow = lastY - row * pageSize - y;
                            int scol = col * pageSize;

                            long offset = ((long)srow * originalWidth + scol) * colorSize * depth / 8;
                            stream.Seek(offset, SeekOrigin.Begin);

                            byte[] buffer = reader.ReadBytes(bufferSize);
                            if (depth == 8) Parse8BitRow(buffer, colors, y);
                            else Parse16BitRow(buffer, colors, y);
                        }

                        texture.SetPixels32(colors);
                        texture.Apply(false);

                        if (compressed) EditorUtility.CompressTexture(texture, format, quality);

                        array.SetPixelData(texture.GetRawTextureData(), 0, row * cols + col);

                        if (compressed)
                        {
                            DestroyImmediate(texture);
                            texture = new Texture2D(pageSize, pageSize, textureFormat, false);
                        }
                    }
                }

                DestroyImmediate(texture);

                reader.Close();

                array.Apply();
            }
            catch
            {
                
            }

            EditorUtility.ClearProgressBar();

            return array;
        }

        public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
        {
            FileInfo info = new FileInfo(ctx.assetPath);
            filesize = info.Length;

            if (cols <= 0 || rows <= 0) return;

            int colorSize = GetPixelSize(colorFormat);

            if ((long)originalWidth * originalHeight * depth * colorSize != info.Length * 8)
            {
                ctx.LogImportError("Cols * Rows * Depth * Pixel Size must be equal to file size * 8");
                return;
            }

            Stream stream = File.OpenRead(ctx.assetPath);
            Texture2DArray array = InitTexture2DArray(stream, ctx);

            ctx.AddObjectToAsset("_MainTex", array);
            ctx.SetMainObject(array);
        }

        private float Parse16BitChannel(byte[] buffer, int index)
        {
            int v1 = buffer[index];
            int v2 = buffer[index + 1];

            if (byteOrder == ByteOrder.Windows) return (v2 * 256 + v1) / 65536f;
            return (v1 * 256 + v2) / 65536f;
        }

        private void Parse16BitRow(byte[] buffer, Color32[] colors, int y)
        {
            Color32 clr;
            if (colorFormat == RawPixelFormat.RGB)
            {
                for (int x = 0; x < pageSize; x++)
                {
                    int i = x * 6;
                    clr = new Color(Parse16BitChannel(buffer, i), Parse16BitChannel(buffer, i + 2), Parse16BitChannel(buffer, i + 4), 255);
                    colors[y * pageSize + x] = clr;
                }
            }
            else if (colorFormat == RawPixelFormat.RGBA)
            {
                for (int x = 0; x < pageSize; x++)
                {
                    int i = x * 8;
                    clr = new Color(Parse16BitChannel(buffer, i), Parse16BitChannel(buffer, i + 2), Parse16BitChannel(buffer, i + 4), 255);
                    colors[y * pageSize + x] = clr;
                }
            }
            else
            {
                for (int x = 0; x < pageSize; x++)
                {
                    int i = x * 2;
                    float v = Parse16BitChannel(buffer, i);
                    clr = new Color(v, v, v, 1);
                    colors[y * pageSize + x] = clr;
                }
            }
        }

        private void Parse8BitRow(byte[] buffer, Color32[] colors, int y)
        {
            Color32 clr;
            if (colorFormat == RawPixelFormat.RGB)
            {
                for (int x = 0; x < pageSize; x++)
                {
                    int i = x * 3;
                    clr = new Color32(buffer[i], buffer[i + 1], buffer[i + 2], 255);
                    colors[y * pageSize + x] = clr;
                }
            }
            else if (colorFormat == RawPixelFormat.RGBA)
            {
                for (int x = 0; x < pageSize; x++)
                {
                    int i = x * 4;
                    clr = new Color32(buffer[i], buffer[i + 1], buffer[i + 2], buffer[i + 3]);
                    colors[y * pageSize + x] = clr;
                }
            }
            else
            {
                for (int x = 0; x < pageSize; x++)
                {
                    byte v = buffer[x];
                    clr = new Color32(v, v, v, 255);
                    colors[y * pageSize + x] = clr;
                }
            }
        }
    }
}