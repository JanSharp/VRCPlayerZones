using JanSharp.Internal;
using UdonSharp;
using UnityEngine;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerZonesPlayerData : CustomPlayerData
    {
        public override string PlayerDataInternalName => "jansharp.player-zones";
        public override string PlayerDataDisplayName => "Player Zones";
        public override bool SupportsImportExport => false;
        public override uint DataVersion => 0u;
        public override uint LowestSupportedDataVersion => 0u;

        [HideInInspector][SerializeField][SingletonReference] private PlayerZonesManagerAPI zonesManager;

        [System.NonSerialized] public PlayerZoneAPI[] zonesThisPlayerIsIn = new PlayerZoneAPI[ArrList.MinCapacity];
        [System.NonSerialized] public int zonesThisPlayerIsInCount = 0;

        public override bool PersistPlayerDataWhileOffline() => false;

        public override void OnPlayerDataUninit(bool forced)
        {
            while (zonesThisPlayerIsInCount > 0)
            {
                zonesThisPlayerIsInCount--;
                PlayerZone zone = (PlayerZone)zonesThisPlayerIsIn[zonesThisPlayerIsInCount];
                ArrList.Remove(ref zone.playersInZone, ref zone.playersInZoneCount, this);
                // Leave localPlayerIsInZoneInGS untouched, OnPlayerDataUninit never runs for the local player's data.
                zone.RaiseOnPlayerZoneExit(this, eventIsInIA: false);
            }
        }

        public override void Serialize(bool isExport)
        {
            // Syncing both the lists of zones a player is in (which happens here)
            // as well as the list of players in a zone (which happens in the syncing GS)
            // specifically to make the order within the arrays match, to be deterministic.
            // This is the easiest way to do it, and it comes at no constant runtime cost, just late joiner cost.
            lockstep.WriteSmallUInt((uint)zonesThisPlayerIsInCount);
            for (int i = 0; i < zonesThisPlayerIsInCount; i++)
                lockstep.WriteSmallUInt((uint)((PlayerZone)zonesThisPlayerIsIn[i]).allZonesIndex);
        }

        public override void Deserialize(bool isImport, uint importedDataVersion)
        {
            zonesThisPlayerIsInCount = (int)lockstep.ReadSmallUInt();
            ArrList.EnsureCapacity(ref zonesThisPlayerIsIn, zonesThisPlayerIsInCount);
            PlayerZone[] allZones = ((PlayerZonesManager)zonesManager).allZones;
            for (int i = 0; i < zonesThisPlayerIsInCount; i++)
                zonesThisPlayerIsIn[i] = allZones[lockstep.ReadSmallUInt()];
        }
    }
}
