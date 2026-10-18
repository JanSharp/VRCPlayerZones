using UdonSharp;
using UnityEngine;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerZoneLocalTest : UdonSharpBehaviour
    {
        [HideInInspector][SerializeField][FindInParent] private PlayerZoneAPI zone;

        [PlayerZoneEvent(PlayerZoneEventType.OnLocalPlayerZoneEnter)]
        public void OnLocalPlayerZoneEnter()
        {
            Debug.Log($"[PlayerZoneDebug] OnLocalPlayerZoneEnter - zone.UniqueIdForLocalPlayerEvent: 0x{zone.UniqueIdForLocalPlayerEvent:x16}");
        }

        [PlayerZoneEvent(PlayerZoneEventType.OnLocalPlayerZoneExit)]
        public void OnLocalPlayerZoneExit()
        {
            Debug.Log($"[PlayerZoneDebug] OnLocalPlayerZoneExit - zone.UniqueIdForLocalPlayerEvent: 0x{zone.UniqueIdForLocalPlayerEvent:x16}");
        }
    }
}
