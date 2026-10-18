using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerZoneTriggerEventsLocalAndRemote : UdonSharpBehaviour
    {
        [HideInInspector][SerializeField][FindInParent] private PlayerZoneAPI zone;

        public UdonBehaviour[] targets;
        private VRCPlayerApi localPlayer;

        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
        }

        [PlayerZoneEvent(PlayerZoneEventType.OnLocalPlayerZoneEnter)]
        public void OnLocalPlayerZoneEnter()
        {
            RaiseOnPlayerTriggerEnter(localPlayer);
        }

        [PlayerZoneEvent(PlayerZoneEventType.OnLocalPlayerZoneExit)]
        public void OnLocalPlayerZoneExit()
        {
            RaiseOnPlayerTriggerExit(localPlayer);
        }

        [PlayerZoneEvent(PlayerZoneEventType.OnPlayerZoneEnter)]
        public void OnPlayerZoneEnter()
        {
            CorePlayerData player = zone.PlayerForEvent.core;
            if (player.isLocal)
                return;
            VRCPlayerApi playerApi = player.playerApi;
            if (playerApi != null)
                RaiseOnPlayerTriggerEnter(playerApi);
        }

        [PlayerZoneEvent(PlayerZoneEventType.OnPlayerAlreadyInZone)]
        public void OnPlayerAlreadyInZone() => OnPlayerZoneEnter();

        [PlayerZoneEvent(PlayerZoneEventType.OnPlayerZoneExit)]
        public void OnPlayerZoneExit()
        {
            CorePlayerData player = zone.PlayerForEvent.core;
            if (player.isLocal)
                return;
            VRCPlayerApi playerApi = player.playerApi;
            if (playerApi != null)
                RaiseOnPlayerTriggerExit(playerApi);
        }

        private void RaiseOnPlayerTriggerEnter(VRCPlayerApi player)
        {
            foreach (UdonBehaviour target in targets)
                if (target != null)
                {
                    target.SetProgramVariable("onPlayerTriggerEnterPlayer", player);
                    target.SendCustomEvent("_onPlayerTriggerEnter");
                }
        }

        private void RaiseOnPlayerTriggerExit(VRCPlayerApi player)
        {
            foreach (UdonBehaviour target in targets)
                if (target != null)
                {
                    target.SetProgramVariable("onPlayerTriggerExitPlayer", player);
                    target.SendCustomEvent("_onPlayerTriggerExit");
                }
        }
    }
}
