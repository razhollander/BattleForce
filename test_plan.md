1. **Define TalentType:** Add `MagneticPull = 7` to `Core.Game.Domains.GamePlay.Shared.S2CModels.TalentType`.
2. **Define Shared Network Event:**
   - Create `CreateMagenticPullFieldNetEventS2C` implementing `INetSerializable` and `IComparable`.
   - Fields: `OccuredOnTick` (int), `Position` (Vector2), `Rotation` (Vector2), `HitPlayerId` (ushort?, use a boolean flag in serialization to handle nullability or standard ushort approach like `null = ushort.MaxValue`? Actually we should use a nullable pattern or reserve 0 / MaxValue), `CasterPlayerId` (ushort).
3. **Update Network Models:**
   - Add `CreateMagenticPullFieldNetEvents` (FixedUnorderedList) to `MatchFullTickPacketS2C.cs` and handle serialization/deserialization.
   - Add `CreateMagenticPullFieldNetEvents` capacity in `NetworkConfig.MaxCap`.
4. **Update Presentation Network Handling:**
   - Add to `ICachedPresentationEventsService` and `CachedPresentationEventsService`.
   - Update `MatchFullTickPacketsHandler` to read the events, filter by `OccuredOnTick`, sort, and cache.
   - Update `PresentationMatchNetEventsHandler` (or command if that's the pattern used) to handle the visual effects. We can process it directly or through a command like `HandleMagneticPullFieldNetEventsCommand`. Wait, the prompt says "When the client receives CreateMagenticPullFieldNetEventS2C: Creates a squared field...". Since there's no data model, it goes straight to a view controller. Let's create `HandleMagneticPullFieldNetEventsCommand`.
5. **Create Presentation Visuals (MVC & Pool):**
   - Create `MagneticPullFieldView` (with serialized field for duration, size, etc.) and `MagneticPullHitEffectView`.
   - Create `MagneticPullFieldPool` and `MagneticPullHitEffectPool`.
   - Create `MagneticPullFieldController` to manage spawning, sizing, and hit effects. Integrate this controller to Zenject (wait, how are they instantiated? There must be an Installer).
6. **Pre-commit Steps:**
   - Complete pre-commit steps to ensure proper testing, verification, review, and reflection are done.
7. **Submit Changes.**
