﻿using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

namespace ScriptEditor.Editor
{
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class ScriptEditor : UnityEditor.Editor
    {
        private string _scriptContent;
        private string _infoMessage;
        private MonoScript _monoScript;
        private bool _displayed;

        private Vector2 _scrollPos;

        private void OnEnable()
        {
            MonoBehaviour script = (MonoBehaviour) target;
            _monoScript = MonoScript.FromMonoBehaviour(script);
            _scriptContent = _monoScript.text;
        }

        private GUIContent GetButtonGuiContent(string iconName, string text, string tooltip)
        {
            GUIContent guiContent = new GUIContent(EditorGUIUtility.IconContent(iconName))
                {text = text, tooltip = tooltip};
            return guiContent;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUIStyle editButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 50f,
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
            };
            if (!_displayed &&
                GUILayout.Button(
                    GetButtonGuiContent("d_editicon.sml", "Edit",
                        "Click here to be able to edit your script directly in this inspector"), editButtonStyle))
            {
                _displayed = true;
            }

            if (!_displayed) return;

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos,
                GUILayout.Width(EditorGUIUtility.currentViewWidth - 25),
                GUILayout.MaxHeight(300));
            _scriptContent = EditorGUILayout.TextArea(_scriptContent, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(GetButtonGuiContent("SaveActive", "Save", "Apply your changes to the script")))
            {
                string guid = AssetDatabase.FindAssets(target.GetType().Name).FirstOrDefault();
                string path = AssetDatabase.GUIDToAssetPath(guid);
                File.WriteAllBytes(path, Encoding.UTF8.GetBytes(_scriptContent));
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }

            if (GUILayout.Button(GetButtonGuiContent("d_TreeEditor.Trash", "Discard",
                "Reverts your changes back to the original script")))
            {
                _scriptContent = _monoScript.text;
                EditorGUI.FocusTextInControl(null);
            }

            if (GUILayout.Button(GetButtonGuiContent("ViewToolZoom", "Select",
                "Highlight the script in the project window")))
            {
                EditorGUIUtility.PingObject(_monoScript);
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}