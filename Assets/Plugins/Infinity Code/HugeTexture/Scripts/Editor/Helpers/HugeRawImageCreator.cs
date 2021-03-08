/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.HugeTexture.Components.UI;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.HugeTexture.Editors
{
    public static class HugeRawImageCreator
    {
        [MenuItem("GameObject/UI/Huge Raw Image", false, 2003)]
        public static HugeRawImage Create()
        {
            Canvas canvas = CanvasUtils.GetCanvas();
            GameObject go = new GameObject("Huge Raw Image");
            go.AddComponent<RectTransform>();
            go.AddComponent<CanvasRenderer>();
            HugeRawImage hugeRawImage = go.AddComponent<HugeRawImage>();
            go.transform.SetParent(canvas.transform);
            go.transform.localPosition = Vector3.zero;

            return hugeRawImage;
        }
    }
}