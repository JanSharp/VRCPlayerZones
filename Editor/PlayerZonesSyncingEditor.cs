using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace JanSharp.Internal
{
    public static class PlayerZonesSyncingOnBuild
    {
        private static PlayerZonesSyncing zonesSyncing;

        [OrderedInitializeOnLoad]
        private static void OnAssemblyLoad()
        {
            OnBuildUtil.RegisterTypeCumulative<PlayerZonesSyncing>(OnZonesSyncingBuild, order: -6);
            OnBuildUtil.RegisterType<PlayerZonesManager>(OnManagerBuild, order: -5);
        }

        private static bool OnZonesSyncingBuild(IEnumerable<PlayerZonesSyncing> zonesSyncing)
        {
            PlayerZonesSyncingOnBuild.zonesSyncing = zonesSyncing.FirstOrDefault();
            return true;
        }

        private static bool OnManagerBuild(PlayerZonesManager manager)
        {
            if (PlayerZoneOnBuild.AtLeastOneZoneRequiresSyncing)
                EnsureHasSyncingScript(manager);
            else
                EnsureNoSyncingScript();
            return true;
        }

        private static void EnsureHasSyncingScript(PlayerZonesManager manager)
        {
            if (zonesSyncing == null)
            {
                SingletonScriptEditor.EnsureSingletonExists<PlayerZonesSyncing>(); // Triggers a rerun.
                return;
            }

            if (zonesSyncing.transform.parent != manager.transform)
                Undo.SetTransformParent(zonesSyncing.transform, manager.transform, $"Make {nameof(PlayerZonesSyncing)} a child of the manager");
        }

        private static void EnsureNoSyncingScript()
        {
            if (zonesSyncing == null)
                return;
            OnBuildUtil.UndoDestroyObjectImmediate(zonesSyncing.gameObject);
        }
    }
}
