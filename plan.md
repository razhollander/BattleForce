1. **Define TalentType**: Add `MagneticPull = 7` to `TalentType` enum in `Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/TalentType.cs`.

2. **Add Network Event**:
   - Create `CreateMagenticPullFieldNetEventS2C` in `Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/CreateMagenticPullFieldNetEventS2C.cs`.
   - Fields:
     - `int OccuredOnTick`
     - `Vector2 Position`
     - `Vector2 Rotation`
     - `bool HasHit`
     - `ushort HitPlayerId`
     - `ushort CasterPlayerId`
   - Implement `INetSerializable` and `IComparable`. Note: `HasHit` is required because `HitPlayerId` is optional.
   - Verify the event structure and namespaces match other events using `read_file`.

3. **Update Full Tick Packet**:
   - Add `CreateMagenticPullFieldNetEvents` configuration to `NetworkConfig` and `MaxCap`. Add `public int CreateMagenticPullFieldNetEvents = 128;` to `Assets/Core/Scripts/Network/NetworkConfig.cs`.
   - Add a `FixedUnorderedList<CreateMagenticPullFieldNetEventS2C> CreateMagenticPullFieldNetEvents` field to `Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/MatchFullTickPacketS2C.cs` and implement its serialization / deserialization methods properly. Make sure to `Clear()` the collection before deserialization.
   - Verify serialization methods in `MatchFullTickPacketS2C.cs` are properly configured using `grep`.

4. **Update Shared Config**:
   - Add a property `public float MagneticPullFieldSize = 5f;` to `SharedGamePlayConfig` in `Assets/Core/Game/Domains/GamePlay/Shared/Scripts/Configs/SharedGamePlayConfig.cs`.

5. **Client Caching of Network Event**:
   - Add `List<CreateMagenticPullFieldNetEventS2C> CreateMagenticPullFieldNetEvents` to `Assets/Core/Game/Domains/GamePlay/Presentation/Scripts/PresentationEvents/ICachedPresentationEventsService.cs` and its implementation `Assets/Core/Game/Domains/GamePlay/Presentation/Scripts/PresentationEvents/CachedPresentationEventsService.cs`.
   - Add `CapacityList<CreateMagenticPullFieldNetEventS2C> _cachedUnprocessedCreateMagenticPullFieldEvents` to `Assets/Core/Game/Domains/GamePlay/Presentation/Match/Scripts/Network/PacketsHandlers/MatchFullTickPacketsHandler.cs`, process these events from `MatchFullTickPacketS2C`, sort them, and call `ProcessCreateMagenticPullFieldEvents`.
   - Add `ProcessCreateMagenticPullFieldEvents(CapacityList<CreateMagenticPullFieldNetEventS2C> events)` to `Assets/Core/Game/Domains/GamePlay/Presentation/Match/Scripts/Network/PacketsHandlers/PresentationMatchNetEventsHandler.cs` which adds them to the cache.

6. **Create Views, Pools, and Controllers**:
   - In `Assets/Core/Game/Domains/GamePlay/Presentation/Match/Features/MagneticPullEffect/Scripts/`:
     - Create `MagneticPullFieldView.cs` extending `MonoBehaviour`, implementing `IPoolable`. It will take a size from config and scale itself. It needs a serialized field for duration, `[SerializeField] private float _showDuration = 2f;`.
     - Create `MagneticPullHitEffectView.cs` extending `MonoBehaviour`, implementing `IPoolable`. Serialize field for duration `[SerializeField] private float _showDuration = 2f;`.
     - Create `MagneticPullFieldPool.cs` and `MagneticPullHitEffectPool.cs` extending `PrefabsPool<View>`.
     - Create `IMagneticPullEffectController.cs` and `MagneticPullEffectController.cs`. Controller implements `InitEntryPoint()` to initialize pools, and methods like `PlayFieldEffect(Vector2 position, Vector2 rotation, float size)` and `PlayHitEffect(Vector2 casterPos, Vector2 enemyPos)`.
   - Verify scripts compile without issues using a test script or `dotnet build` if available. Wait, since it's unity, I'll write the scripts carefully and verify the contents.

7. **Implement Command to Execute Network Events**:
   - Create `HandleCreateMagenticPullFieldNetEventsCommand.cs` in `Assets/Core/Game/Domains/GamePlay/Presentation/Match/Scripts/Commands/NetEvents/`.
   - Implement `ICommandVoid` and `BaseCommand`. Iterate through `_cachedPresentationEventsService.CreateMagenticPullFieldNetEvents`.
   - For each event, invoke `_magneticPullEffectController.PlayFieldEffect(event.Position, event.Rotation, sharedConfig.MagneticPullFieldSize)`.
   - If `event.HasHit`, get positions of Caster and Hit player from `_matchDataService.GetPlayer(id).Transform.Position`. Then call `_magneticPullEffectController.PlayHitEffect(casterPos, hitPos)`.
   - Finally `Clear()` the cached events list.
   - Register the command in `Assets/Core/Game/Domains/GamePlay/Presentation/Match/Scripts/TickProcessor/ClientMatchPresentationTickProcessor.cs` and execute it during `ManagedUpdate()`.
   - Verify the command is registered in `ClientMatchPresentationTickProcessor` correctly.

8. **Zenject Installer Integration**:
   - Update `Assets/Core/Game/Domains/GamePlay/Presentation/Match/Scripts/ZenjectInstallers/GamePlayMatchInstaller.cs` to add `Container.BindInterfacesTo<MagneticPullEffectController>().AsSingle().WithArguments(_magneticPullFieldViewPrefab, _magneticPullHitEffectViewPrefab).NonLazy();`. Add `[SerializeField] private MagneticPullFieldView _magneticPullFieldViewPrefab;` and `[SerializeField] private MagneticPullHitEffectView _magneticPullHitEffectViewPrefab;` to the installer. We must use `EditableRef` or similar if required by project, but unity normal `SerializeField` works.
   - Also add `.WithArguments(_magneticPullFieldViewPrefab, _magneticPullHitEffectViewPrefab)` assuming I injected them to controller.

9. **Tests and Compilation**:
   - Run any available scripts like `./test_compilation.sh` to ensure no major syntax issues.

10. **Pre commit step**: Complete pre-commit steps to ensure proper testing, verification, review, and reflection are done.
