using UdonSharp;

namespace JanSharp
{
    public enum PlayerZoneEventType
    {
        /// <summary>
        /// <para>Use <see cref="PlayerZoneAPI.UniqueIdForLocalPlayerEvent"/> for latency hiding, however also
        /// note that it can be <c>0uL</c> - no input action having been sent - which happens when a player
        /// enters/exits a zone while <see cref="LockstepAPI.IsInitialized"/> is still
        /// <see langword="false"/>.</para>
        /// <para>Not game state safe.</para>
        /// </summary>
        OnLocalPlayerZoneEnter,
        /// <inheritdoc cref="OnLocalPlayerZoneEnter"/>
        OnLocalPlayerZoneExit,
        /// <summary>
        /// <para>Use <see cref="PlayerZoneAPI.PlayerForEvent"/>.</para>
        /// <para>Check <see cref="PlayerZoneAPI.EventIsInIA"/> and when <see langword="true"/> use
        /// <see cref="LockstepAPI.SendingUniqueId"/> to resolve latency hiding, if any latency hiding was
        /// done.</para>
        /// <para>Game state safe.</para>
        /// </summary>
        OnPlayerZoneEnter,
        /// <inheritdoc cref="OnPlayerZoneEnter"/>
        OnPlayerZoneExit,
        /// <summary>
        /// <para>Use <see cref="PlayerZoneAPI.PlayerForEvent"/>.</para>
        /// <para>Raised inside of <see cref="LockstepEventType.OnClientBeginCatchUp"/>, for every player
        /// which is already in the zone in the game state, so foreach player in
        /// <see cref="PlayerZoneAPI.PlayerZoneAPI"/>.</para>
        /// <para>Exists for ease of use for any systems which simply care about getting pairs of enter and
        /// exit events for local and remote players, without specific care about said events being game state
        /// safe. Which is a guarantee this provides, for every player there will either be an
        /// <see cref="OnPlayerAlreadyInZone"/> or <see cref="OnPlayerZoneEnter"/> with a matching
        /// <see cref="OnPlayerZoneExit"/>, in perfect pairs.</para>
        /// <para>Not game state safe.</para>
        /// </summary>
        OnPlayerAlreadyInZone,
    }

    [System.AttributeUsage(System.AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class PlayerZoneEventAttribute : CustomRaisedEventBaseAttribute
    {
        /// <summary>
        /// <para>The method this attribute gets applied to must be public.</para>
        /// <para>The name of the function this attribute is applied to must have the exact same name as the
        /// name of the <paramref name="eventType"/>.</para>
        /// <para>Event registration is performed at OnBuild, which is to say that scripts with these kinds of
        /// event handlers must exist in the scene at build time, any runtime instantiated objects with these
        /// scripts on them will not receive these events.</para>
        /// <para>Disabled scripts still receive events.</para>
        /// </summary>
        /// <param name="eventType">The event to register this function as a listener to.</param>
        public PlayerZoneEventAttribute(PlayerZoneEventType eventType)
            : base((int)eventType)
        { }
    }

    public abstract class PlayerZoneAPI : UdonSharpBehaviour
    {
        public abstract PlayerZoneExpansion[] Expansions { get; }

        /// <summary>
        /// <para>Not game state safe.</para>
        /// </summary>
        public abstract bool LocalPlayerIsInZone { get; }
        /// <summary>
        /// <para>Not game state safe.</para>
        /// </summary>
        public abstract bool LocalPlayerIsInZoneInGS { get; }

        #region Game State

        /// <summary>
        /// <para>Game state safe.</para>
        /// </summary>
        public abstract PlayerZonesPlayerData[] PlayersInZone { get; }
        /// <summary>
        /// <para>Game state safe.</para>
        /// </summary>
        public abstract int PlayersInZoneCount { get; }

        #endregion

        public abstract ulong UniqueIdForLocalPlayerEvent { get; }
        public abstract PlayerZonesPlayerData PlayerForEvent { get; }
        public abstract bool EventIsInIA { get; }
    }
}
