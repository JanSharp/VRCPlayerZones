using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace JanSharp.Internal
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(10)]
    [CustomRaisedEventsDispatcher(typeof(PlayerZonesManagerEventAttribute), typeof(PlayerZonesManagerEventType))]
    public class PlayerZonesManager : PlayerZonesManagerAPI
    {
        [HideInInspector][FindInScene] public PlayerZone[] allZones;
        public PlayerZonesSyncing zonesSyncing;

        [SerializeField] private LayerMask zonesLayer;
        private const float HeadCheckRadius = 0.2f;

        private VRCPlayerApi localPlayerAPI;

        private Collider[] hitColliders = new Collider[64];
        [System.NonSerialized] public PlayerZone[] activeZones;
        [System.NonSerialized] public int activeZonesCount = 0;
        private string isInZoneFieldName;
        private PlayerZone[] flipBuffer;
        private int flipBufferCount = 0;
        private string isInZoneFlipFieldName;

        private DataDictionary zonesByTransform;

        private void Start()
        {
            localPlayerAPI = Networking.LocalPlayer;

            int allZonesCount = allZones.Length;
            activeZones = new PlayerZone[allZonesCount];
            flipBuffer = new PlayerZone[allZonesCount];
            isInZoneFieldName = nameof(PlayerZone.localPlayerIsInZone);
            isInZoneFlipFieldName = nameof(PlayerZone.localPlayerIsInZoneFlip);

            BuildZoneIndexesByTransformDictionary();
        }

        private void BuildZoneIndexesByTransformDictionary()
        {
            zonesByTransform = new DataDictionary();
            int allZonesCount = allZones.Length;
            for (int i = 0; i < allZonesCount; i++)
            {
                PlayerZone zone = allZones[i];
                if (zone == null)
                    continue;
                zone.allZonesIndex = i;
                zonesByTransform.SetValue(zone.transform, zone);
                foreach (PlayerZoneExpansion expansion in zone.Expansions)
                    if (expansion != null)
                        zonesByTransform.SetValue(expansion.transform, zone);
            }
        }

        private void Update()
        {
            Vector3 headPosition = localPlayerAPI.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            int hitCount = Physics.OverlapSphereNonAlloc(headPosition, HeadCheckRadius, hitColliders, zonesLayer, QueryTriggerInteraction.Collide);
            for (int i = 0; i < hitCount; i++)
            {
                Collider collider = hitColliders[i];
                if (collider == null || !zonesByTransform.TryGetValue(collider.transform, out DataToken zoneToken))
                    continue;

                PlayerZone zone = (PlayerZone)zoneToken.Reference;
                if ((bool)zone.GetProgramVariable(isInZoneFlipFieldName))
                    continue; // Already found in this iteration.
                zone.SetProgramVariable(isInZoneFlipFieldName, true);
                flipBuffer[flipBufferCount++] = zone;

                if ((bool)zone.GetProgramVariable(isInZoneFieldName))
                    continue; // Was already in zone.

                ulong uniqueId = zone.requiresSyncing ? zonesSyncing.SendPlayerEnterZoneIA(zone) : 0uL;
                zone.RaiseOnLocalPlayerZoneEnter(uniqueId);
            }

            for (int i = 0; i < activeZonesCount; i++)
            {
                PlayerZone zone = activeZones[i];
                zone.SetProgramVariable(isInZoneFieldName, false);
                if ((bool)zone.GetProgramVariable(isInZoneFlipFieldName))
                    continue; // Also found in current iteration, still in zone.

                ulong uniqueId = zone.requiresSyncing ? zonesSyncing.SendPlayerExitZoneIA(zone) : 0uL;
                zone.RaiseOnLocalPlayerZoneExit(uniqueId);
            }

            ReplaceActiveZonesWithFlipBuffer();
        }

        private void ReplaceActiveZonesWithFlipBuffer()
        {
            PlayerZone[] temp1 = activeZones;
            activeZones = flipBuffer;
            flipBuffer = temp1;

            activeZonesCount = flipBufferCount;
            flipBufferCount = 0;

            string temp2 = isInZoneFieldName;
            isInZoneFieldName = isInZoneFlipFieldName;
            isInZoneFlipFieldName = temp2;
        }
    }
}
