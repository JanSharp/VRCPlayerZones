
- [x] Add layer handling
  - [x] Some logic to pick a layer
  - [x] Some setup for the layer to not collide with anything
  - [x] Force player zones and expansions onto that layer
- [x] Smartly reduce the amount of physics checks
  - [x] do about 4 per second
  - [x] when moving quickly or teleporting, do more
  - [ ] maybe do one when zones get enabled or disabled
- [x] Spread OnClientBeginCatchUp out across frames
