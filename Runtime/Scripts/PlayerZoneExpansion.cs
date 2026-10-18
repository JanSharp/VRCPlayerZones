using UdonSharp;
using UnityEngine;

namespace JanSharp
{
    [DisallowMultipleComponent]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerZoneExpansion : UdonSharpBehaviour
    {
        [HideInInspector][FindInParent] public PlayerZoneAPI zone;
    }
}
