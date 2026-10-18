using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerZoneTriggerEventsLocal : UdonSharpBehaviour
    {
        public UdonBehaviour[] targets;
        private VRCPlayerApi localPlayer;

        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
        }

        [PlayerZoneEvent(PlayerZoneEventType.OnLocalPlayerZoneEnter)]
        public void OnLocalPlayerZoneEnter()
        {
            foreach (UdonBehaviour target in targets)
                if (target != null)
                {
                    target.SetProgramVariable("onPlayerTriggerEnterPlayer", localPlayer);
                    target.SendCustomEvent("_onPlayerTriggerEnter");
                }
        }

        [PlayerZoneEvent(PlayerZoneEventType.OnLocalPlayerZoneExit)]
        public void OnLocalPlayerZoneExit()
        {
            foreach (UdonBehaviour target in targets)
                if (target != null)
                {
                    target.SetProgramVariable("onPlayerTriggerExitPlayer", localPlayer);
                    target.SendCustomEvent("_onPlayerTriggerExit");
                }
        }
    }
}
