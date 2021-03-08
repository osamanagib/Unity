/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Reflection;
using InfinityCode.HugeTexture.Components;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.HugeTexture.Editors
{
    [CustomEditor(typeof(HugeTextureBridge))]
    public class HugeTextureBridgeEditor:Editor
    {
        private SerializedProperty texture;
        private SerializedProperty cols;
        private SerializedProperty rows;
        private SerializedProperty component;
        private SerializedProperty memberName;

        private string[] textureMembers;
        private int memberSelectedIndex = 0;

        private void CacheMembers()
        {
            Object c = component.objectReferenceValue;
            if (c == null)
            {
                memberSelectedIndex = 0;
                textureMembers = new string[0];
                return;
            }

            List<string> tm = new List<string>();

            Type textureType = typeof(Texture);

            MemberInfo[] members = c.GetType().GetMembers(BindingFlags.Instance| BindingFlags.Public);
            for (int i = 0; i < members.Length; i++)
            {
                MemberInfo m = members[i];
                
                if (m.MemberType == MemberTypes.Field)
                {
                    Type fieldType = (m as FieldInfo).FieldType;
                    if (fieldType == textureType || fieldType.IsSubclassOf(textureType)) tm.Add(m.Name);
                }
                else if (m.MemberType == MemberTypes.Property)
                {
                    PropertyInfo p = m as PropertyInfo;
                    Type propertyType = p.PropertyType;
                    if (propertyType == textureType || propertyType.IsSubclassOf(textureType))
                    {
                        if (p.CanWrite) tm.Add(m.Name);
                    }
                }
            }

            memberSelectedIndex = tm.IndexOf(memberName.stringValue);
            if (memberSelectedIndex == -1)
            {
                memberSelectedIndex = 0;
                if (tm.Count > 0) memberName.stringValue = tm[0];
                else memberName.stringValue = "";
            }

            textureMembers = tm.ToArray();
        }

        private void OnEnable()
        {
            texture = serializedObject.FindProperty("texture");
            cols = serializedObject.FindProperty("cols");
            rows = serializedObject.FindProperty("rows");
            component = serializedObject.FindProperty("component");
            memberName = serializedObject.FindProperty("memberName");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox("If there is the slightest chance, do not use this bridge, because it is very slow, and requires a lot of memory allocation.", MessageType.Warning);

            EditorGUILayout.PropertyField(texture);
            EditorGUILayout.PropertyField(cols);
            EditorGUILayout.PropertyField(rows);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(component);

            if (EditorGUI.EndChangeCheck() || textureMembers == null) CacheMembers();

            EditorGUI.BeginChangeCheck();
            memberSelectedIndex = EditorGUILayout.Popup("Member", memberSelectedIndex, textureMembers);
            if (EditorGUI.EndChangeCheck()) memberName.stringValue = textureMembers[memberSelectedIndex];

            serializedObject.ApplyModifiedProperties();
        }
    }
}