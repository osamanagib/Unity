/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.IO;
using UnityEditor;
using UnityEditor.Build;

using UnityEngine;

namespace InfinityCode.HugeTexture.Editors
{
    public abstract class HugeImporter : UnityEditor.AssetImporters.ScriptedImporter, IActiveBuildTargetChanged
    {
        public const string version = "1.1.0.1";
        public const string MenuPath = "Window/Infinity Code/Huge Texture/";

        [SerializeField]
        protected int pageSize = 1024;

        [SerializeField]
        protected int originalWidth;

        [SerializeField]
        protected int originalHeight;

        [SerializeField]
        protected int cols = 0;

        [SerializeField]
        protected int rows = 0;

        [SerializeField]
        protected bool compressed = false;

        [SerializeField]
        protected bool transparent = false;

        [SerializeField]
        protected TextureCompressionQuality quality = TextureCompressionQuality.Fast;

        public int callbackOrder
        {
            get { return 0; }
        }

        public static TextureFormat GetTextureFormat(BuildTarget buildTarget, bool compressed, bool transparent)
        {
            TextureFormat format = transparent ? TextureFormat.ARGB32 : TextureFormat.RGB24;

            if (compressed)
            {
                if (buildTarget == BuildTarget.Android)
                {
                    format = transparent ? TextureFormat.ETC2_RGBA8 : TextureFormat.ETC_RGB4;
                }
                else if (buildTarget == BuildTarget.iOS)
                {
                    format = transparent ? TextureFormat.PVRTC_RGBA4 : TextureFormat.PVRTC_RGB4;
                }
                else if (buildTarget == BuildTarget.tvOS)
                {
                    format = transparent ? TextureFormat.ASTC_4x4 : TextureFormat.ASTC_4x4;
                }
                else
                {
                    format = transparent ? TextureFormat.DXT5 : TextureFormat.DXT1;
                }
            }

            return format;
        }

        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2DArray");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }

        [MenuItem("Assets/Import Huge Texture", false, 20)]
        public static void Import()
        {
            string path = EditorUtility.OpenFilePanelWithFilters("Select Image", Application.dataPath, new[] { "Image Files", "png,jpg,jpeg,raw" });
            if (string.IsNullOrEmpty(path)) return;

            string ext = ".huge" + Path.GetExtension(path).Substring(1);
            string newPath = Application.dataPath + "/" + Path.GetFileNameWithoutExtension(path);

            if (File.Exists(newPath + ext))
            {
                int index = 1;
                while (File.Exists(newPath + "_" + index + ext)) index++;

                newPath += "_" + index;
            }

            newPath += ext;

            File.Copy(path, newPath);
            AssetDatabase.Refresh();
        }
    }
}