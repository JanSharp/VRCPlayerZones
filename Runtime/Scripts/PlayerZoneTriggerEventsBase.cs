using UdonSharp;
using UnityEngine;
using VRC.Udon;

namespace JanSharp
{
    public abstract class PlayerZoneTriggerEventsBase : UdonSharpBehaviour
    {
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public const string AutoPopulateTargetsPropName = nameof(autoPopulateTargets);
        public bool AutoPopulateTargets => autoPopulateTargets;
#endif

        [Tooltip("When enabled, the Targets list will get populated with "
            + "all other scripts which are also specifically on this object OnBuild.")]
        [SerializeField] private bool autoPopulateTargets = true;
        public UdonBehaviour[] targets;
    }
}
