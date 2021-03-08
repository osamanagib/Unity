/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InfinityCode.HugeTexture.Editors
{
    /// <summary>
    /// Methods for Canvas and Event System
    /// </summary>
    public static class CanvasUtils
    {
        /// <summary>
        /// Returns the current canvas or creates a new one
        /// </summary>
        /// <returns></returns>
        public static Canvas GetCanvas()
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGO = new GameObject("Canvas");
                canvasGO.layer = LayerMask.NameToLayer("UI");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
            }

            GetEventSystem();

            return canvas;
        }

        /// <summary>
        /// Returns the current event system or creates a new one
        /// </summary>
        /// <returns></returns>
        public static EventSystem GetEventSystem()
        {
            EventSystem eventSystem = Object.FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            return eventSystem;
        }
    }
}