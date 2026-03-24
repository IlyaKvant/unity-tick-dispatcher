using UnityEditor;

namespace UnityTickDispatcher.Editor
{
    [CustomEditor(typeof(TickManager))]
    public class TickManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            #if VCONTAINER_SUPPORT
            EditorGUILayout.HelpBox(
                "VContainer is available. Use RegisterEntryPoint<TickDispatcher> instead of TickManager.",
                MessageType.Warning
            );
            #else
            EditorGUILayout.PropertyField(serializedObject.FindProperty("loopTiming"));
            #endif

            serializedObject.ApplyModifiedProperties();
        }
    }
}