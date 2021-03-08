/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEngine;
using UnityEngine.UI;

namespace InfinityCode.HugeTexture.Components.UI
{
    [AddComponentMenu("UI/Huge Raw Image", 12)]
    public class HugeRawImage : RawImage
    {
        protected override void UpdateMaterial()
        {
            if (!IsActive()) return;

            canvasRenderer.materialCount = 1;
            canvasRenderer.SetMaterial(materialForRendering, 0);
            materialForRendering.mainTexture = texture;
        }
    }
}