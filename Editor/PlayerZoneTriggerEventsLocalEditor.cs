using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace JanSharp.Internal
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PlayerZoneTriggerEventsLocal))]
    public class PlayerZoneTriggerEventsLocalEditor : Editor
    {
        private SerializedProperty targetsProp;

        private void OnEnable()
        {
            targetsProp = serializedObject.FindProperty(nameof(PlayerZoneTriggerEventsLocal.targets));
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(targets))
                return;

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                GUILayout.Label("Acts as a bridge from Player Zones to scripts using "
                    + "VRChat's OnPlayerTriggerEnter/Exit events.\n"
                    + "Raises events only for the local player entering and exiting.", EditorStyles.wordWrappedLabel);

            serializedObject.Update();
            EditorGUILayout.PropertyField(targetsProp);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
