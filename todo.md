
- [ ] Add layer handling
  - [ ] Some logic to pick a layer
  - [ ] Some setup for the layer to not collide with anything
  - [ ] Force player zones and expansions onto that layer
- [ ] Smartly reduce the amount of physics checks
  - [ ] do about 4 per second
  - [ ] when moving quickly or teleporting, do more
  - [ ] maybe do one when zones get enabled or disabled
- [x] Spread OnClientBeginCatchUp out across frames
