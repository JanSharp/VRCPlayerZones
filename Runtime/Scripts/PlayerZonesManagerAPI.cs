using UdonSharp;

namespace JanSharp
{
    public enum PlayerZonesManagerEventType
    {
    }

    [System.AttributeUsage(System.AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class PlayerZonesManagerEventAttribute : CustomRaisedEventBaseAttribute
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
        public PlayerZonesManagerEventAttribute(PlayerZonesManagerEventType eventType)
            : base((int)eventType)
        { }
    }

    [SingletonScript("3d6271bcb745bf3b09029c7b709df249")] // Runtime/Prefabs/PlayerZonesManager.prefab
    public abstract class PlayerZonesManagerAPI : UdonSharpBehaviour
    {
    }
}
