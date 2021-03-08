/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Diagnostics;
using System.IO;
using System.Linq;
using InfinityCode.HugeTexture.Components.UI;
using UnityEditor;

using UnityEngine;

namespace InfinityCode.HugeTexture.Editors
{
    public abstract class HugeImporterEditor : UnityEditor.AssetImporters.ScriptedImporterEditor
    {
        protected int[] pageSizes = { 256, 512, 1024, 2048, 4096, 8192 };
        protected string[] displayedPageSizes;

        protected SerializedProperty pageSize;
        protected SerializedProperty cols;
        protected SerializedProperty rows;
        protected SerializedProperty originalWidth;
        protected SerializedProperty originalHeight;
        protected SerializedProperty quality;
        protected SerializedProperty compressed;
        protected SerializedProperty transparent;

        protected GUIContent updateAvailableContent;
        private GUIStyle toolbarLabelStyle;

        public Material CreateMaterial(string shader, bool setTexture)
        {
            HugeImporter importer = serializedObject.targetObject as HugeImporter;
            string path = importer.assetPath;

            string matPath = path.Substring(0, path.LastIndexOf("."));
            if (File.Exists(matPath + ".mat"))
            {
                int index = 1;
                while (File.Exists(matPath + "_" + index + ".mat"))
                {
                    index++;
                }

                matPath += "_" + index;
            }

            matPath += ".mat";

            Material mat = new Material(Shader.Find(shader));
            if (setTexture) mat.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2DArray>(path));
            mat.SetInt("_Cols", cols.intValue);
            mat.SetInt("_Rows", rows.intValue);

            AssetDatabase.CreateAsset(mat, matPath);

            return AssetDatabase.LoadAssetAtPath<Material>(matPath);
        }

        protected virtual void DrawExtraSizeFields()
        {
            
        }

        protected abstract void DrawSize();

        protected void DrawToolbarGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (Updater.hasNewVersion && updateAvailableContent != null)
            {
                Color defBackgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(1, 0.5f, 0.5f);
                if (GUILayout.Button(updateAvailableContent, EditorStyles.toolbarButton)) Updater.OpenWindow();
                GUI.backgroundColor = defBackgroundColor;
            }
            else
            {
                GUILayout.Label("Huge Texture v" + HugeImporter.version, toolbarLabelStyle);
            }

            if (GUILayout.Button("Help", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Documentation"), false, () => { Process.Start("https://infinity-code.com/documentation/huge-texture.pdf"); });
                menu.AddItem(new GUIContent("API Reference"), false, () => { Process.Start("https://infinity-code.com/en/docs/api/huge-texture"); });
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Product Page"), false, () => { Process.Start("https://infinity-code.com/assets/huge-texture"); });
                menu.AddItem(new GUIContent("Forum"), false, () => { Process.Start("https://forum.infinity-code.com"); });
                menu.AddItem(new GUIContent("Check Updates"), false, Updater.OpenWindow);
                menu.AddItem(new GUIContent("Support"), false, () => { Process.Start("mailto:support@infinity-code.com?subject=Huge%20Texture"); });
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Rate and Review"), false, () => { Process.Start("https://assetstore.unity.com/packages/tools/input-management/huge-texture-163576/reviews"); });
                menu.AddItem(new GUIContent("About"), false, About.OpenWindow);
                menu.ShowAsContext();
            }

            GUILayout.EndHorizontal();
        }

        protected virtual void DrawWarnings()
        {
            
        }

        public override void OnEnable()
        {
            base.OnEnable();

            pageSize = serializedObject.FindProperty("pageSize");
            cols = serializedObject.FindProperty("cols");
            rows = serializedObject.FindProperty("rows");
            originalWidth = serializedObject.FindProperty("originalWidth");
            originalHeight = serializedObject.FindProperty("originalHeight");
            quality = serializedObject.FindProperty("quality");
            compressed = serializedObject.FindProperty("compressed");
            transparent = serializedObject.FindProperty("transparent");

            displayedPageSizes = pageSizes.Select(s => s.ToString()).ToArray();

            updateAvailableContent = new GUIContent("Update Available", EditorUtils.LoadAsset<Texture2D>("Textures/UpdateAvailable.png"), "Update Available");
        }

        public override void OnInspectorGUI()
        {
            if (toolbarLabelStyle == null) toolbarLabelStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
             
            DrawToolbarGUI();

            DrawSize();
            DrawExtraSizeFields();

            compressed.boolValue = EditorGUILayout.Toggle("Compression", compressed.boolValue);
            EditorGUILayout.LabelField("or");
            compressed.boolValue = !EditorGUILayout.Toggle("Readability", !compressed.boolValue);

            if (compressed.boolValue)
            {
                EditorGUILayout.PropertyField(quality);
                if (quality.enumValueIndex > 1) EditorGUILayout.HelpBox("Importing with best quality can take VERY long time.", MessageType.Warning);
            }

            EditorGUILayout.PropertyField(transparent);

            TextureFormat format = HugeImporter.GetTextureFormat(EditorUserBuildSettings.activeBuildTarget, compressed.boolValue, transparent.boolValue);
            EditorGUILayout.LabelField("Format", format.ToString());

            pageSize.intValue = EditorGUILayout.IntPopup("Page Size", pageSize.intValue, displayedPageSizes, pageSizes);

            int cx = originalWidth.intValue / pageSize.intValue;
            if (cx == 0) cx = 1;

            int cy = originalHeight.intValue / pageSize.intValue;
            if (cy == 0) cy = 1;

            cols.intValue = cx;
            rows.intValue = cy;

            EditorGUILayout.LabelField("Cols", cx.ToString());
            EditorGUILayout.LabelField("Rows", cy.ToString());
            EditorGUILayout.LabelField("Total Pages", (cx * cy).ToString());

            if (cx * cy > 2048)
            {
                EditorGUILayout.HelpBox("Total Pages (Cols * Rows) must be less or equal to 2048.\nPage Size will be automatically increased.", MessageType.Error);
            }

            serializedObject.ApplyModifiedProperties();
            bool hasErrors = !ValidateFields();
            if (hasErrors) DrawWarnings();
            using (new EditorGUI.DisabledScope(hasErrors))
            {
                ApplyRevertGUI();
            }

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Actions:");
            if (GUILayout.Button("Create Material"))
            {
                if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset == null) CreateMaterial("Huge Texture/Diffuse Array", true);
                else CreateMaterial("Shader Graphs/HugeTexturePBR", true);
            }

            if (GUILayout.Button("Create Huge Raw Image"))
            {
                HugeRawImage hugeRawImage = HugeRawImageCreator.Create();
                Material mat = CreateMaterial("Huge Texture/UI Array", false);
                hugeRawImage.material = mat;
                hugeRawImage.texture = AssetDatabase.LoadAssetAtPath<Texture2DArray>((serializedObject.targetObject as HugeImporter).assetPath);
            }
        }

        protected virtual bool ValidateFields()
        {
            return true;
        }
    }
}