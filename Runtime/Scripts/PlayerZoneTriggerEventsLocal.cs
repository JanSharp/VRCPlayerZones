using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerZoneTriggerEventsLocal : PlayerZoneTriggerEventsBase
    {
        [PlayerZoneEvent(PlayerZoneEventType.OnLocalPlayerZoneEnter)]
        public void OnLocalPlayerZoneEnter()
        {
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
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
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            foreach (UdonBehaviour target in targets)
                if (target != null)
                {
                    target.SetProgramVariable("onPlayerTriggerExitPlayer", localPlayer);
                    target.SendCustomEvent("_onPlayerTriggerExit");
                }
        }
    }
}
