/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.IO;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.HugeTexture.Editors
{
    public static class EditorUtils
    {
        private static string _assetPath;

        public static string assetPath
        {
            get
            {
                if (_assetPath == null)
                {
                    string[] assets = AssetDatabase.FindAssets("HugeTextureImporterEditor");
                    FileInfo info = new FileInfo(AssetDatabase.GUIDToAssetPath(assets[0]));
                    _assetPath = info.Directory.Parent.Parent.FullName.Substring(Application.dataPath.Length - 6) + "/";
                }
                return _assetPath;
            }
        }

        public static T LoadAsset<T>(string path) where T: Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(assetPath + path);
        }
    }
}