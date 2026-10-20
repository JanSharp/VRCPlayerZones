
- [x] Add layer handling
  - [x] Some logic to pick a layer
  - [x] Some setup for the layer to not collide with anything
  - [x] Force player zones and expansions onto that layer
- [x] Smartly reduce the amount of physics checks
  - [x] do about 4 per second
  - [x] when moving quickly or teleporting, do more
  - [ ] maybe do one when zones get enabled or disabled
- [x] Spread OnClientBeginCatchUp out across frames
- [x] explain expansions in their inspector
- [x] list expansions in the zone inspector too
- [x] add usage documentation to the readme
- [x] explain PlayerZoneTriggerEventsLocal and LocalAndRemote in their inspector
- [x] add an on build error for orphaned player zone listeners
- [x] test whether VRChat raises on player trigger enter and exit events when a collider is on a layer which does not collide with the local player nor player layers
  - events do not get raised when the layers do not collide - good!
- [ ] expose list of zones the local player is in (latency state) on PlayerZonesManagerAPI
- [ ] add general "player entered/exited sone/any zone" events

Performance Notes:

When running checks every single frame, average times (on a 5950X):
- while in 0 zones: 0.020 ms
- while in 1 zones: 0.050 ms
- while in 2 zones: 0.065 ms
- while in 3 zones: 0.080 ms

When running 5 times per second, average time per frame is like 0.005 ms to 0.010 ms, though per frame time would be higher with lower frame rates, of course
