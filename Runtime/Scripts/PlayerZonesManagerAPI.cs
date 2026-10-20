using UdonSharp;

namespace JanSharp
{
    [SingletonScript("3d6271bcb745bf3b09029c7b709df249")] // Runtime/Prefabs/PlayerZonesManager.prefab
    public abstract class PlayerZonesManagerAPI : UdonSharpBehaviour
    {
        /// <summary>
        /// <para>The maximum amount of overlapping colliders the player zones system will check for.</para>
        /// </summary>
        public const int MaxSimultaneousZones = 64;
        /// <summary>
        /// <para>Readonly.</para>
        /// <para>The list of synced zones which this effectively contains is game state safe. Otherwise not
        /// game state safe, because non synced zones are allowed to get destroyed at runtime.</para>
        /// </summary>
        public abstract PlayerZoneAPI[] AllZones { get; }
        /// <summary>
        /// <para>All zones the local player is currently inside of.</para>
        /// <para>Only read up to <see cref="ZonesLocalPlayerIsInCount"/>, the reset of the elements are
        /// undefined.</para>
        /// <para>The used ranges of elements may contain <see langword="null"/> specifically when player
        /// zones get destroyed at runtime. For best compatibility expect <see langword="null"/>.</para>
        /// <para>The returned array itself is never <see langword="null"/>.</para>
        /// <para>The system swaps/flips between 2 of these arrays internally, therefore one must refetch this
        /// array before using it every time to ensure one has the correct currently used array.</para>
        /// <para>The length of this array is <see cref="MaxSimultaneousZones"/>.</para>
        /// <para>Effectively the latency state of <see cref="PlayerZonesPlayerData.zonesThisPlayerIsIn"/> for
        /// the local player.</para>
        /// <para>Readonly.</para>
        /// <para>Not game state safe.</para>
        /// </summary>
        public abstract PlayerZoneAPI[] ZonesLocalPlayerIsIn { get; }
        /// <summary>
        /// <para>The amount of zones in <see cref="ZonesLocalPlayerIsIn"/> the local player is currently
        /// inside of.</para>
        /// <para>Not game state safe.</para>
        /// </summary>
        public abstract int ZonesLocalPlayerIsInCount { get; }
    }
}
