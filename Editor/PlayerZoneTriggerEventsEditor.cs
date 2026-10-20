using System.Linq;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using VRC.Udon;

namespace JanSharp
{
    public static class PlayerZoneTriggerEventsBaseOnBuild
    {
        [OrderedInitializeOnLoad]
        private static void OnAssemblyLoad()
        {
            OnBuildUtil.RegisterType<PlayerZoneTriggerEventsBase>(OnBuild, order: -5);
        }

        private static bool OnBuild(PlayerZoneTriggerEventsBase bridge)
        {
            if (bridge.AutoPopulateTargets)
                PopulateTargetsFromObject(bridge);
            return true;
        }

        public static void PopulateTargetsFromObject(PlayerZoneTriggerEventsBase bridge)
        {
            // These can be null, it'll compare in the condition down below just fine.
            UdonBehaviour bridgeBacking = UdonSharpEditorUtility.GetBackingUdonBehaviour(bridge);
            UdonBehaviour zoneBacking = bridge.TryGetComponent(out PlayerZoneAPI zone)
                ? UdonSharpEditorUtility.GetBackingUdonBehaviour(zone)
                : null;

            SerializedObject so = new(bridge);
            EditorUtil.SetArrayProperty(
                so.FindProperty(nameof(PlayerZoneTriggerEventsBase.targets)),
                bridge.GetComponents<UdonBehaviour>()
                    .Where(ub => ub != bridgeBacking && ub != zoneBacking)
                    .ToList(),
                (p, v) => p.objectReferenceValue = v);
            so.ApplyModifiedProperties();
        }
    }

    public abstract class PlayerZoneTriggerEventsBaseEditor : Editor
    {
        protected abstract string InfoAboutWhichEventsGetRaised { get; }

        private SerializedProperty autoPopulateTargetsProp;
        private SerializedProperty targetsProp;

        private void OnEnable()
        {
            autoPopulateTargetsProp = serializedObject.FindProperty(PlayerZoneTriggerEventsBase.AutoPopulateTargetsPropName);
            targetsProp = serializedObject.FindProperty(nameof(PlayerZoneTriggerEventsBase.targets));
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(targets))
                return;

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                GUILayout.Label("Acts as a bridge from Player Zones to scripts using "
                    + "VRChat's OnPlayerTriggerEnter/Exit events.\n"
                    + InfoAboutWhichEventsGetRaised, EditorStyles.wordWrappedLabel);

            serializedObject.Update();

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(autoPopulateTargetsProp);

                if (GUILayout.Button(
                    new GUIContent("Populate Targets", "With other scripts which are also specifically on this object"),
                    GUILayout.MaxWidth(130f)))
                {
                    foreach (PlayerZoneTriggerEventsBase bridge in targets.Cast<PlayerZoneTriggerEventsBase>())
                        PlayerZoneTriggerEventsBaseOnBuild.PopulateTargetsFromObject(bridge);
                }
            }

            bool someUseAutoPopulation = autoPopulateTargetsProp.hasMultipleDifferentValues || autoPopulateTargetsProp.boolValue;
            using (new EditorGUI.DisabledGroupScope(disabled: someUseAutoPopulation))
                EditorGUILayout.PropertyField(targetsProp);

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(PlayerZoneTriggerEventsLocal))]
    public class PlayerZoneTriggerEventsLocalEditor : PlayerZoneTriggerEventsBaseEditor
    {
        protected override string InfoAboutWhichEventsGetRaised
            => "Raises events only for the local player entering and exiting.";
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(PlayerZoneTriggerEventsLocalAndRemote))]
    public class PlayerZoneTriggerEventsLocalAndRemoteEditor : PlayerZoneTriggerEventsBaseEditor
    {
        protected override string InfoAboutWhichEventsGetRaised
            => "Raises events for local and remote players entering and exiting.";
    }
}
