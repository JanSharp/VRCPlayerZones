using UdonSharp;
using UnityEngine;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerZoneRemoteTest : UdonSharpBehaviour
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

        [PlayerZoneEvent(PlayerZoneEventType.OnPlayerZoneEnter)]
        public void OnPlayerZoneEnter()
        {
            Debug.Log($"[PlayerZoneDebug] OnPlayerZoneEnter - zone.PlayerForEvent.core.displayName: {zone.PlayerForEvent.core.displayName}");
        }

        [PlayerZoneEvent(PlayerZoneEventType.OnPlayerAlreadyInZone)]
        public void OnPlayerAlreadyInZone()
        {
            Debug.Log($"[PlayerZoneDebug] OnPlayerAlreadyInZone - zone.PlayerForEvent.core.displayName: {zone.PlayerForEvent.core.displayName}");
        }

        [PlayerZoneEvent(PlayerZoneEventType.OnPlayerZoneExit)]
        public void OnPlayerZoneExit()
        {
            Debug.Log($"[PlayerZoneDebug] OnPlayerZoneExit - zone.PlayerForEvent.core.displayName: {zone.PlayerForEvent.core.displayName}");
        }
    }
}
