using System.Collections.Generic;
using System.Linq;
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
        }

        private static bool OnBuild(IEnumerable<PlayerZone> zones)
        {
            if (!ValidateNoNesting(zones))
                return false;

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
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(targets))
                return;

            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
}
