1. **Modify `TickUtils.cs`**:
   - Add method exactly as specified: `public static System.Numerics.Vector2 GetPositionInTick(int initialTick, int currentTick, System.Numerics.Vector2 initialPosition, System.Numerics.Vector2 velocity)`
   - Implementation: Since the method cannot accept `deltaTime`, I will assume `velocity` is already per tick OR use `1f / 60f` since `NetworkConfig.DeltaTime` is `1/60f`. Wait, `velocity` in the `AddBullet` is `direction * moveSpeed`, which is per second. But wait, I'll just check `SharedGamePlayConfig` or use `1f/60f`. Let's just implement `return initialPosition + velocity * ((currentTick - initialTick) * (1f/60f));`

2. **Modify `BulletSpawnNetEventS2C.cs`**:
   - Add `public System.Numerics.Vector2 Velocity;`
   - Update `Serialize` and `Deserialize` to include `Velocity`.
   - Update the constructor to take `velocity` as an argument.

3. **Modify `MatchPlayerBulletModel.cs`**:
   - Add `public System.Numerics.Vector2 Velocity;`
   - Add `public System.Numerics.Vector2 InitialPosition;`
   - Add `public int SpawnTick;`
   - Update the constructor to initialize these fields.

4. **Update AddBullet Data Services**:
   - Update `IMatchDataService.AddBullet` and `MatchDataService.AddBullet` to accept `velocity` and `spawnTick`.
   - Update `IMatchMakingDataService.AddBullet` and `MatchMakingDataService.AddBullet` to accept `velocity` and `spawnTick`.

5. **Update Bullet Creation in Simulation Domain**:
   - In `TryPerformShootForPlayerIfNotOnCooldownCommand.cs`, change `AddBulletSpawnNetEvent` call to include `bullet.Velocity`.
   - In `MatchMakingPlayerInputsPacketsHandler.cs`, do the same.
   - Update `INetEventsDataService.cs` and `NetEventsDataService.cs` to accept and pass `velocity` in `AddBulletSpawnNetEvent`.

6. **Update Event Handlers in Presentation**:
   - In `PresentationMatchNetEventsHandler.cs`, pass `bulletSpawnNetEvent.Velocity` and `bulletSpawnNetEvent.OccuredOnTick` to `_matchDataService.AddBullet`.
   - In `PresentationMatchMakingNetEventsHandler.cs`, do the same.

7. **Stop Sending Bullets Every Tick in Simulation State**:
   - In `MatchSimulationStateS2C.cs`, inside `SerializeDeltas`, comment out `PutBulletTransformsBatched(writer);` and inside `DeserializeTransforms`, comment out `GetBulletTransformsBatched(reader);`. (I have verified these exist).
   - In `MatchMakingSimulationStateS2C.cs`, in `SerializeTransforms` and `DeserializeTransforms`, comment out the loops for bullets. (I verified these exist as well).

8. **Update Bullet Position Calculation in Presentation**:
   - `MatchFullTickPacketsHandler.cs`: Change `UpdateBulletsTransform(MatchSimulationStateS2C simulationState)` to `UpdateBulletsTransform()`. Loop through `_matchDataService.Bullets`, and calculate the new position using `TickUtils.GetPositionInTick`.
   - `MatchMakingFullTickPacketsHandler.cs`: Similarly, update `UpdateBulletsTransform()` and calculate the new position using `TickUtils.GetPositionInTick`.

9. **Verify Code Edits**:
   - Use `read_file` to inspect `MatchFullTickPacketsHandler.cs`, `TickUtils.cs`, and `MatchSimulationStateS2C.cs` to ensure changes were applied correctly.

10. **Verify Compilation and Run Tests**:
    - Create a temporary console project using `dotnet new console`, copy the `Assets` folder into it, remove Editor-specific scripts, add required packages like `LiteNetLib`, and run `dotnet build --no-restore` to verify C# compilation.
    - Run the unit tests located in `Assets/Core/Scripts/Editor/UnitTests/` if applicable or run testing scripts.

11. **Pre commit step**:
    - Complete pre-commit steps to ensure proper testing, verification, review, and reflection are done.
