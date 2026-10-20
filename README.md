
# Player Zones

A reliable zone system for scripts to detect local and or remote players within specified areas.

Works with players in stations seamlessly.

# Installing

Then head to my [VCC Listing](https://jansharp.github.io/vrc/vcclisting.xhtml) and follow the instructions there.

# Usage

Areas are defined using colliders (triggers or not). The local player detects which zones they are in using physics overlap checks rather than trigger enter and exit events. This makes the system reliable.

Player Zones and their colliders can be toggled at runtime, and if a player is inside of said zone an enter or exit event will get raised respectively.

When moving from one zone to another without a moment of being outside of both of them in between, the enter event for the new zone is guaranteed to always be raised before the exit event for the previous zone. Even when both happen within the same frame.

For remote players, when a script requires knowing about them, which zones players are in is synced through [lockstep networking](https://github.com/JanSharp/VRCLockstep) by this system.

## Listening To Player Zone Events

Listening to events works simply by creating a script, such as the examples below, and then adding said script to a `PlayerZone` or its children.

```cs
using JanSharp;
using UdonSharp;
using UnityEngine;

namespace MySystem
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MyLocalZoneListener : UdonSharpBehaviour
    {
        [PlayerZoneEvent(PlayerZoneEventType.OnLocalPlayerZoneEnter)]
        public void OnLocalPlayerZoneEnter()
        {
            Debug.Log($"Local player entered {name}");
        }

        [PlayerZoneEvent(PlayerZoneEventType.OnLocalPlayerZoneExit)]
        public void OnLocalPlayerZoneExit()
        {
            Debug.Log($"Local player exited {name}");
        }
    }
}
```

```cs
using JanSharp;
using UdonSharp;
using UnityEngine;

namespace MySystem
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MyLocalAndRemoteZoneListener : UdonSharpBehaviour
    {
        [HideInInspector][SerializeField][FindInParent] private PlayerZoneAPI zone;

        // These local events get raised immediately when the local player enters/exits.

        [PlayerZoneEvent(PlayerZoneEventType.OnLocalPlayerZoneEnter)]
        public void OnLocalPlayerZoneEnter()
        {
            Debug.Log($"Local player entered {name}");
        }

        [PlayerZoneEvent(PlayerZoneEventType.OnLocalPlayerZoneExit)]
        public void OnLocalPlayerZoneExit()
        {
            Debug.Log($"Local player exited {name}");
        }

        // The enter and exit events below get raised for the local player too,
        // however delayed by input action latency, as well as delayed until
        // lockstep is initialized in the case of having just loaded into the world.

        // For late joiners, to get events for players which are
        // already in a zone at the time of loading into the world.
        [PlayerZoneEvent(PlayerZoneEventType.OnPlayerAlreadyInZone)]
        public void OnPlayerAlreadyInZone() => OnPlayerZoneEnter();

        [PlayerZoneEvent(PlayerZoneEventType.OnPlayerZoneEnter)]
        public void OnPlayerZoneEnter()
        {
            CorePlayerData player = zone.PlayerForEvent.core;
            if (player.isLocal)
                return;
            Debug.Log($"{player.displayName} entered {name}");
        }

        [PlayerZoneEvent(PlayerZoneEventType.OnPlayerZoneExit)]
        public void OnPlayerZoneExit()
        {
            CorePlayerData player = zone.PlayerForEvent.core;
            if (player.isLocal)
                return;
            Debug.Log($"{player.displayName} exited {name}");
        }
    }
}
```

## Migrating Existing Worlds

The `PlayerZoneTriggerEventsLocal` and `PlayerZoneTriggerEventsLocalAndRemote` scripts are bridges from `PlayerZone`s to other scripts which were written using VRChat's player trigger enter and exit events.

The Player Zones system, by default, configures a layer which collides with nothing. Which means they also do not collide with local nor remote players. Colliders on this layer will not raise any VRChat nor Unity trigger events.

The `PlayerZone` script will force the object it is on to the layer as defined by the `PlayerZonesManager`.

With all that in mind, in a lot of cases migrating an existing script setup can often be as simple as:

- Add the `PlayerZone` script to the same object
- Add the `PlayerZoneTriggerEventsLocal` script also to the same object
  - (or `PlayerZoneTriggerEventsLocalAndRemote` if the script requires remote player events)

And that's it. Only times this does not work are:

- The layer used by the Player Zone system gets changed to ultimately collide with local and or remote players
  - See `PlayerZonesManager` (which gets created automatically on build or entering play mode)
  - See ProjectSettings => Physics
- The third party script which is listening to VRChat's trigger events uses non-none sync mode
  - In this case the third party script should be moved to a new empty object
  - Make the `PlayerZoneTriggerEvents-Local/LocalAndRemote` target said new object
  - Make sure not to break references to a script when putting it on a different object
    - Use the References Window from [Editor Tools](https://github.com/JanSharp/VRCEditorTools) to help find such incoming references
- The third party script was written depending on specific behavior and quirks of trigger events, which may be incompatible with actually getting reliable events
  - Such as the fact that teleporting from one zone to another, the Player Zones system guarantees to get the enter event for the new zone before the exit event of the previous zone, while with VRChat's trigger events I believe it to be the other way around most of the time, but not always

# Contributing

This project uses [custom git filters](.gitattributes) to reduce the amount of noise generated by Udon and UdonSharp. These filters are not required in order to contribute, though without them all UdonSharp asset files will show as modified and those "changes" should not be committed.
