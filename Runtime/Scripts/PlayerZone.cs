using UdonSharp;
using UnityEngine;

namespace JanSharp.Internal
{
    [DisallowMultipleComponent]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [SingletonDependency(typeof(PlayerZonesManagerAPI))]
    [CustomRaisedEventsDispatcher(typeof(PlayerZoneEventAttribute), typeof(PlayerZoneEventType), FindListenersInChildrenOnly = true)]
    public class PlayerZone : PlayerZoneAPI
    {
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public const string ExpansionsPropName = nameof(expansions);
#endif

        [HideInInspector][SerializeField][FindInChildren] private PlayerZoneExpansion[] expansions;
        [HideInInspector] public bool requiresSyncing; // Set through editor scripting.
        [HideInInspector] public int indexInAllZones; // Set through editor scripting.
        public override PlayerZoneExpansion[] Expansions => expansions;
        public override bool SyncsRemotePlayers => requiresSyncing;
        public override int IndexInAllZones => indexInAllZones;

        // These 2 variables get used in an alternating fashion each iteration, like flip buffers.
        [System.NonSerialized] public bool localPlayerIsInZone;
        [System.NonSerialized] public bool localPlayerIsInZoneFlip;
        public override bool LocalPlayerIsInZone => localPlayerIsInZone || localPlayerIsInZoneFlip;

        [System.NonSerialized] public bool localPlayerIsInZoneInGS;
        public override bool LocalPlayerIsInZoneInGS => localPlayerIsInZoneInGS;

        #region Game State
        [System.NonSerialized] public PlayerZonesPlayerData[] playersInZone = new PlayerZonesPlayerData[ArrList.MinCapacity];
        [System.NonSerialized] public int playersInZoneCount = 0;
        public override PlayerZonesPlayerData[] PlayersInZone => playersInZone;
        public override int PlayersInZoneCount => playersInZoneCount;
        #endregion

        private void OnDestroy()
        {
            if (requiresSyncing)
                Debug.LogError($"[PlayerZones] Destroying PlayerZones which are keeping track of remote players "
                    + $"- are doing syncing - is invalid, not supported and breaks syncing. "
                    + $"Do not do it. Destroyed PlayerZone: {name}", this);
        }

        #region EventDispatcher

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public UdonSharpBehaviour[] OnLocalPlayerZoneEnterListeners => onLocalPlayerZoneEnterListeners;
        public UdonSharpBehaviour[] OnLocalPlayerZoneExitListeners => onLocalPlayerZoneExitListeners;
        public UdonSharpBehaviour[] OnPlayerZoneEnterListeners => onPlayerZoneEnterListeners;
        public UdonSharpBehaviour[] OnPlayerZoneExitListeners => onPlayerZoneExitListeners;
        public UdonSharpBehaviour[] OnPlayerAlreadyInZoneListeners => onPlayerAlreadyInZoneListeners;
#endif

        [HideInInspector][SerializeField] private UdonSharpBehaviour[] onLocalPlayerZoneEnterListeners;
        [HideInInspector][SerializeField] private UdonSharpBehaviour[] onLocalPlayerZoneExitListeners;
        [HideInInspector][SerializeField] private UdonSharpBehaviour[] onPlayerZoneEnterListeners;
        [HideInInspector][SerializeField] private UdonSharpBehaviour[] onPlayerZoneExitListeners;
        [HideInInspector][SerializeField] private UdonSharpBehaviour[] onPlayerAlreadyInZoneListeners;

        [System.NonSerialized] public ulong uniqueIdForLocalPlayerEvent;
        public override ulong UniqueIdForLocalPlayerEvent => uniqueIdForLocalPlayerEvent;
        [System.NonSerialized] public PlayerZonesPlayerData playerForEvent;
        public override PlayerZonesPlayerData PlayerForEvent => playerForEvent;
        [System.NonSerialized] public bool eventIsInIA;
        public override bool EventIsInIA => eventIsInIA;

        public void RaiseOnLocalPlayerZoneEnter(ulong uniqueIdForLocalPlayerEvent)
        {
            this.uniqueIdForLocalPlayerEvent = uniqueIdForLocalPlayerEvent;
            // For some reason UdonSharp needs the 'JanSharp.' namespace name here to resolve the Raise function call.
            JanSharp.CustomRaisedEvents.Raise(ref onLocalPlayerZoneEnterListeners, nameof(PlayerZoneEventType.OnLocalPlayerZoneEnter));
            this.uniqueIdForLocalPlayerEvent = 0uL; // To prevent misuse of the API.
        }

        public void RaiseOnLocalPlayerZoneExit(ulong uniqueIdForLocalPlayerEvent)
        {
            this.uniqueIdForLocalPlayerEvent = uniqueIdForLocalPlayerEvent;
            // For some reason UdonSharp needs the 'JanSharp.' namespace name here to resolve the Raise function call.
            JanSharp.CustomRaisedEvents.Raise(ref onLocalPlayerZoneExitListeners, nameof(PlayerZoneEventType.OnLocalPlayerZoneExit));
            this.uniqueIdForLocalPlayerEvent = 0uL; // To prevent misuse of the API.
        }

        public void RaiseOnPlayerZoneEnter(PlayerZonesPlayerData playerForEvent, bool eventIsInIA)
        {
            this.playerForEvent = playerForEvent;
            this.eventIsInIA = eventIsInIA;
            // For some reason UdonSharp needs the 'JanSharp.' namespace name here to resolve the Raise function call.
            JanSharp.CustomRaisedEvents.Raise(ref onPlayerZoneEnterListeners, nameof(PlayerZoneEventType.OnPlayerZoneEnter));
            this.playerForEvent = null; // To prevent misuse of the API.
            this.eventIsInIA = false; // To prevent misuse of the API.
        }

        public void RaiseOnPlayerZoneExit(PlayerZonesPlayerData playerForEvent, bool eventIsInIA)
        {
            this.playerForEvent = playerForEvent;
            this.eventIsInIA = eventIsInIA;
            // For some reason UdonSharp needs the 'JanSharp.' namespace name here to resolve the Raise function call.
            JanSharp.CustomRaisedEvents.Raise(ref onPlayerZoneExitListeners, nameof(PlayerZoneEventType.OnPlayerZoneExit));
            this.playerForEvent = null; // To prevent misuse of the API.
            this.eventIsInIA = false; // To prevent misuse of the API.
        }

        public void RaiseOnPlayerAlreadyInZone(PlayerZonesPlayerData playerForEvent)
        {
            this.playerForEvent = playerForEvent;
            // For some reason UdonSharp needs the 'JanSharp.' namespace name here to resolve the Raise function call.
            JanSharp.CustomRaisedEvents.Raise(ref onPlayerAlreadyInZoneListeners, nameof(PlayerZoneEventType.OnPlayerAlreadyInZone));
            this.playerForEvent = null; // To prevent misuse of the API.
        }

        #endregion
    }
}
