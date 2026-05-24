1. **Simulation Configuration (`PlayerSpaceshipConfig.cs`)**:
   - Update `PlayerSpaceshipConfig` in `Assets/Core/Game/Domains/GamePlay/Simulation/Scripts/Configurations/PlayerSpaceshipConfig.cs` to add `public float LockOnHeartMaxDistance = 15f;` and `public float LockOnHeartMaxAngleDegrees = 30f;`.

2. **Simulation Data Models**:
   - Create a new network event `PlayerLockOnHeartTargetsChangedNetEventS2C` in `Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PlayerLockOnHeartTargetsChangedNetEventS2C.cs`. It will contain `LockedOnHeartIds` (`FixedUnorderedList<ushort>`) and `PlayerId` (`ushort`). Add constructor accepting `int maxHeartsIdsOnTarget` to initialize the list.
   - Update `MaxCap` in `Assets/Core/Scripts/Network/NetworkConfig.cs` to add `public int MaxHeartsIdsOnTarget = 8;` and `public int PlayerLockOnHeartTargetsChangedNetEvents = 8;`.
   - Add `public FixedUnorderedList<ushort> PlayerHeartsIdsOnTarget;` to `PlayerSpaceshipStateS2C`. Modify its constructor to accept an `int maxHeartsIdsOnTarget` parameter, and initialize the list with it. Update custom serialization / deserialization methods. Copy it properly in `GetClone()`.
   - Modify `PlayerStateS2C` constructor to accept `int maxHeartsIdsOnTarget` and pass it to the `PlayerSpaceshipStateS2C` constructor. Update `PlayerStateS2C` instantiation points (`SimulationStateS2C`, `MatchSimulationStateS2C`, `PlayerRejoinAcceptPacketS2C`, `MatchPlayerJoinPacketsHandler`) to pass the appropriate `MaxCap.MaxHeartsIdsOnTarget` or use `networkConfig.MaxCap.MaxHeartsIdsOnTarget` depending on available context.
   - Update `MatchFullTickPacketS2C` in `Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/MatchFullTickPacketS2C.cs` to include `public FixedClassUnorderedList<PlayerLockOnHeartTargetsChangedNetEventS2C> PlayerLockOnHeartTargetsChangedNetEvents;`. Initialize it using `MaxCap.PlayerLockOnHeartTargetsChangedNetEvents` and `MaxCap.MaxHeartsIdsOnTarget`. Handle its serialization and deserialization. Make sure to `Clear()` before deserialization.

3. **Net Events Service**:
   - Add a method to `INetEventsDataService` and `NetEventsDataService` to trigger this event: `void AddPlayerLockOnHeartTargetsChangedNetEvent(int onTick, ushort playerId, FixedUnorderedList<ushort> targetHeartIds)`.

4. **PhysicsSimulator**:
   - Add `bool RayCast(Vector2 point1, Vector2 point2, out PhysicsBodyData hitBodyData)` method to `IPhysicsSimulator.cs` and `PhysicsSimulator.cs`.
   - Use Box2D's internal callback. I'll create a local function to handle this and return the `PhysicsBodyData` of the closest hit.

5. **Simulation Logic - LockOnHeartTargetService**:
   - Create `LockOnHeartTargetService` (and interface `ILockOnHeartTargetService`) inside `Assets/Core/Game/Domains/GamePlay/Simulation/Match/Scripts/Services/LockOnHeartTargetService/LockOnHeartTargetService.cs`. The service will contain `public void Process(int processedTick, PlayerStateS2C playerState);`.
   - It will depend on `IMatchDataService`, `IPhysicsSimulator`, `SimulationGamePlayConfig`, and `INetEventsDataService`. (Check `IMatchDataService.cs` using `read_file` to verify access to simulation state).
   - Add a Zenject binding for `ILockOnHeartTargetService` to `LockOnHeartTargetService` in `Assets/Core/Game/Domains/GamePlay/Simulation/Match/Scripts/Initiator/ServerMatchInstaller.cs`.
   - In `MatchPlayerInputsPacketsHandler.cs`: Inject `ILockOnHeartTargetService` via constructor and assign to `_lockOnHeartTargetService`. Inside `UpdatePlayerShoot`, call `_lockOnHeartTargetService.Process(processedTick, playerModel)`.
   - The method logic for `LockOnHeartTargetService.Process`:
     - Loop over `_matchDataService.SimulationState.Players` using a `for` loop.
     - Ignore self.
     - Calculate RayCast start (head): `playerState.Spaceship.Transform.Position + playerState.Spaceship.Transform.Direction * _gamePlayConfig.PlayerSpaceship.DefaultPlayerRadius`.
     - Calculate target (heart): `enemyState.Spaceship.Transform.GetHeartPosition()`.
     - Call `RayCast`. If hit, check if `hitBodyData.PhysicsBodyType == PhysicsBodyType.PlayerHeart` and `hitBodyData.Id == enemyState.Id`.
     - If true, calculate distance squared and angle. If within thresholds, add to list.
     - Compare lists and trigger `AddPlayerLockOnHeartTargetsChangedNetEvent` if changed.

6. **Testing**:
   - Test by running the following bash commands to create a temporary console project, build the project with `--no-restore`, and clean up:
     ```bash
     dotnet new console -n TempCompileTest
     cp -R Assets TempCompileTest/
     cd TempCompileTest
     dotnet build --no-restore
     cd ..
     rm -rf TempCompileTest
     ```
   - Make any necessary fixes if there are compilation errors.

7. **Pre-commit**:
   - Complete pre-commit steps to ensure proper testing, verification, review, and reflection are done.

8. **Submit**:
   - Submit the implementation.
