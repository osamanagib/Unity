/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.HugeTexture.Editors
{
    [CustomEditor(typeof(HugeRawImporter))]
    public class HugeRawImporterEditor : HugeImporterEditor
    {
        private static string[] depthStrings = { "8", "16" };
        private static int[] depthInts = { 8, 16 };

        private SerializedProperty byteOrder;
        private SerializedProperty colorFormat;
        private SerializedProperty depth;
        private SerializedProperty filesize;
        private bool wrongFilesize;
        private bool wrongWidth;
        private bool wrongHeight;
        private bool wrongTexture2DArraySize;

        protected override void DrawSize()
        {
            EditorGUILayout.PropertyField(originalWidth);
            EditorGUILayout.PropertyField(originalHeight);
        }

        protected override void DrawExtraSizeFields()
        {
            if (depth == null) return;
             
            EditorGUI.BeginChangeCheck();
            int d = EditorGUILayout.IntPopup("Depth", depth.intValue, depthStrings, depthInts);
            if (EditorGUI.EndChangeCheck()) depth.intValue = d;
            if (depth.intValue == 16) EditorGUILayout.PropertyField(byteOrder);
            EditorGUILayout.PropertyField(colorFormat);
        }

        protected override void DrawWarnings()
        {
            if (cols.intValue == 0 || rows.intValue == 0)
            {

            }
            else if (wrongFilesize)
            {
                EditorGUILayout.HelpBox("Original Width * Original Height * Color Size * Depth / 8 should be equal to the file size.", MessageType.Error);
            }
            else if (wrongTexture2DArraySize)
            {
                EditorGUILayout.HelpBox("Original Width * Original Height * " + (transparent.boolValue? 4: 3) + " should be less that 2GB (2147483648 bytes).", MessageType.Error);
            }
            else if (wrongWidth)
            {
                EditorGUILayout.HelpBox("Page Size * Cols should be equal to Original Width.\nAuto scaling for RAW is not supported.", MessageType.Error);
            }
            else if (wrongHeight)
            {
                EditorGUILayout.HelpBox("Page Size * Rows should be equal to Original Height.\nAuto scaling for RAW is not supported.", MessageType.Error);
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();

            depth = serializedObject.FindProperty("depth");
            byteOrder = serializedObject.FindProperty("byteOrder");
            colorFormat = serializedObject.FindProperty("colorFormat");
            filesize = serializedObject.FindProperty("filesize");
        }

        protected override bool ValidateFields()
        {
            int pixelSize = HugeRawImporter.GetPixelSize((RawPixelFormat)colorFormat.enumValueIndex);
            long expectedSize = (long)originalWidth.intValue * originalHeight.intValue * pixelSize * depth.intValue;
            wrongFilesize = filesize.longValue * 8 != expectedSize;
            wrongTexture2DArraySize = (long) originalWidth.intValue * originalHeight.intValue * (transparent.boolValue ? 4 : 3) > 2147483648L;

            wrongWidth = pageSize.intValue * cols.intValue != originalWidth.intValue;
            wrongHeight = pageSize.intValue * rows.intValue != originalHeight.intValue;
            return !wrongFilesize && !wrongWidth && !wrongHeight && !wrongTexture2DArraySize;
        }

        
    }
}