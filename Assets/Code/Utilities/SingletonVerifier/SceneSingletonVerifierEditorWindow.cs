using UnityEditor;
using UnityEngine;

namespace UnityUtils
{
#if UNITY_EDITOR

    public class SceneSingletonVerifierEditorWindow : EditorWindow
    {
        [MenuItem("Tools/Singleton Verifier")]
        public static void Open() => GetWindow<SceneSingletonVerifierEditorWindow>("Singleton Verifier");
        
        private Vector2 _scroll;

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Singleton Scene Verifier", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            DrawEnableButton();
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Required Singletons", EditorStyles.boldLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            foreach (var typeName in SceneSingletonVerifier.GetTypeNames())
            {
                var reference = SceneSingletonVerifier.GetReference(typeName);
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.ObjectField(typeName, reference, typeof(MonoBehaviour), true);
                }
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Recheck Types"))
            {
                SceneSingletonVerifier.RecheckTypes();
            }
        }

        private void DrawEnableButton()
        {
            bool isEnabled = SceneSingletonVerifier.IsEnabled;
            Color originalColor = GUI.backgroundColor;

            GUI.backgroundColor = isEnabled ? Color.green : Color.red;

            if (GUILayout.Button(isEnabled ? "ENABLED" : "DISABLED", GUILayout.Height(30)))
            {
                SceneSingletonVerifier.IsEnabled = !isEnabled;
            }

            GUI.backgroundColor = originalColor;
        }
    }
#endif
}