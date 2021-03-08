/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;

namespace InfinityCode.HugeTexture.Editors
{

    [CustomEditor(typeof(HugeTextureImporter))]
    public class HugeTextureImporterEditor : HugeImporterEditor
    {
        protected override void DrawSize()
        {
            EditorGUILayout.LabelField("Original Width", originalWidth.intValue.ToString());
            EditorGUILayout.LabelField("Original Height", originalHeight.intValue.ToString());
        }
    }
}