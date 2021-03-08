/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.IO;
using UnityEditor;

using UnityEngine;

namespace InfinityCode.HugeTexture.Editors
{
    [UnityEditor.AssetImporters.ScriptedImporter(1, new[] { "hugepng", "hugejpg", "hugejpeg" })]
    public class HugeTextureImporter : HugeImporter
    {
        private Texture2DArray InitTexture2DArray(Texture2D sourceTexture, UnityEditor.AssetImporters.AssetImportContext ctx)
        {
            TextureFormat format = GetTextureFormat(ctx.selectedBuildTarget, compressed, transparent);

            EditorUtility.DisplayProgressBar("Import Huge Texture", ctx.assetPath, 0);

            Texture2DArray array = null;

            try
            {
                if (format == sourceTexture.format)
                {
                    array = new Texture2DArray(pageSize, pageSize, cols * rows, sourceTexture.format, false);
                    array.wrapMode = TextureWrapMode.Clamp;
                    for (int x = 0; x < cols; x++)
                    {
                        for (int y = 0; y < rows; y++)
                        {
                            EditorUtility.DisplayProgressBar("Import Huge Texture", ctx.assetPath, (x * rows + y) / (float)(cols * rows));
                            Graphics.CopyTexture(sourceTexture, 0, 0, x * pageSize, y * pageSize, pageSize, pageSize, array, y * cols + x, 0, 0, 0);
                        }
                    }
                }
                else if (!compressed)
                {
                    array = new Texture2DArray(pageSize, pageSize, cols * rows, format, false);
                    array.wrapMode = TextureWrapMode.Clamp;
                    for (int x = 0; x < cols; x++)
                    {
                        for (int y = 0; y < rows; y++)
                        {
                            EditorUtility.DisplayProgressBar("Import Huge Texture", ctx.assetPath, (x * rows + y) / (float)(cols * rows));
                            Color[] colors = sourceTexture.GetPixels(x * pageSize, y * pageSize, pageSize, pageSize);
                            array.SetPixels(colors, y * cols + x);
                        }
                    }
                }
                else if (transparent)
                {
                    array = new Texture2DArray(pageSize, pageSize, cols * rows, format, false);
                    array.wrapMode = TextureWrapMode.Clamp;
                    for (int x = 0; x < cols; x++)
                    {
                        for (int y = 0; y < rows; y++)
                        {
                            EditorUtility.DisplayProgressBar("Import Huge Texture", ctx.assetPath, (x * rows + y) / (float)(cols * rows));
                            Texture2D tempTexture = new Texture2D(pageSize, pageSize);
                            Color[] colors = sourceTexture.GetPixels(x * pageSize, y * pageSize, pageSize, pageSize);
                            tempTexture.SetPixels(colors);
                            tempTexture.Apply();
                            EditorUtility.CompressTexture(tempTexture, format, quality);

                            array.SetPixelData(tempTexture.GetRawTextureData(), 0, y * cols + x);
                            DestroyImmediate(tempTexture);
                        }
                    }
                }
                else
                {
                    array = new Texture2DArray(pageSize, pageSize, cols * rows, format, false);
                    array.wrapMode = TextureWrapMode.Clamp;
                    for (int x = 0; x < cols; x++)
                    {
                        for (int y = 0; y < rows; y++)
                        {
                            EditorUtility.DisplayProgressBar("Import Huge Texture", ctx.assetPath, (x * rows + y) / (float)(cols * rows));
                            Texture2D tempTexture = new Texture2D(pageSize, pageSize, TextureFormat.RGB24, false);
                            Color[] colors = sourceTexture.GetPixels(x * pageSize, y * pageSize, pageSize, pageSize);
                            tempTexture.SetPixels(colors);
                            tempTexture.Apply();
                            EditorUtility.CompressTexture(tempTexture, format, quality);

                            array.SetPixelData(tempTexture.GetRawTextureData(), 0, y * cols + x);
                            DestroyImmediate(tempTexture);
                        }
                    }
                }

                array.Apply(false);
            }
            catch
            {
                
            }

            EditorUtility.ClearProgressBar();

            return array;
        }

        public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
        {
            byte[] bytes = File.ReadAllBytes(ctx.assetPath);
            Texture2D sourceTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            sourceTexture.LoadImage(bytes);

            originalWidth = sourceTexture.width;
            originalHeight = sourceTexture.height;

            cols = sourceTexture.width / pageSize;
            if (cols == 0) cols = 1;

            rows = sourceTexture.height / pageSize;
            if (rows == 0) rows = 1;

            bool changedPageSize = false;

            while (cols * rows > 2048)
            {
                pageSize *= 2;
                cols /= 2;
                rows /= 2;
                changedPageSize = true;
            }

            if (changedPageSize)
            {
                ctx.LogImportWarning("Texture Width * Texture Height * Page Size must be less or equal to 2048, so the Page Size is increased to " + pageSize);
            }

            int resizeX = sourceTexture.width;
            int resizeY = sourceTexture.height;

            if (sourceTexture.width % pageSize != 0) resizeX = cols * pageSize;
            if (sourceTexture.height % pageSize != 0) resizeY = rows * pageSize;

            if (resizeX != sourceTexture.width || resizeY != sourceTexture.height)
            {
                TextureScaler.Scale(sourceTexture, resizeX, resizeY);
                ctx.LogImportWarning("The width and height of the texture must be equal to Page Size * N, so the texture size is changed to " + resizeX + "x" + resizeY);
            }

            Texture2DArray array = InitTexture2DArray(sourceTexture, ctx);

            ctx.AddObjectToAsset("_MainTex", array);
            ctx.SetMainObject(array);

            DestroyImmediate(sourceTexture);
        }
    }
}