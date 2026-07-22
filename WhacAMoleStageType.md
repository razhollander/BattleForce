# Whac-A-Mole StageType — Implementation Spec

> **Audience:** multiple agents implementing this feature in parallel/sequence.
> **Working directory for all code paths below:** `Assets/`
> Read `Assets/CLAUDE.md` first for the architecture overview. This doc assumes that context.

---

## 1. Feature summary

Introduce a second game mode alongside the existing one. Concretely:

1. Rename the current health-based elimination mode from the generic "the normal stage" to **DeathMatch**. `StageType.DeathMatch` already exists (`Shared/Scripts/Enums/StageType.cs`) — this is mostly making the *behavior* explicitly branch on stage type, and naming things consistently.
2. Add a new **`StageType.WhacAMole`** mode with these rules:
   - **Moles** spawn at random positions during the stage.
   - Players destroy a Mole by **shooting it (bullet)** or **spinning into it** (the player's spinning body hitting the Mole). When hit, the Mole **disappears** and the hitting player's **team score ("moles hit") increments**.
   - **No player health / no deaths.** Players cannot damage each other by shooting — bullets pass through / do not damage enemy players. **Talents still affect enemies** (spin, KO, fishing rod, etc. keep working).
   - **PowerUp balls still spawn** (same system as today).
   - A **countdown timer is shown in the middle of the screen.** When it reaches zero the stage ends and the **team with the most Moles hit wins** (define tie-breaking, see §7).
3. **Stage rotation:** every **X (default 3)** stages the players enter is a Whac-A-Mole stage; the rest are DeathMatch.
4. Whac-A-Mole is **configurable** and has its **own pool of environment layouts** (chosen randomly from that pool, independent of the DeathMatch pool).

Everything runs inside the existing server-authoritative, 60 tick/s simulation. The server owns all logic; clients render from tick packets + net events.

---

## 2. Where the current mode lives (grounding)

Key existing files an implementer must understand:

| Concern | File |
|---|---|
| Stage type enum | `Core/Game/Domains/GamePlay/Shared/Scripts/Enums/StageType.cs` |
| Per-stage scoring/state | `Simulation/Match/Scripts/Stage/StageDataService.cs` (+ `IStageDataService.cs`) |
| Stage build + rotation counter (`_stageNumber`) | `Simulation/Match/Scripts/Commands/InitStageCommand.cs` |
| Environment layout pool (`AvailableLayoutIndexes`) | `Shared/Scripts/Configs/EnvironmentConfig.cs` |
| Server tick loop | `Simulation/Match/Scripts/NetworkManager/TickHandlers/ServerMatchNetworkTickProcessor.cs` |
| Win condition (elimination) | `Simulation/Match/Scripts/Commands/PlayerHitCommand.cs` → `TryInvokeMatchEnded()` |
| Stage-end sequence | `Simulation/Match/Scripts/Commands/StageEndedCommand.cs` |
| Collision resolution | `Simulation/Match/Scripts/Commands/ProcessCachedCollisionsCommand.cs` |
| PowerUp spawning (template for Mole spawner) | `Simulation/Match/Scripts/Commands/TrySpawnPowerUpBallsCommand.cs` |
| Timers stepped each tick | `Simulation/Match/Scripts/Commands/StepTimersCommand.cs` |
| Simulation state (single source of truth, serialized for rejoin) | `Shared/Scripts/S2CModels/MatchSimulationStateS2C.cs` |
| Physics body enum + collision filters | `Simulation/Scripts/Physics/PhysicsBodyType.cs`, `PhysicsCollisionFilters.cs` |
| Physics interface | `Simulation/Scripts/Physics/IPhysicsSimulator.cs`, `PhysicsSimulator.cs` |
| Tick packet + event mask | `Shared/Scripts/S2CModels/MatchFullTickPacketS2C.cs` |
| Server per-client event queues | `Simulation/Scripts/NetworkManager/NetEventsDataService.cs` (+ interface) |
| Client packet unpack | `Presentation/Match/Scripts/Network/PacketsHandlers/MatchFullTickPacketsHandler.cs` |
| Client tick processor | `Presentation/Match/Scripts/TickProcessor/ClientMatchPresentationTickProcessor.cs` |
| Presentation MVC binding | `Presentation/Match/Scripts/ZenjectInstallers/GamePlayMatchInstaller.cs` |
| Closest presentation MVC analog for Moles | `Presentation/Match/Features/PowerUpBall/Scripts/` (spawn + obtained/despawn) |

**Important existing facts to respect:**

- **Zero runtime allocation.** All collections are fixed-capacity (`FixedUnorderedList`, `FixedClassUnorderedList`, `CapacityDict`) sized from `NetworkConfig.MaxCap` and allocated in entry points / constructors, never during gameplay. Follow this for Moles.
- Entity IDs are `ushort`; Box2D entity IDs start at `1` (`SharedGamePlayConfig.MinEntityId`).
- **The primary 64-bit net-event mask (bits 0–63) is FULL.** `MatchFullTickPacketS2C` already has a second `eventMask2` for overflow events (bit 64+). Soul talent uses `eventMask2` bits 0–1. **New Mole net events must use the next free `eventMask2` bits** — read `CalculateEventMask2()` in `MatchFullTickPacketS2C.cs` to find them. Use the `/add-net-event` skill.
- `MatchSimulationStateS2C.StageType` field **already exists** and is already serialized (`writer.Put((byte)StageType)`), so the client already receives the current stage type. It is currently never *set* to anything but default — Work Package B sets it.
- Spinning must go through `SpinPlayerCommand`/`TrySpinPlayerCommand`; forces through `AddForceToPlayerCommand`. Never write `AngularVelocity`/`Velocity` directly (see `Assets/CLAUDE.md` "Common simulation patterns").

---

## 2.5 Coding conventions every package MUST follow

**Before writing code, invoke the `/coding-guidlines` skill and follow it.** This is the standard every PR in this repo is reviewed against. The rules most likely to bite *this* feature:

- **Zero garbage allocation in the Simulation domain.** No `new`, LINQ, boxing, closures, or `params` on any per-tick path. Pre-allocate every list/dict from `NetworkConfig.MaxCap` in `ResolveDependencies()` or an entry point. This is the single most important rule here.
- **Commands:** one job, `SetX(...)` fluent setters that store fields, then `Execute()`. Create them via `_commandFactory.CreateCommandVoid<T>()` in `ResolveDependencies()`/`InitEntryPoint()` — never inside `OnTick`/per-frame.
- **No methods called in constructors** — constructors only assign fields. Resolve dependencies and build caches in `ResolveDependencies()`.
- **`Get*` methods must not mutate.** `EnvironmentConfig.GetLayoutIndexesForStageType(...)` must be a pure lookup. The "don't repeat a layout until the pool is exhausted" mutation must live in a non-`Get` method — mirror the existing `GenerateRandomStageId()` naming, not a `Get`. Same for score reads: a getter that returns the winning team must not clear or advance anything.
- **`Try` prefix** for any method that can early-exit / fail (`TrySpawnMolesCommand`, `TryGetMoleIndexById`, `TryEndWhacAMoleStageCommand`). Follow the existing `TryGet.../out` idiom already used throughout `MatchSimulationStateS2C`.
- **Naming:** enums suffixed `Type`; booleans prefixed `is/can/has` (`IsWhacAMoleModeEnabled`); **time fields carry their unit** (`MatchDurationSeconds`, `MoleLifetimeSeconds`, `MoleSpawnIntervalSeconds`); no abbreviations (`Position` not `Pos`). Constants in `ALL_CAPS`, no magic numbers — every tuning value goes in `WhacAMoleConfig` (§4.2), matching how `MAX_ATTEMPTS_TO_FIND_FREE_SPAWN_POSITION` is a named const in `TrySpawnPowerUpBallsCommand`.
- **Method ≤ 30 lines, class ≤ 200 lines.** If `TrySpawnMolesCommand` or a collision handler grows past that, split into private one-job methods (the existing spawner/collision commands already do this — copy that granularity).
- **Validate at the gate, trust inside.** Public entry points (command setters, spawner) check inputs; internal helpers assume valid data.
- **For every `Register` there must be an `Unregister`** (and every event subscription an unsubscribe). Applies to any `ITickService.RegisterObserver`, and in Package F to every MVC event/listener and controller lifecycle hook.
- **Statics hide dependencies — avoid adding new ones.** Note that `InitStageCommand._stageNumber` is an **existing** `private static int`; reusing it for rotation (§4.4) is consistent with the current "add talent every X stages" code, but be aware it is process-global and is not reset per match — if the rotation must restart at stage 1 for a new match, that reset is a real concern to raise, not to silently depend on.

### MVC / domain boundaries (Package F especially)
- **View** = dumb `MonoBehaviour`: shows sprites/text, never talks to a controller or listens to logic events.
- **Controller** = pure C# logic; it may touch other features' **DataServices** but not their **Controllers**.
- **Cross-feature communication** goes through a `Command` or a `Broadcaster` event — never direct controller-to-controller calls.
- Presentation stays render-only: **no game logic on the client.** All authoritative decisions (spawn, hit, score, win) are server-side; the client reacts to state + net events.
- In the Presentation domain, obtain cancellation tokens from `_stageCancellationTokenProvider` (never `destroyCancellationToken`) if any async is introduced.

---

## 3. Work packages & dependency order

Split the work into these packages. **A must land first** (everything depends on the config/enum/pool foundation). B, C, D can then proceed largely in parallel; E depends on C; F (presentation) depends on C+D's net events existing.

```
A. Foundation: stage-type selection, config, environment pools     (blocking)
        │
        ├── B. Mode branching: disable PvP damage, skip elimination win
        │
        ├── C. Mole entity: state model, physics, spawner, collision, net events
        │        └── E. Whac-A-Mole scoring + timer-based win condition
        │
        └── F. Presentation: Mole MVC, on-screen timer UI, scoreboard
```

Each package below lists concrete files to add/change and the acceptance check.

---

## 4. Work Package A — Foundation (blocking)

**Goal:** the simulation decides, per stage, which `StageType` to play and which environment pool to draw from; everything is config-driven.

### A.1 StageType enum
`Shared/Scripts/Enums/StageType.cs` already has `None = 0, DeathMatch = 1`. Add:
```csharp
WhacAMole = 2,
```
Keep values stable — they are serialized as a `byte` in `MatchSimulationStateS2C`.

### A.2 Config (Simulation inner config)
`Simulation/Scripts/Configurations/SimulationGamePlayInnerConfig.cs` holds per-mode tuning (this is the `GamePlayConfig` reached via `ISimulationGamePlayConfigService.GamePlayConfig`). Add a nested `WhacAMoleConfig` and a rotation setting. Suggested:

```csharp
public bool IsWhacAMoleModeEnabled = true;
public int WhacAMoleEveryXStages = 3;   // every Nth stage entered is Whac-A-Mole
public WhacAMoleConfig WhacAMole;
```

Create `WhacAMoleConfig` (new `[System.Serializable]` class, e.g. `Simulation/Scripts/Configurations/WhacAMoleConfig.cs`) with at least:
```csharp
public float MatchDurationSeconds = 60f;   // the middle-screen countdown
public float MoleSpawnIntervalSeconds = 1.5f;
public int MaxConcurrentMoles = 8;
public float MoleRadius = 0.8f;
public float MoleLifetimeSeconds = 4f;     // 0 or negative => moles never auto-despawn
public int ScorePerMoleHit = 1;
```
Tune defaults later; the point is everything is a serialized field.

> **Note on the `ScriptableObject` assets:** `SimulationGamePlayConfig` / `SharedGamePlayConfig` are Unity `.asset` files. Adding serialized fields is safe (Unity fills defaults). The implementer should set sensible values in the Editor on `Assets/Core/Game/Domains/GamePlay/Simulation/Assets/Configs/SimulationGamePlayConfig.asset`. Flag in the PR that these asset values need a human pass.

### A.3 Environment pools per stage type
Today `EnvironmentConfig.AvailableLayoutIndexes` (`Shared/Scripts/Configs/EnvironmentConfig.cs`) is a single flat `List<int>` of layout indexes, and `InitStageCommand.GenerateRandomStageId()` picks from `SharedGamePlayConfig.Environment.AvailableLayoutIndexes`.

Give each mode its own pool. Preferred approach (minimal churn, backward compatible):
- Keep `AvailableLayoutIndexes` as the **DeathMatch** pool (or rename to `DeathMatchLayoutIndexes` and add a compatibility note).
- Add `public List<int> WhacAMoleLayoutIndexes;` to `EnvironmentConfig`.
- Add a helper `public List<int> GetLayoutIndexesForStageType(StageType stageType)` returning the right list. **Keep it a pure lookup** (Get = no mutation) — the "don't repeat until exhausted" bookkeeping (`DidntPlayYetStageIndexes`) stays in `GenerateRandomStageId()`, not here.

The actual environment layouts themselves are stored per index in `_environmentLayoutConfigs` (`SerializableDictionary<int, EnvironmentLayoutConfig>`); Whac-A-Mole layouts are just additional indexes authored in the Editor and referenced from `WhacAMoleLayoutIndexes`. No new layout-building code is needed — `InitStageCommand.CreateEnvironmentLayout` already builds any layout from its index.

### A.4 Per-stage StageType selection + pool selection in `InitStageCommand`
`InitStageCommand` (`Simulation/Match/Scripts/Commands/InitStageCommand.cs`) already tracks `private static int _stageNumber` and increments it at the end of `Execute()`. Wire the stage type here:

1. Early in `Execute()`, compute the upcoming stage type and store it on the simulation state:
   ```csharp
   var stageType = ResolveStageTypeForStage(_stageNumber);
   _matchDataService.SimulationState.StageType = stageType;
   ```
   where `ResolveStageTypeForStage` returns `WhacAMole` when
   `config.IsWhacAMoleModeEnabled && _stageNumber % config.WhacAMoleEveryXStages == 0`, else `DeathMatch`.
   (Match the existing `_stageNumber % EveryXStages == 0` idiom already used for the "add talent every X stages" feature in `SetupPlayers`.)
2. In `GenerateNextStageEnvironmentLayoutId()` / `GenerateRandomStageId()`, source the pool from `EnvironmentConfig.GetLayoutIndexesForStageType(stageType)` instead of the single `AvailableLayoutIndexes`. Keep the existing "don't repeat until pool exhausted" logic (`DidntPlayYetStageIndexes`) but make it per-pool aware (e.g. refill from the current stage type's pool).

> **Rejoin/serialization:** because `StageType` is set on `SimulationState` before the start-stage packet is built, joining/rejoining clients already receive the correct stage type via `MatchSimulationStateS2C.Serialize`. No extra work needed there.

**Acceptance (A):** With `WhacAMoleEveryXStages = 3`, server logs / state show stage types cycling `DeathMatch, DeathMatch, WhacAMole, DeathMatch, DeathMatch, WhacAMole…` and each Whac-A-Mole stage picks a layout from `WhacAMoleLayoutIndexes` only. No behavioral change yet (moles/scoring come later).

---

## 5. Work Package B — Mode branching (no PvP damage, no elimination)

**Goal:** in Whac-A-Mole, players take no damage from enemy bullets and the stage never ends via elimination. Talents keep working.

Read the current stage type anywhere via `_matchDataService.SimulationState.StageType`. Add a tiny helper if convenient (e.g. on `IStageDataService` or a small `IStageModeService`) like `bool IsWhacAMole => SimulationState.StageType == StageType.WhacAMole;`.

### B.1 Bullets don't damage players
In `ProcessCachedCollisionsCommand.cs`:
- `HandlePlayerHeartBulletCollision(...)` is the path that calls `PlayerHitCommand` for bullet→player. In Whac-A-Mole, **still destroy the bullet** (so it doesn't tunnel) but **skip the `PlayerHitCommand`** damage call. Simplest: guard the `_playerHitCommand...Execute()` block with `if (!IsWhacAMole)`.
- `HandlePlayerBulletCollision(...)` already only destroys the bullet on the ship body — leave as is (or, if design prefers bullets pass *through* teammates/enemies entirely in this mode, gate bullet-vs-player collision at the filter level; see note below). Recommended default: bullets still visually stop on players but deal no damage. Confirm desired feel with design.

> Alternative (cleaner but bigger): give players a different collision profile in Whac-A-Mole so bullets ignore ships entirely. That touches `PhysicsCollisionFilters`/player fixture creation and is stage-type dependent at body-creation time — heavier. Prefer the guard approach unless the "pass-through" feel is required.

### B.2 No elimination win in Whac-A-Mole
`PlayerHitCommand.KillPlayer` calls `TryAddLosingTeam` + `TryInvokeMatchEnded` (elimination → `StageEndedCommand`). Since players won't take lethal damage in Whac-A-Mole (B.1 removes the only PvP damage source, and there's "no health"), this path shouldn't fire. To be safe and explicit, guard `TryAddLosingTeam`/`TryInvokeMatchEnded` so they are **no-ops in Whac-A-Mole** — the stage ends only via the timer (Work Package E).

Environmental hazards (lava/spikes) also call `PlayerHitCommand`. Decide per design whether hazards exist in Whac-A-Mole layouts; simplest is to author Whac-A-Mole layouts without lava/spikes so the question is moot. If hazards must be disabled generally in this mode, guard those damage calls too.

### B.3 Player setup in Whac-A-Mole
In `InitStageCommand.SetupPlayers`, players are given `StartHealth`. In Whac-A-Mole health is irrelevant (nothing damages them). Leave setup as-is (harmless) — do **not** special-case unless the HUD would show a health bar; the presentation package (F) hides/ignores the health bar in this mode.

**Acceptance (B):** In a forced Whac-A-Mole stage, shooting an enemy player does nothing to their health, no one dies, and the stage does not end from combat.

---

## 6. Work Package C — Mole entity (state, physics, spawner, collision, net events)

**Goal:** Moles exist as first-class simulated entities: spawned server-side, replicated in state, hit-detected via physics, removed on hit with a net event.

Use existing entity patterns as templates. **PowerUpBall is the closest analog** (spawns at random free positions, lives in `SimulationState`, has a spawn net event and an "obtained"/removed net event, has an MVC on the client). Mirror it.

### C.1 Mole state model
Add `MoleStateS2C` — `Shared/Scripts/S2CModels/MoleStateS2C.cs`. Model after `PowerUpBallS2C`. Value struct implementing `INetSerializable`:
```csharp
public ushort Id;
public System.Numerics.Vector2 Position;
// optional: float RemainingLifetimeSeconds; (server-only need not serialize if not shown)
```
Serialize position quantized like other entities.

### C.2 Add Moles to `MatchSimulationStateS2C`
In `Shared/Scripts/S2CModels/MatchSimulationStateS2C.cs`:
- Declare `public FixedUnorderedList<MoleStateS2C> Moles;`
- Initialize in the constructor with a new capacity arg (e.g. `maxMoles`) threaded from `NetworkConfig.MaxCap`.
- Add count-prefixed serialize/deserialize blocks in `Serialize`/`Deserialize` (follow the `PowerUpBalls` block exactly).
- Add `Add`/`TryGetMoleIndexById`/`GetMoleById`/`RemoveMoleById` helpers (copy the PowerUpBall helper set).
- Clear `Moles` in `ClearObjectStates()`.
- Add `Moles` capacity to `NetworkConfig.MaxCap` (`Core/Scripts/Network/NetworkConfig.cs`).

> The `MatchSimulationStateS2C` constructor is called in a few places (e.g. `MatchDataService`, packet constructors). Update all call sites with the new capacity argument — grep for `new MatchSimulationStateS2C(`.

Also add an `AddMole(...)` convenience on `IMatchDataService`/`MatchDataService` mirroring `AddPowerUpBall`.

### C.3 Physics body + collision filter for Moles
`Simulation/Scripts/Physics/PhysicsBodyType.cs`:
- Add `Mole = 21` to `PhysicsBodyType` (next free value; current max is `SoulGhost = 20`).
- Add `Mole = 15` to `PhysicsCollisionType` (next free value; current max is `SoulGhost = 14`).

`Simulation/Scripts/Physics/PhysicsCollisionFilters.cs`:
- Add a `case PhysicsBodyType.Mole:` whose category mask lets it be hit by **PlayerBullet** and **PlayerSpaceship** (for spin hits):
  ```csharp
  case PhysicsBodyType.Mole:
      collisionMask = GetCollisionMask(PhysicsCollisionType.PlayerBullet)
                      | GetCollisionMask(PhysicsCollisionType.PlayerSpaceship);
      break;
  ```
- Add `GetCollisionMask(PhysicsCollisionType.Mole)` to the **PlayerBullet** case and the **PlayerSpaceship** case so those bodies see moles.
  - ⚠️ Adding Mole to the PlayerBullet mask means bullets will now generate contacts with moles in *all* modes. That's fine because moles only exist in Whac-A-Mole stages (none are spawned otherwise), so there are no mole bodies to contact in DeathMatch.

`Simulation/Scripts/Physics/IPhysicsSimulator.cs` + `PhysicsSimulator.cs`:
- Add `void AddMole(ushort id, System.Numerics.Vector2 position, float radius);` and `void RemoveMole(ushort id);` (model after `AddPowerUpBall`/`RemoveBody`/`RemovePowerUpBall`). A static circle sensor/solid body is fine — copy the PowerUpBall body creation but with `PhysicsBodyType.Mole` and zero velocity (moles are stationary; if design wants moving moles, give them velocity like power-ups).

### C.4 Mole spawner command
Create `TrySpawnMolesCommand` — `Simulation/Match/Scripts/Commands/TrySpawnMolesCommand.cs`, **modeled directly on `TrySpawnPowerUpBallsCommand.cs`**:
- Only spawn when `SimulationState.StageType == WhacAMole` and not in preparation phase.
- Own spawn-interval timer (add an `IMolesSpawnerService` mirroring `IPowerUpsSpawnerService`/`PowerUpsSpawnTimerService`, OR reuse a simple float timer in a dedicated service — keep the "restart timer when elapsed" pattern). Interval + max concurrent + radius come from `WhacAMoleConfig`.
- Cap at `WhacAMoleConfig.MaxConcurrentMoles`.
- Find a free position with `_physicsSimulator.IsSquareHitAnyBodyTypes(pos, radius, PhysicsBodyType.Wall, ...)` — reuse `TryFindAvailablePosition`/spawn-point logic from the power-up spawner (moles can reuse the same free-space search; they don't need dedicated spawn points unless design wants them).
- On spawn: `_matchDataService.AddMole(...)`, `_physicsSimulator.AddMole(...)`, and fire a **MoleSpawned net event** (see C.6).

Register it in `ServerMatchNetworkTickProcessor`:
- Add a `TrySpawnMolesCommand _trySpawnMolesCommand;` field, create it in `InitEntryPoint()` via `_commandFactory.CreateCommandVoid<...>()`, and call `_trySpawnMolesCommand.SetProcessedTick(currentTick).Execute();` inside `OnTick`, right next to `_trySpawnPowerUpBallsCommand`. It self-gates on stage type so it's safe to call every tick.

If moles have a lifetime (`MoleLifetimeSeconds > 0`), step it in `StepTimersCommand` (or in the spawner's `OnTick`) and despawn expired moles with the same removal path as a hit (but you may want a distinct "expired" vs "hit" net event, or reuse MoleHit with a flag — simplest: a `MoleRemoved` event carrying a reason, or just remove silently on expire and let the client fade). Keep it minimal; lifetime is optional per A.2.

### C.5 Mole collision handling (the actual "hit")
In `ProcessCachedCollisionsCommand.cs`, add two handlers, called from the `Begin` block in `ProcessCollisions()` (next to the other `Handle...` calls):

1. **`HandleBulletMoleCollision(objectA, objectB, contact)`** — bullet ↔ Mole:
   - Detect the pair (copy the `HandlePlayerBulletPowerUpCollision` shape).
   - Resolve the bullet's owning player (`bulletModel.BelongToPlayerId`) → its `TeamId`.
   - Destroy the bullet (`DestroyBullet`), call the shared **`HitMole(moleId, byPlayerId, teamId, tick)`** routine (C.7), which removes the mole + scores + fires MoleHit net event.

2. **`HandleSpinPlayerMoleCollision(objectA, objectB)`** — player ↔ Mole, only counts if the player **is currently spinning**:
   - Detect the pair (copy `HandleChickenEggPlayerCollision` shape).
   - Gate on the player actually spinning: check `player.Spaceship.IsSpinned` (set/cleared by `SpinPlayerCommand` — see `PlayerStateS2C`/`Spaceship`). Only then count the hit.
   - Call `HitMole(moleId, player.Id, player.TeamId, tick)`.

> Both handlers must tolerate the "already removed this tick" race — use `TryGetMoleIndexById`/`TryGet...` and bail if gone, exactly like the existing handlers log-and-return when a bullet was "already destroyed in this frame."

### C.6 / C.7 Net events + shared hit routine
Use the **`/add-net-event` skill** for each event. Two events:
- **`MoleSpawnedNetEventS2C`** (struct): `{ int OccuredOnTick; ushort MoleId; Vector2 Position; }` — mirrors `PowerUpSpawnedNetEvent`.
- **`MoleHitNetEventS2C`** (struct): `{ int OccuredOnTick; ushort MoleId; ushort ByPlayerId; ushort TeamId; }` — drives despawn VFX + score popup on the client.

For each: add the `MaxCap` capacity, add the field + serialize/deserialize + **`eventMask2`** bit (next free bit in `CalculateEventMask2()` — remember primary mask is full, Soul uses eventMask2 bits 0–1), add the per-client queue + `Add...` method in `NetEventsDataService`/`INetEventsDataService`, wire the field assignment in `ServerMatchNetworkTickProcessor.SendCurrentTickStateToAllClients`, and unpack in `MatchFullTickPacketsHandler`. The skill enumerates all 7 touch-points; follow it exactly.

Put the shared removal logic in one place (e.g. a `HitMoleCommand` or a private method reused by both collision handlers, or on the Whac-A-Mole score service from Package E):
```
HitMole(moleId, byPlayerId, teamId, tick):
    if !TryGetMoleIndexById(moleId) return
    RemoveMoleById(moleId); physicsSimulator.RemoveMole(moleId)
    whacAMoleScoreService.AddScoreForTeam(teamId, config.ScorePerMoleHit)   // Package E
    netEventsDataService.AddMoleHitNetEvent(tick, moleId, byPlayerId, teamId)
```

**Acceptance (C):** In a forced Whac-A-Mole stage, moles appear at intervals up to the cap; shooting one or spinning into one removes it and emits a MoleHit event; a moving (non-spinning) player brushing a mole does **not** remove it.

---

## 7. Work Package E — Whac-A-Mole scoring + timer win condition

**Goal:** track moles-hit per team, run the middle-screen countdown, and end the stage with the correct winner when it expires.

### E.1 Per-team score
Reuse the existing per-team-score plumbing pattern. `StageDataService` already has `GemsCollectedPerTeam` (`Dictionary<ushort,int>`) with `AddGemsForTeam` and per-team init in `InitEntryPoint`/`ClearData`. Add a parallel `MolesHitPerTeam` dictionary (or generalize gems→"score") with `AddMoleHitForTeam(teamId, amount)`, initialized/cleared the same way. Keep it on `IStageDataService` so `StageEndedCommand` and the tick processor can read it.

`HitMole` (C.7) calls `AddMoleHitForTeam(teamId, config.ScorePerMoleHit)`.

### E.2 Match timer (networked, shown mid-screen)
The client must render a countdown, so remaining time (or an end-tick) must reach the client. Two viable approaches — pick one:

- **(Preferred) End-tick on simulation state.** Add `public int WhacAMoleEndTick;` (or `float WhacAMoleTimeRemaining`) to `MatchSimulationStateS2C`, set it when a Whac-A-Mole stage inits (`InitStageCommand`: `EndTick = currentTick + Ceil(MatchDurationSeconds * tickRate)`), and serialize it (add to `Serialize`/`Deserialize` next to the other stage fields). Client computes remaining = `(EndTick - currentTick) / tickRate`. This is rejoin-safe and needs no per-tick event. Gate serialization/use on `StageType == WhacAMole`.
- (Alt) A dedicated `IWhacAMoleTimerService` stepped in `StepTimersCommand` with the remaining time mirrored into state each tick. More moving parts; only do this if you need pause semantics.

Add a timer service only if you go the Alt route. For the Preferred route, the "timer" is just arithmetic on ticks — no new service, use the existing `ITickService.CurrentTick`.

### E.3 End condition
In `ServerMatchNetworkTickProcessor.OnTick` (server tick loop), after the existing `TryHandleStageEnded` early-return, add a check (only when `StageType == WhacAMole` and stage not already ended):

```
if (StageType == WhacAMole && currentTick >= WhacAMoleEndTick):
    winningTeamId = whacAMole winner from StageDataService.MolesHitPerTeam   // §7.4
    StageEndedCommand.SetWinningTeamId(winningTeamId).SetProcessedTick(currentTick).Execute();
```

`StageEndedCommand` already: sets `CurrentStageWinnerTeamId`, `IsInShowoffWinners`, fires `StageEndNetEvent` (which carries per-team scores — pass `MolesHitPerTeam` in place of / alongside gems), sets `IsStageEnded = true` and `StageRestartTimer`. Then the existing `TryHandleStageEnded` path restarts into the next stage via `InitStageCommand` — which will pick the next stage type per the rotation. **No changes needed to the restart flow.**

Put this end-check behind a small command (`TryEndWhacAMoleStageCommand`) created in `InitEntryPoint` and called each tick, mirroring how other per-tick commands are structured, rather than inlining logic in the processor.

`StageEndedCommand.GetPlayerToFocusOn()` assumes an alive winning-team player; in Whac-A-Mole everyone is alive, so it works, but there's no "winning blow" player. Pass `PlayerIdDoingWinningBlow` = the last mole-hitter if you track it, else it falls through to the first winning-team player — acceptable.

### E.4 Winner selection & tie-break
Winner = team with max `MolesHitPerTeam`. **Define tie-break explicitly:**
- Recommended: on a tie, declare a draw. `StageEndedCommand`/`StageEndNetEvent` currently expects a single `winningTeamId`. Either (a) pick the lowest teamId among tied leaders as a simple deterministic winner, or (b) extend the end event with an `IsDraw` flag and have the presentation show "Draw". Simplest shippable: deterministic lowest-teamId winner; note it as a follow-up if design wants true draws.

**Acceptance (E):** A forced Whac-A-Mole stage counts moles per team, shows a shrinking timer client-side, and at expiry ends with the higher-scoring team shown as winner, then rotates into the next stage.

---

## 8. Work Package F — Presentation (Mole MVC, timer UI, scoreboard)

**Goal:** render moles, the countdown, and mode-appropriate HUD. Presentation is pure rendering from tick state + net events; **it must never contain game logic.**

### F.1 Mole MVC
Create a Mole feature under `Presentation/Match/Features/Mole/Scripts/` following the **PowerUpBall** feature layout (`Features/PowerUpBall/Scripts/Mvc` + `ObtainedEffect`):
- `MoleView` (MonoBehaviour: sprite/animator refs), `MoleController` (pure C#), `MoleControllers` (pool + lifecycle), plus a hit/despawn effect controller mirroring `PowerUpBallObtainedEffectController`.
- Bind them in `Presentation/Match/Scripts/ZenjectInstallers/GamePlayMatchInstaller.cs` (inject the Mole view prefab like the power-up ball prefab is injected).
- Drive spawns/removes with `Handle...NetEventsCommand`s (one per new net event), reading from `ICachedPresentationEventsService` and calling `events.Clear()`, exactly like the power-up spawn/obtained commands. Register these commands in `ClientMatchPresentationTickProcessor.ManagedUpdate()`.
- Add `List<MoleSpawnedNetEventS2C>` / `List<MoleHitNetEventS2C>` to `CachedPresentationEventsService` and populate them from `PresentationMatchNetEventsHandler` (the `/add-net-event` skill covers the client cache wiring).
- Moles are stationary and few; also reconcile against `SimulationState.Moles` each tick (the state list is authoritative for rejoin) — follow how power-up balls are reconciled from state so a rejoining client sees existing moles even without the spawn event.

### F.2 On-screen countdown timer
- Add a countdown widget centered on screen, shown only when `SimulationState.StageType == WhacAMole`. Compute remaining seconds from the networked end-tick (E.2) and `ITickService`/current tick on the client. Look at existing match UI under `Presentation/Match/Features/UI/Scripts/` (e.g. `MatchPlayerUIController`, `TeamsBoard/TeamsBoardUIController`) for the HUD binding pattern and where to hook per-tick updates.

### F.3 Mode-aware HUD
- Hide/disable the health bar in Whac-A-Mole (`MatchPlayerUIController`/`MatchPlayerUIView` own the health display).
- Show a "Moles hit per team" scoreboard. `TeamsBoardUIController` already renders per-team info (gems/bolts) and consumes `StageEndNetEvent` payloads — extend it to show moles-hit in this mode, reusing the per-team score carried in the events/state from Package E.
- The stage-end/winner screen (`StageEndedUiController`/`StageEndedUiView`) already reacts to `StageEndNetEvent`; make sure it presents the Whac-A-Mole result (winner + scores, or draw) using the same event.

**Acceptance (F):** Client shows moles appearing/vanishing with VFX, a centered countdown during Whac-A-Mole, a moles-hit scoreboard, no health bar, and a correct winner screen at timeout. Rejoining mid-stage shows current moles, score, and remaining time.

---

## 9. Cross-cutting checklist / gotchas

- [ ] **`/coding-guidlines` skill followed** (see §2.5): zero-alloc, commands, `Get`≠mutate, `Try` prefixes, unit-suffixed time fields, method ≤30 / class ≤200 lines, no magic numbers.
- [ ] **Every `new MatchSimulationStateS2C(...)` call site** updated for the new `maxMoles` arg (grep it).
- [ ] **`ClearObjectStates()`** clears `Moles` (so stage restart wipes them) — verify a DeathMatch stage after a Whac-A-Mole stage has zero mole bodies.
- [ ] **Net-event bits use `eventMask2`** (primary mask full); pick the next free bit after Soul's bits — read `CalculateEventMask2()`.
- [ ] **Zero-alloc:** no `new`/LINQ in per-tick paths; size all lists from `MaxCap`; allocate in `ResolveDependencies`/entry points.
- [ ] **Spin/force via commands only** (`TrySpinPlayerCommand`, `AddForceToPlayerCommand`) — relevant if moles ever push players.
- [ ] **`for` not `foreach` when iterating collisions** and when removing entities mid-iteration (see the comment in `ProcessCollisions`).
- [ ] **Stage-type gating is centralized** — read `SimulationState.StageType`; don't scatter `_stageNumber % X` checks outside `InitStageCommand`.
- [ ] **Config assets** (`SimulationGamePlayConfig.asset`, `EnvironmentConfig`, `SharedGamePlayConfig.asset`) need Editor values set + Whac-A-Mole layouts authored and added to `WhacAMoleLayoutIndexes`. Call this out for a human.
- [ ] **Every `Register`/subscribe paired with an `Unregister`/unsubscribe** (tick observers, MVC listeners in Package F).
- [ ] **Presentation is render-only** — no game logic on the client; strict View/Controller boundaries (§2.5 MVC).
- [ ] **`_stageNumber` static** reuse understood — process-global, not reset per match; raise if per-match rotation reset is required.
- [ ] **Bullet-vs-player feel** (stop-no-damage vs pass-through) confirmed with design.
- [ ] **Tie-break** behavior confirmed with design (deterministic winner vs draw).

## 10. Suggested manual test plan

1. Set `WhacAMoleEveryXStages = 1` temporarily → every stage is Whac-A-Mole. Verify moles spawn/cap/despawn-on-hit (bullet + spin), timer counts down, winner at expiry, rotation continues.
2. Set back to `3`, play through ≥4 stages → verify `DM, DM, WhacAMole, DM…` cycle and that Whac-A-Mole stages only use `WhacAMoleLayoutIndexes` layouts.
3. In Whac-A-Mole: shoot an enemy → no damage/no death; use a spin/KO talent on an enemy → still works.
4. PowerUp balls still spawn and are collectible in Whac-A-Mole.
5. Rejoin mid Whac-A-Mole stage → moles, per-team score, and remaining time all correct.
6. Regression: a normal DeathMatch stage still ends by elimination and has no moles/no countdown.
```
