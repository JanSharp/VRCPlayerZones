using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace JanSharp.Internal
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PlayerZoneTriggerEventsLocalAndRemote))]
    public class PlayerZoneTriggerEventsLocalAndRemoteEditor : Editor
    {
        private SerializedProperty targetsProp;

        private void OnEnable()
        {
            targetsProp = serializedObject.FindProperty(nameof(PlayerZoneTriggerEventsLocalAndRemote.targets));
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(targets))
                return;

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                GUILayout.Label("Acts as a bridge from Player Zones to scripts using "
                    + "VRChat's OnPlayerTriggerEnter/Exit events.\n"
                    + "Raises events for local and remote players entering and exiting.", EditorStyles.wordWrappedLabel);

            serializedObject.Update();
            EditorGUILayout.PropertyField(targetsProp);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
