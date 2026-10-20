using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerZoneTriggerEventsLocalAndRemote : PlayerZoneTriggerEventsBase
    {
        [HideInInspector][SerializeField][FindInParent] private PlayerZoneAPI zone;

        [PlayerZoneEvent(PlayerZoneEventType.OnLocalPlayerZoneEnter)]
        public void OnLocalPlayerZoneEnter()
        {
            RaiseOnPlayerTriggerEnter(Networking.LocalPlayer);
        }

        [PlayerZoneEvent(PlayerZoneEventType.OnLocalPlayerZoneExit)]
        public void OnLocalPlayerZoneExit()
        {
            RaiseOnPlayerTriggerExit(Networking.LocalPlayer);
        }

        [PlayerZoneEvent(PlayerZoneEventType.OnPlayerAlreadyInZone)]
        public void OnPlayerAlreadyInZone() => OnPlayerZoneEnter();

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
