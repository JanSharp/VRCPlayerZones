using UdonSharp;
using UnityEngine;

namespace JanSharp.Internal
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [SingletonScript("c1f8d923d5aefec2e9886bc941cd0b3b")] // Runtime/Prefabs/Internal/PlayerZonesSyncing.prefab
    public class PlayerZonesSyncing : LockstepGameState
    {
        public override string GameStateInternalName => "jansharp.player-zones";
        public override string GameStateDisplayName => "Player Zones";
        public override bool GameStateSupportsImportExport => false;
        public override uint GameStateDataVersion => 0u;
        public override uint GameStateLowestSupportedDataVersion => 0u;
        public override LockstepGameStateOptionsUI ExportUI => null;
        public override LockstepGameStateOptionsUI ImportUI => null;

        [HideInInspector][SerializeField][SingletonReference] private PlayerZonesManagerAPI zonesManagerAPI;
        private PlayerZonesManager zonesManager;
        [HideInInspector][SerializeField][SingletonReference] private PlayerDataManagerAPI playerDataManager;
        private int zonesPlayerDataIndex;
        private PlayerZonesPlayerData localPlayer;

        private int suspendedIndexInArray = 0;
        private System.Diagnostics.Stopwatch suspensionSw = new System.Diagnostics.Stopwatch();
        private double maxWorkMSPerFrame;

        private void Start()
        {
            zonesManager = (PlayerZonesManager)zonesManagerAPI;
            maxWorkMSPerFrame = lockstep.MaxWorkMSPerFrame;
        }

        [LockstepEvent(LockstepEventType.OnMaxWorkMSPerFrameChanged)]
        public void OnMaxWorkMSPerFrameChanged()
        {
            maxWorkMSPerFrame = lockstep.MaxWorkMSPerFrame;
        }

        private bool LogicIsRunningLong()
        {
            if (suspensionSw.Elapsed.TotalMilliseconds > maxWorkMSPerFrame)
            {
                lockstep.FlagToContinueNextFrame();
                return true;
            }
            return false;
        }

        [PlayerDataEvent(PlayerDataEventType.OnRegisterCustomPlayerData)]
        public void OnRegisterCustomPlayerData()
        {
            playerDataManager.RegisterCustomPlayerData<PlayerZonesPlayerData>(nameof(PlayerZonesPlayerData));
        }

        [PlayerDataEvent(PlayerDataEventType.OnAllCustomPlayerDataRegistered)]
        public void OnAllCustomPlayerDataRegistered()
        {
            zonesPlayerDataIndex = playerDataManager.GetPlayerDataClassNameIndex<PlayerZonesPlayerData>(nameof(PlayerZonesPlayerData));
        }

        [PlayerDataEvent(PlayerDataEventType.OnLocalPlayerDataAvailable)]
        public void OnLocalPlayerDataAvailable()
        {
            localPlayer = (PlayerZonesPlayerData)playerDataManager.LocalPlayerData.customPlayerData[zonesPlayerDataIndex];
        }

        [LockstepEvent(LockstepEventType.OnInit)]
        public void OnInit()
        {
            int count = zonesManager.activeZonesCount;
            for (int i = 0; i < count; i++)
                HandlePlayerEnteringZoneInGS(zonesManager.activeZones[i], localPlayer, isInIA: false);
        }

        [LockstepEvent(LockstepEventType.OnClientBeginCatchUp)]
        public void OnClientBeginCatchUp()
        {
            PlayerZone[] allZones = zonesManager.allZones;
            int allZonesCount = allZones.Length;
            while (suspendedIndexInArray < allZonesCount)
            {
                PlayerZone zone = allZones[suspendedIndexInArray++];
                if (zone == null || !zone.requiresSyncing)
                    continue;
                if (LogicIsRunningLong())
                    return;
                PlayerZonesPlayerData[] playersInZone = zone.playersInZone;
                int count = zone.playersInZoneCount;
                for (int i = 0; i < count; i++)
                    zone.RaiseOnPlayerAlreadyInZone(playersInZone[i]);
            }
            suspendedIndexInArray = 0;
        }

        public ulong SendPlayerEnterZoneIA(PlayerZone zone)
        {
            lockstep.WriteSmallUInt((uint)zone.allZonesIndex);
            return lockstep.SendInputAction(playerEnterZoneIAId);
        }

        [HideInInspector][SerializeField] private uint playerEnterZoneIAId;
        [LockstepInputAction(nameof(playerEnterZoneIAId))]
        public void OnPlayerEnterZoneIA()
        {
            PlayerZone zone = zonesManager.allZones[lockstep.ReadSmallUInt()];
            PlayerZonesPlayerData player = (PlayerZonesPlayerData)playerDataManager.SendingPlayerData.customPlayerData[zonesPlayerDataIndex];
            HandlePlayerEnteringZoneInGS(zone, player, isInIA: true);
        }

        private void HandlePlayerEnteringZoneInGS(PlayerZone zone, PlayerZonesPlayerData player, bool isInIA)
        {
            ArrList.Add(ref player.zonesThisPlayerIsIn, ref player.zonesThisPlayerIsInCount, zone);
            ArrList.Add(ref zone.playersInZone, ref zone.playersInZoneCount, player);
            if (player.core.isLocal)
                zone.localPlayerIsInZoneInGS = true;
            zone.RaiseOnPlayerZoneEnter(player, isInIA);
        }

        public ulong SendPlayerExitZoneIA(PlayerZone zone)
        {
            lockstep.WriteSmallUInt((uint)zone.allZonesIndex);
            return lockstep.SendInputAction(playerExitZoneIAId);
        }

        [HideInInspector][SerializeField] private uint playerExitZoneIAId;
        [LockstepInputAction(nameof(playerExitZoneIAId))]
        public void OnPlayerExitZoneIA()
        {
            PlayerZone zone = zonesManager.allZones[lockstep.ReadSmallUInt()];
            PlayerZonesPlayerData player = (PlayerZonesPlayerData)playerDataManager.SendingPlayerData.customPlayerData[zonesPlayerDataIndex];
            ArrList.Remove(ref player.zonesThisPlayerIsIn, ref player.zonesThisPlayerIsInCount, zone);
            ArrList.Remove(ref zone.playersInZone, ref zone.playersInZoneCount, player);
            if (player.core.isLocal)
                zone.localPlayerIsInZoneInGS = false;
            zone.RaiseOnPlayerZoneExit(player, eventIsInIA: true);
        }

        private void WritePlayerZoneData(PlayerZone zone)
        {
            PlayerZonesPlayerData[] playersInZone = zone.playersInZone;
            int count = zone.playersInZoneCount;

            lockstep.WriteSmallUInt((uint)count);
            for (int i = 0; i < count; i++)
                playerDataManager.WriteCorePlayerDataRef(playersInZone[i].core);
        }

        private void ReadPlayerZoneData(PlayerZone zone)
        {
            int count = (int)lockstep.ReadSmallUInt();
            zone.playersInZoneCount = count;
            ArrList.EnsureCapacity(ref zone.playersInZone, count);
            PlayerZonesPlayerData[] playersInZone = zone.playersInZone;

            zone.localPlayerIsInZoneInGS = false;
            for (int i = 0; i < count; i++)
            {
                CorePlayerData core = playerDataManager.ReadCorePlayerDataRef();
                if (core.isLocal)
                    zone.localPlayerIsInZoneInGS = true;
                playersInZone[i] = (PlayerZonesPlayerData)core.customPlayerData[zonesPlayerDataIndex];
            }
        }

        public override void SerializeGameState(bool isExport, LockstepGameStateOptionsData exportOptions)
        {
            foreach (PlayerZone zone in zonesManager.allZones)
            {
                // Non synced zones are allowed to get destroyed at runtime, therefore check for null.
                if (zone != null && zone.requiresSyncing)
                    WritePlayerZoneData(zone);
            }
        }

        public override string DeserializeGameState(bool isImport, uint importedDataVersion, LockstepGameStateOptionsData importOptions)
        {
            foreach (PlayerZone zone in zonesManager.allZones)
            {
                // Non synced zones are allowed to get destroyed at runtime, therefore check for null.
                if (zone != null && zone.requiresSyncing)
                    ReadPlayerZoneData(zone);
            }
            return null;
        }
    }
}
