using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace JanSharp.Internal
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PlayerZoneExpansion))]
    public class PlayerZoneExpansionEditor : Editor
    {
        private SerializedProperty zoneProp;

        private void OnEnable()
        {
            zoneProp = serializedObject.FindProperty(nameof(PlayerZoneExpansion.zone));
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(targets))
                return;

            using (new GUILayout.HorizontalScope())
            {
                serializedObject.Update();
                using (new EditorGUI.DisabledGroupScope(disabled: true))
                    EditorGUILayout.PropertyField(zoneProp, new GUIContent("Player Zone", "Automatically found in the parents of the Expansion"));

                if (GUILayout.Button(new GUIContent("Find", "Runs all OnBuild handlers"), GUILayout.MaxWidth(60f)))
                    OnBuildUtil.RunOnBuild(showDialogOnFailure: true, useSceneViewNotification: false, abortIfScriptsGotInstantiated: false);
            }
        }
    }
}
