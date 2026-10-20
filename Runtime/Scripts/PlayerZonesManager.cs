using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace JanSharp.Internal
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(10)]
    public class PlayerZonesManager : PlayerZonesManagerAPI
    {
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public const string AutoZonesLayerPropName = nameof(autoZonesLayer);
        public const string ZonesLayerIndexPropName = nameof(zonesLayerIndex);
        public bool AutoZonesLayerIndex => autoZonesLayer;
        public int ZonesLayerIndex => zonesLayerIndex;
        public PlayerZonesSyncing ZonesSyncing => zonesSyncing;
#endif

        [HideInInspector][SerializeField][SingletonReference] private LockstepAPI lockstep;
        [HideInInspector][SerializeField][SingletonReference(Optional = true)] private PlayerZonesSyncing zonesSyncing;
        [HideInInspector][FindInScene] public PlayerZone[] allZones;
        public override PlayerZoneAPI[] AllZones => allZones;

        [SerializeField] private bool autoZonesLayer = true;
        [SerializeField] private int zonesLayerIndex;
        private int zonesLayerMask;
        private const float HeadCheckRadius = 0.2f;

        private Vector3 pervHeadPosition;
        private float nextCheckTime;
        private const float MaxCheckInterval = 0.2f;
        // Keeping it notably smaller than the diameter to keep the chance very high that the previous and current
        // checks overlap even when moving quickly, to avoid phasing through thin colliders without it getting detected.
        private const float MinMovementForAnotherCheck = HeadCheckRadius * 1.25f;

        private VRCPlayerApi localPlayerAPI;

        private Collider[] hitColliders = new Collider[MaxSimultaneousZones];
        // Must be set here, not in Start, to make the API never return null.
        [System.NonSerialized] public PlayerZone[] activeZones = new PlayerZone[MaxSimultaneousZones];
        [System.NonSerialized] public int activeZonesCount = 0;
        private string isInZoneFieldName;
        private PlayerZone[] flipBuffer = new PlayerZone[MaxSimultaneousZones];
        private int flipBufferCount = 0;
        private string isInZoneFlipFieldName;

        public override PlayerZoneAPI[] ZonesLocalPlayerIsIn => activeZones;
        public override int ZonesLocalPlayerIsInCount => activeZonesCount;

        private DataDictionary zonesByTransform;

#if PLAYER_ZONES_STOPWATCH
        [HideInInspector][SerializeField][SingletonReference] private QuickDebugUI qd;
        private System.Diagnostics.Stopwatch updateSw = new System.Diagnostics.Stopwatch();
        private object[] updateSwData;
#endif

        private void Start()
        {
            localPlayerAPI = Networking.LocalPlayer;
            nextCheckTime = Time.time;

            isInZoneFieldName = nameof(PlayerZone.localPlayerIsInZone);
            isInZoneFlipFieldName = nameof(PlayerZone.localPlayerIsInZoneFlip);
            zonesLayerMask = 1 << zonesLayerIndex;

            BuildZoneIndexesByTransformDictionary();

#if PLAYER_ZONES_STOPWATCH
            updateSwData = StopwatchUtil.CreateDataContainer();
#endif
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
                zonesByTransform.SetValue(zone.transform, zone);
                foreach (PlayerZoneExpansion expansion in zone.Expansions)
                    if (expansion != null)
                        zonesByTransform.SetValue(expansion.transform, zone);
            }
        }

        public override void WritePlayerZoneRef(PlayerZoneAPI zone)
        {
            lockstep.WriteSmallUInt(zone == null
                ? 0u
                : (uint)((PlayerZone)zone).indexInAllZones + 1u);
        }

        public override PlayerZoneAPI ReadPlayerZoneRef()
        {
            uint id = lockstep.ReadSmallUInt();
            return id == 0u ? null : allZones[id - 1u];
        }

#if PLAYER_ZONES_STOPWATCH
        private void Update()
        {
            updateSw.Restart();
            UpdateLogic();
            updateSw.Stop();
            qd.ShowForOneFrame(this, "Update MS", StopwatchUtil.FormatAvgMinMax(updateSw, updateSwData));
        }
#endif

#if PLAYER_ZONES_STOPWATCH
        private void UpdateLogic()
#else
        private void Update()
#endif
        {
            Vector3 headPosition = localPlayerAPI.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            float time = Time.time;
            if (time < nextCheckTime && Vector3.Distance(pervHeadPosition, headPosition) < MinMovementForAnotherCheck)
                return;
            nextCheckTime = time + MaxCheckInterval;
            pervHeadPosition = headPosition;
            CheckForZones(headPosition);
        }

        private void CheckForZones(Vector3 headPosition)
        {
            int hitCount = Physics.OverlapSphereNonAlloc(headPosition, HeadCheckRadius, hitColliders, zonesLayerMask, QueryTriggerInteraction.Collide);
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
