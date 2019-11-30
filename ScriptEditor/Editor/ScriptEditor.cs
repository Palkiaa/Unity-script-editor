using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

namespace ScriptEditor.Editor
{
    [CustomEditor(typeof(MonoScript))]
    public class ScriptEditor : UnityEditor.Editor
    {
        private string _scriptContent;
        private string _infoMessage;
        private MonoScript _monoScript;
        private bool _displayed;
        private Vector2 _scrollPos;

        private string _feedbackMessage;
        private Color _feedbackColor;
        private Color DefaultLabelColor => EditorStyles.label.normal.textColor;

        private GUIStyle _buttonStyle;
        private GUIStyle _editButtonStyle;

        private void OnEnable()
        {
            _monoScript = (MonoScript) target;
            _scriptContent = _monoScript.text;
        }

        private GUIContent GetButtonGuiContent(string iconName, string text, string tooltip)
        {
            GUIContent guiContent = new GUIContent(EditorGUIUtility.IconContent(iconName))
            {
                text = text,
                tooltip = tooltip,
            };
            return guiContent;
        }

        public override void OnInspectorGUI()
        {
            SetupStyles();

            DisplayEditButton();

            if (!_displayed) return;

            DisplayCodeArea();
            DisplayFeedbackMessage();

            EditorGUILayout.BeginHorizontal();
            DisplaySaveButton();
            DisplayDiscardButton();
            DisplayCloseButton();
            EditorGUILayout.EndHorizontal();
        }

        private void SetupStyles()
        {
            _editButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 50f,
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
            };
            _buttonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 30f,
                fontSize = 11,
            };
        }

        private void DisplayEditButton()
        {
            if (!_displayed &&
                GUILayout.Button(
                    GetButtonGuiContent("d_editicon.sml", "Edit",
                        "Click here to be able to edit your script directly in this inspector"), _editButtonStyle))
            {
                SetFeedbackMessage(null, DefaultLabelColor);
                _displayed = true;
            }
        }

        private void DisplayCodeArea()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos,
                GUILayout.Width(EditorGUIUtility.currentViewWidth - 25),
                GUILayout.MaxHeight(300));
            try
            {
                _scriptContent = EditorGUILayout.TextArea(_scriptContent, GUILayout.ExpandHeight(true));
            }
            catch (Exception)
            {
                // TODO : For some reason, I get a NullReferenceException sometimes in the TextArea, even though all is set
                // ignored
            }

            EditorGUILayout.EndScrollView();
        }

        private void SetFeedbackMessage(string value, Color color)
        {
            _feedbackMessage = value;
            _feedbackColor = color;
        }

        private void DisplayFeedbackMessage()
        {
            EditorGUILayout.LabelField(_feedbackMessage,
                new GUIStyle(EditorStyles.label) {wordWrap = true, normal = {textColor = _feedbackColor}});
        }

        private void DisplaySaveButton()
        {
            if (GUILayout.Button(GetButtonGuiContent("SaveActive", "Save", "Apply your changes to the script"),
                _buttonStyle))
            {
                string scriptPath = AssetDatabase.GetAssetPath(target);

                // Write new content into the script file
                File.WriteAllBytes(scriptPath, Encoding.UTF8.GetBytes(_scriptContent));

                // Refresh to force recompile
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

                // Intercept logs to display a feedback
                Application.logMessageReceived += (condition, trace, type) =>
                {
                    switch (type)
                    {
                        case LogType.Error:
                            SetFeedbackMessage("There are some compile errors ! (" + condition + ")",
                                new Color(0.45f, 0.04f, 0.05f));
                            break;
                        default:
                            SetFeedbackMessage("Saved script !", new Color(0.11f, 0.45f, 0.11f));
                            break;
                    }
                };
            }
        }

        private void Discard()
        {
            // Get back to the original content
            _scriptContent = _monoScript.text;

            // Focus out the text area
            EditorGUI.FocusTextInControl(null);
        }

        private void DisplayDiscardButton()
        {
            if (GUILayout.Button(GetButtonGuiContent("d_TreeEditor.Trash", "Discard",
                "Reverts your changes back to the original script"), _buttonStyle))
            {
                Discard();

                SetFeedbackMessage("Changes discarded", DefaultLabelColor);
            }
        }

        private void DisplayCloseButton()
        {
            if (GUILayout.Button(GetButtonGuiContent("LookDevClose@2x", "Close",
                "Close the edit area (also discards any unsaved changes)"), _buttonStyle))
            {
                Discard();
                _displayed = false;
            }
        }
    }
}