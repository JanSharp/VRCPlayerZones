using System.Collections.Generic;
using System.Linq;
using UdonSharp;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace JanSharp.Internal
{
    public static class PlayerZoneOnBuild
    {
        public static bool AtLeastOneZoneRequiresSyncing { get; private set; }

        [OrderedInitializeOnLoad]
        private static void OnAssemblyLoad()
        {
            OnBuildUtil.RegisterTypeCumulative<PlayerZone>(OnBuild, order: -10);
            OnBuildUtil.RegisterTypeCumulative<PlayerZoneExpansion>(OnExpansionBuild, order: -10);
        }

        private static bool OnExpansionBuild(IEnumerable<PlayerZoneExpansion> expansions)
        {
            PlayerZonesManagerOnBuild.SetPlayerZonesLayerAndApply(expansions.Select(z => z.gameObject).ToArray());
            return true;
        }

        private static bool OnBuild(IEnumerable<PlayerZone> zones)
        {
            if (!ValidateNoNesting(zones))
                return false;

            PlayerZonesManagerOnBuild.SetPlayerZonesLayerAndApply(zones.Select(z => z.gameObject).ToArray());

            List<PlayerZone> syncedZones = new();
            List<PlayerZone> localZones = new();
            foreach (PlayerZone zone in zones)
            {
                if ((zone.OnPlayerZoneEnterListeners?.Length ?? 0) != 0
                    || (zone.OnPlayerZoneExitListeners?.Length ?? 0) != 0
                    || (zone.OnPlayerAlreadyInZoneListeners?.Length ?? 0) != 0)
                {
                    syncedZones.Add(zone);
                }
                else
                    localZones.Add(zone);
            }
            SetRequiresSyncing(syncedZones.ToArray(), true);
            SetRequiresSyncing(localZones.ToArray(), false);
            AtLeastOneZoneRequiresSyncing = syncedZones.Count != 0;
            return true;
        }

        private static bool ValidateNoNesting(IEnumerable<PlayerZone> zones)
        {
            bool valid = true;
            foreach (PlayerZone zone in zones.Where(z => z.transform.parent?.GetComponentInParent<PlayerZone>() != null))
            {
                Debug.LogError($"[PlayerZones] Nested Player Zones are not supported and disallowed.", zone);
                valid = false;
            }
            return valid;
        }

        private static void SetRequiresSyncing(PlayerZone[] zones, bool requiresSyncing)
        {
            if (zones.Length == 0)
                return;
            SerializedObject so = new(zones);
            so.FindProperty(nameof(PlayerZone.requiresSyncing)).boolValue = requiresSyncing;
            so.ApplyModifiedProperties();
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(PlayerZone))]
    public class PlayerZoneEditor : Editor
    {
        private static bool listenersFoldedOut = false;
        private SerializedProperty requiresSyncingProp;

        private void OnEnable()
        {
            requiresSyncingProp = serializedObject.FindProperty(nameof(PlayerZone.requiresSyncing));
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(targets))
                return;

            if (GUILayout.Button(new GUIContent("Update Readonly Info Below", "Runs all OnBuild handlers\nInfo for EditorOnly PlayerZones will be empty")))
                OnBuildUtil.RunOnBuild(showDialogOnFailure: true, useSceneViewNotification: false, abortIfScriptsGotInstantiated: false);

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            using (new EditorGUI.IndentLevelScope())
            {
                serializedObject.Update();

                EditorGUI.showMixedValue = requiresSyncingProp.hasMultipleDifferentValues;
                EditorGUILayout.Toggle(
                    new GUIContent("Syncs Remote Players", "Enabled whenever any of the listeners listen to remote player zone events"),
                    requiresSyncingProp.boolValue);
                EditorGUI.showMixedValue = false;

                if (listenersFoldedOut = EditorGUILayout.Foldout(listenersFoldedOut, "Listeners", toggleOnLabelClick: true))
                    foreach (UdonSharpBehaviour listener in targets.Cast<PlayerZone>().SelectMany(EnumerateAllListeners).Distinct())
                        EditorGUILayout.ObjectField(listener, typeof(UdonSharpBehaviour), allowSceneObjects: true);
            }
        }

        private static IEnumerable<UdonSharpBehaviour> EnumerateAllListeners(PlayerZone zone)
        {
            if (zone.OnLocalPlayerZoneEnterListeners != null)
                foreach (UdonSharpBehaviour listener in zone.OnLocalPlayerZoneEnterListeners)
                    yield return listener;
            if (zone.OnLocalPlayerZoneExitListeners != null)
                foreach (UdonSharpBehaviour listener in zone.OnLocalPlayerZoneExitListeners)
                    yield return listener;
            if (zone.OnPlayerZoneEnterListeners != null)
                foreach (UdonSharpBehaviour listener in zone.OnPlayerZoneEnterListeners)
                    yield return listener;
            if (zone.OnPlayerZoneExitListeners != null)
                foreach (UdonSharpBehaviour listener in zone.OnPlayerZoneExitListeners)
                    yield return listener;
            if (zone.OnPlayerAlreadyInZoneListeners != null)
                foreach (UdonSharpBehaviour listener in zone.OnPlayerAlreadyInZoneListeners)
                    yield return listener;
        }
    }
}
