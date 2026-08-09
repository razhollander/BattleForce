# GatePass StageType + Bonus Stages — Implementation Spec

> **Audience:** agents/engineers implementing this feature.
> **Working directory for all code paths below:** `Assets/`
> Read `Assets/CLAUDE.md` first for the architecture overview, and `WhacAMoleStageType.md` for the spec of the mode this one is modelled on. This doc assumes both.
> Whac-A-Mole is **already implemented and merged** — this feature reuses its plumbing rather than inventing new plumbing.

---

## 1. Feature summary

1. Add **`StageType.GatePass = 3`**.
2. Introduce the concept of a **Bonus Stage** = `{ WhacAMole, GatePass }`. Every **X** stages (existing `WhacAMoleEveryXStages`, renamed) the players enter a Bonus Stage instead of a DeathMatch stage. **Which** bonus stage is chosen must differ from the previous bonus stage played (was Whac-A-Mole last time → GatePass this time, and vice versa). The very first bonus stage of a match is chosen randomly.
3. GatePass rules:
   - No player health, no deaths, no elimination win — exactly like Whac-A-Mole.
   - The arena contains one (or more) **`ScoreGateObstacle`**: a *dynamic* physical object made of **two square colliders with a configurable gap between them**, with **medium mass**. Players can push it by ramming it, and any talent that pushes/spins things (KO, Rock, grappling-hook drag, frigid block, headbutt…) affects it too.
   - Every time a player **passes through the gap**, that player's **team scores +1**, a **`+1` popup pops from the gate** (same effect family as the mole-hit popup), and the **gate is tinted with the scoring player's team colour**.
   - A **countdown timer** runs (same widget as Whac-A-Mole). At zero the stage ends; **the team with the most points wins** and gems are awarded **by rank** (identical algorithm to `TryEndWhacAMoleStageCommand.AwardGemsByRank`).
   - Team points are shown in the **top-middle HUD** (the existing per-team board slot Whac-A-Mole uses), and each player's own contribution is shown on **his player UI** (the slot that replaces the health bar in Whac-A-Mole).
4. Author **one simple rectangular GatePass layout**, in its own environment-layout pool (like `WhacAMoleLayoutIndexes`).

Everything runs inside the existing server-authoritative 60 tick/s simulation. The server owns every decision; the client renders state + net events.

---

## 2. Grounding — what already exists and is reused

| Concern | File |
|---|---|
| Stage type enum | `Core/Game/Domains/GamePlay/Shared/Scripts/Enums/StageType.cs` |
| Stage selection + rotation counter (`static int _stageNumber`) | `Simulation/Match/Scripts/Commands/InitStageCommand.cs` (`ResolveStageTypeForCurrentStage`, `SetupWhacAMoleStageData`, `CreateEnvironmentLayout`) |
| Per-stage-type layout pools | `Shared/Scripts/Configs/EnvironmentConfig.cs` (`GetLayoutIndexesForStageType`), `Simulation/Match/Scripts/MatchModel/MatchDataService.cs` (`_didntPlayYetStageIndexesPerStageType`) |
| Layout payloads (JSON blobs per layout index) | `Shared/Scripts/Configs/EnvironmentLayoutConfig.cs` |
| Layout authoring buttons | `Shared/LevelEnvironment/Scripts/EnvironmentGenerator.cs`, `MoleSpawnPoint.cs` |
| Bonus-stage score, per team + per player | `Shared/Scripts/S2CModels/MatchSimulationStateS2C.cs` (`MolesHitPerTeamId`, `AddMolesHitScoreForPlayer`), `Shared/Scripts/S2CModels/PlayerStateS2C.cs` (`MolesHitScore`) |
| Countdown end tick | `MatchSimulationStateS2C.WhacAMoleEndTick`, client `UpdateWhacAMoleCountdownCommand` |
| Timer end + gems by rank | `Simulation/Match/Scripts/Commands/TryEndWhacAMoleStageCommand.cs` |
| "No damage in this mode" guard | `Simulation/Match/Scripts/Commands/TryHitPlayerCommand.cs:92` (`_stageDataService.IsWhacAMoleStage`) |
| Lock-on retarget per mode | `Simulation/Match/Scripts/PlayerLockOnTarget/TrySendPlayersLockOnTargetChangedCommand.cs:70` |
| **Closest analog for the gate body** (dynamic, physics-driven, body→state each tick, client interpolates) | `Simulation/Scripts/Physics/PhysicsSimulator.cs` `AddFrigidBlock`, `StepPhysiscsSimulationCommand.ApplyPhysicsSimulationToMatchModel`, `Presentation/Match/Features/FrigidBlock/…`, `UpdateFrigidBlocksTransformCommand` |
| `+1` popup | `Presentation/Match/Features/MoleHitScoreEffect/` |
| Team board (top-middle HUD) | `Presentation/Match/Features/UI/Scripts/TeamsBoard/` |
| Per-player score slot | `Presentation/Match/Features/UI/Scripts/MatchPlayerUIControllers.cs` (`switch (_matchDataService.StageType)`), `MatchPlayerUIView` |
| Countdown widget | `Presentation/Match/Features/WhacAMoleCountdown/` |
| Full-state / rejoin rebuild | `Presentation/Match/Scripts/Commands/NetEvents/SyncMatchSimulationStateCommand.cs` |
| Net event checklist | `/add-net-event` skill (`Assets/.claude/skills/add-net-event/`) |

**Facts to respect (verified in code, not assumed):**

- **The primary 64-bit event mask is full.** `MatchFullTickPacketS2C` uses a second `eventMask2`; **bits 0–11 are taken** (Soul 0–1, Rock 2–3, Lava 4–5, Frozen 6–7, Moles 8–11). **The GatePass event takes `eventMask2` bit 12.**
- `PhysicsBodyType` max used value is `Mole = 21`; `PhysicsCollisionType` max used value is `Mole = 15`. Collision types are **bit indexes** into a 32-bit field, so 31 is the ceiling.
- **Almost every talent projectile is a Box2D *sensor*** (`fixtureDef.isSensor = true`): `KOProjectile`, `GrapplingHookProjectile`, `FishingRodTip`, `SoulGhost`, `SwapField`, `ChickenEgg` and `Mole` all are. A sensor generates contact *events* but **zero solver impulse**, so none of these will push the gate on their own. The only solid bodies that can shove the gate through the physics solver are `PlayerSpaceship` (incl. the Rock-enlarged body), `PlayerBullet`, `PowerUpBall` and `FrigidBlock`. Every other talent interaction must be applied manually with `Body.ApplyLinearImpulse` / `ApplyAngularImpulse` (both exist in the vendored port, `Bodies/Body.cs:470,509`). **See the full matrix in §6.4.**
- **Three talents hit through geometric casts, not contacts** — `MagneticPull` (`ArcCastByPriority`), `YearsOfPain` (`RectangleCastByPriority`) and `WaterGun` (`EllipseCastOnPlayers`). They never produce a collision event at all, so they are invisible to `ProcessCachedCollisionsCommand`; reaching the gate means passing `PhysicsBodyType.ScoreGate` into the cast itself.
- **Two talents move a player by writing `Transform.Position` directly** — `SwapTalentController.cs:157` and the teleport gate at `ProcessCachedCollisionsCommand.cs:814`. These are discontinuous jumps and they break the pass-detection segment test unless invalidated (§7.2).
- `BombTalentController` and `HammerTalentController` are **empty stubs** — no `TalentType` entries, nothing to integrate.
- Player bodies get their velocity **overwritten from the simulation state every tick** (`PhysicsSimulator.CopyPlayerStateToBody`), which is why `HandlePlayerFrigidBlockCollision` manually reflects the player's velocity in state. The gate needs the same treatment.
- `FrigidBlock` is **not** copied state→body; it is physics-driven and copied body→state in `ApplyPhysicsSimulationToMatchModel`. The gate does the same.
- `PolygonShape.SetAsBox(hx, hy, center, angle)` exists — one body can hold both gate posts as two offset boxes.
- `MatchDataService._didntPlayYetStageIndexesPerStageType` is a hard-coded dictionary with **only** `DeathMatch` and `WhacAMole` keys. **Adding `GatePass` there is mandatory** or `GetDidntPlayYetStageIndexes(GatePass)` throws `KeyNotFoundException` the first time a GatePass stage is rolled.

---

## 2.5 Coding conventions every package MUST follow

**Invoke the `/coding-guidlines` skill before writing code.** The rules most likely to bite here:

- **Zero garbage allocation in the Simulation domain.** No `new`, LINQ, boxing, closures or `params` on any per-tick path. The pass-detection tracker (§6.4) pre-allocates its per-player previous-position and cooldown storage from `NetworkConfig.MaxCap` in an entry point.
- **Commands:** one job, fluent `SetX(...)` setters, `Execute()`. Create them in `ResolveDependencies()` / `InitEntryPoint()` via `_commandFactory.CreateCommandVoid<T>()` — never per tick.
- **`Get*` never mutates**; `Try` prefix for anything that can early-exit.
- **Naming:** enums suffixed `Type`, booleans prefixed `is/can/has`, **time fields carry their unit** (`PassScoreCooldownSeconds`), no abbreviations, no magic numbers — every tuning value lives in `GatePassConfig`.
- **Never write `Velocity`/`AngularVelocity` on a player directly** — use `AddForceToPlayerCommand` / `SpinPlayerCommand`. (The *gate* is not a player; it is driven through the Box2D body and is exempt.)
- Presentation is **render-only**. Model mutation belongs in `PresentationMatchNetEventsHandler.Process*Events`; `Handle*NetEventsCommand` only reads and clears. See the "Who writes to `IMatchDataService`" table in `Assets/CLAUDE.md`.
- Method ≤ 30 lines, class ≤ 200 lines.

---

## 3. Work packages & dependency order

```
0. Generalize "Whac-A-Mole score/timer" → "Bonus Stage score/timer"   (blocking, mechanical rename)
        │
A. Foundation: StageType.GatePass, bonus-stage rotation, config, layout pool
        │
        ├── B. ScoreGateObstacle entity: state, physics body, spawn from layout, interactions
        │        └── C. Pass detection + scoring + ScoreGatePassed net event
        │                 └── D. Stage end / gems / mode branching
        │
        └── E. Presentation: gate MVC, +1 popup, team colour, HUD, countdown, rejoin
                  │
                  └── F. Stage authoring: ScoreGate authoring component + the rectangular layout
```

Package 0 must land first and alone (it is a pure rename with no behaviour change, so it is trivially reviewable). A is blocking for B–F. B→C→D are sequential. E depends on C's net event existing. F is independent of C/D and can run in parallel with E.

---

## 4. Work Package 0 — Generalize Whac-A-Mole scoring/timer to "Bonus Stage" (blocking, no behaviour change)

**Goal:** GatePass needs *exactly* the same score-per-team, score-per-player, countdown-end-tick and gems-by-rank machinery that Whac-A-Mole already has. Rather than duplicating four parallel sets of fields, rename the existing ones to be mode-neutral. **This package changes no behaviour** — after it lands, Whac-A-Mole must play identically.

> **Why the rename and not parallel fields:** a second set (`GatePassScorePerTeamId`, `GatePassEndTick`, `PlayerStateS2C.GatePassScore`, `UpdateTeamGatePassScore`, a second countdown command…) would double the serialization surface of the tick packet and force every consumer to branch on stage type. One neutral set means §6–§8 only add the *scoring trigger*, and every future bonus stage is free.

### 0.1 Shared / simulation-state renames

| From | To | File |
|---|---|---|
| `MatchSimulationStateS2C.MolesHitPerTeamId` | `BonusScorePerTeamId` | `Shared/Scripts/S2CModels/MatchSimulationStateS2C.cs` |
| `MatchSimulationStateS2C.WhacAMoleEndTick` | `BonusStageEndTick` | same |
| `AddMolesHitForTeam` | `AddBonusScoreForTeam` | same |
| `ResetMolesHitPerTeam` | `ResetBonusScorePerTeam` | same |
| `AddMolesHitScoreForPlayer` | `AddBonusScoreForPlayer` | same |
| `ResetMolesHitScoreForAllPlayers` | `ResetBonusScoreForAllPlayers` | same |
| `PlayerStateS2C.MolesHitScore` | `PlayerStateS2C.BonusScore` | `Shared/Scripts/S2CModels/PlayerStateS2C.cs` |
| `MoleHitNetEventS2C.TeamMolesHitTotal` | `TeamBonusScoreTotal` | `…/PacketEvents/NetEvents/MoleHitNetEventS2C.cs` |
| `MoleHitNetEventS2C.ByPlayerMolesHitScoreTotal` | `ByPlayerBonusScoreTotal` | same |

Wire format is **unchanged** (same order, same types) — this is a source-level rename only, so client and server stay compatible as long as both are rebuilt.

### 0.2 Simulation renames

- `IStageDataService` / `StageDataService`: keep `IsWhacAMoleStage` (mole-specific call sites still want it) and **add** `bool IsBonusStage => SimulationState.StageType.IsBonusStage();`.
- `TryEndWhacAMoleStageCommand` → **`TryEndBonusStageCommand`** (`git mv` the `.cs` *and* its `.meta` so the GUID survives). Its `isCountdownOver` condition becomes `simulationState.StageType.IsBonusStage() && …`. Extract the mole teardown into a private `HideAllMoles()` that early-returns when `StageType != WhacAMole` (GatePass has nothing to tear down — the gate stays on screen during the winner showoff).
- `ServerMatchNetworkTickProcessor`: rename the field/creation/call accordingly.

### 0.3 Presentation renames

| From | To |
|---|---|
| `ITeamsBoardUIController.UpdateTeamMolesHit` / `SetIsMolesHitShown` | `UpdateTeamBonusScore` / `SetIsBonusScoreShown` (+ `TeamsBoardUIController`, `TeamsBoardContainerView`, `TeamBoardUIView`) |
| `IMatchPlayerUIControllers.UpdatePlayerMolesHitScore`, `MatchPlayerUIController.ShowMolesHitScore/UpdateMolesHitScore`, `MatchPlayerUIView.ShowMolesHitScore/UpdateMolesHitScore` | `…BonusScore` |
| `MatchPlayerModel.MolesHitScore`, `IMatchDataService.SetPlayerMolesHitScore` / `SetTeamMolesHit` | `BonusScore`, `SetPlayerBonusScore`, `SetTeamBonusScore` |
| `UpdateWhacAMoleCountdownCommand` | `UpdateBonusStageCountdownCommand` |
| `Features/WhacAMoleCountdown/` (`IWhacAMoleCountdownController`, `WhacAMoleCountdownController`, `WhacAMoleCountdownView`) | `Features/BonusStageCountdown/` (`IBonusStageCountdownController`, …) |
| `Features/MoleHitScoreEffect/` | `Features/BonusScoreEffect/` (`IBonusScoreEffectController`, `BonusScoreEffectView`, `BonusScoreEffectPool`) |

⚠️ **Unity asset gotchas for this package — do not skip:**

- **MonoBehaviour renames** (`WhacAMoleCountdownView`, `MoleHitScoreEffectView`): move the `.cs` **together with its `.meta`** (`git mv a.cs b.cs && git mv a.cs.meta b.cs.meta`) and rename the class to match the new file name. The MonoScript GUID is in the `.meta`, so prefab/scene references survive. If the `.meta` is left behind Unity mints a new GUID and **every prefab reference silently becomes `Missing (Mono Script)`**.
- **Serialized field renames on views** (e.g. `MatchPlayerUIView._molesHitScoreText`, `_molesHitScoreContainer`; `TeamBoardUIView._molesHitContainer`, `_molesHitCountText`) require `[UnityEngine.Serialization.FormerlySerializedAs("_molesHitScoreText")]` or the prefab reference is lost.
- The Zenject binding fields in `GamePlayMatchInstaller` (`_whacAMoleCountdownView`, `_moleHitScoreEffectViewPrefab`) are also serialized — same `FormerlySerializedAs` treatment, and re-verify the assignments in the `GamePlayMatchScene` after the rename.

**Acceptance (0):** Whac-A-Mole plays exactly as before — moles spawn, score updates in the team board and player UI, countdown runs, gems awarded by rank, rejoin correct. `git grep -i "moleshit\|whacamolecountdown\|molehitscoreeffect"` returns nothing outside the Mole feature itself.

---

## 5. Work Package A — Foundation: StageType, bonus-stage rotation, config, layout pool

### A.1 Enum + helper

`Shared/Scripts/Enums/StageType.cs`:
```csharp
public enum StageType
{
    None = 0,
    DeathMatch = 1,
    WhacAMole = 2,
    GatePass = 3,
}
```
Values are serialized as a `byte` in `MatchSimulationStateS2C` — **never renumber existing values.**

Add `Shared/Scripts/Enums/StageTypeExtensions.cs`:
```csharp
public static class StageTypeExtensions
{
    public static bool IsBonusStage(this StageType stageType)
    {
        return stageType == StageType.WhacAMole || stageType == StageType.GatePass;
    }
}
```
This is the single definition of "bonus stage"; nothing else may hard-code the pair.

### A.2 Config

`Simulation/Scripts/Configurations/SimulationGamePlayInnerConfig.cs` — rename and extend (use `[FormerlySerializedAs]` on the renamed fields so the existing `SimulationGamePlayConfig.asset` values survive):

```csharp
[FormerlySerializedAs("IsWhacAMoleModeEnabled")] public bool IsBonusStagesEnabled = true;
[ConditionalField(nameof(IsBonusStagesEnabled), true)]
[FormerlySerializedAs("WhacAMoleEveryXStages")] public int BonusStageEveryXStages = 3;
[ConditionalField(nameof(IsBonusStagesEnabled), true)]
public List<StageType> EnabledBonusStageTypes = new List<StageType> { StageType.WhacAMole, StageType.GatePass };
public int DefaultWhacAMoleEnvironmentId = 21;   // existing
public int DefaultGatePassEnvironmentId = 22;    // new — the layout authored in Package F
public WhacAMoleConfig WhacAMole;                // existing
public GatePassConfig GatePass;                  // new
```

New `Simulation/Scripts/Configurations/GatePassConfig.cs`:
```csharp
[System.Serializable]
public class GatePassConfig
{
    public float StageDurationSeconds = 60f;
    public int ScorePerPass = 1;
    public float PassScoreCooldownSeconds = 0.4f; // one player cannot re-score on the same gate faster than this
}
```
Gate *geometry and mass* live in `SharedGamePlayConfig` instead (§6.1), because the client needs them to build the view.

### A.3 Layout pool

`Shared/Scripts/Configs/EnvironmentConfig.cs`:
```csharp
public List<int> AvailableLayoutIndexes;   // DeathMatch pool (unchanged name for asset compatibility)
public List<int> WhacAMoleLayoutIndexes;   // existing
public List<int> GatePassLayoutIndexes;    // new

public List<int> GetLayoutIndexesForStageType(StageType stageType)
{
    switch (stageType)
    {
        case StageType.WhacAMole: return WhacAMoleLayoutIndexes;
        case StageType.GatePass:  return GatePassLayoutIndexes;
        default:                  return AvailableLayoutIndexes;
    }
}
```
Keep it a pure lookup — the "don't repeat until exhausted" bookkeeping stays in `InitStageCommand.GenerateRandomStageId`.

`Simulation/Match/Scripts/MatchModel/MatchDataService.cs` — **add the missing dictionary key** (see §2 gotcha):
```csharp
{ StageType.GatePass, new List<int>() },
```

### A.4 Bonus-stage rotation service

New `Simulation/Match/Scripts/Stage/IBonusStageRotationService.cs` + `BonusStageRotationService.cs`:

```csharp
public interface IBonusStageRotationService
{
    StageType ResolveNextBonusStageType();  // advances the rotation
    void ResetData();                       // called once per match, not per stage
}
```

Behaviour:
- Reads `EnabledBonusStageTypes` from the config, pre-allocating a `List<StageType>` candidate buffer in the constructor (no per-call allocation).
- If `_lastPlayedBonusStageType == StageType.None` → pick a **random** enabled type (`RNG.NextInt(0, count)`).
- Otherwise → pick randomly among the enabled types **excluding** `_lastPlayedBonusStageType`. With exactly two enabled types this degenerates to strict alternation, which is the required behaviour; with three it still never repeats back-to-back.
- If only one type is enabled, return it (and do not deadlock).
- Stores the result in `_lastPlayedBonusStageType` before returning.

Bind it in `Simulation/Match/Scripts/Initiator/ServerMatchInstaller.cs` as a singleton next to `IStageDataService`. Call `ResetData()` from the match entry point (`ServerMatchEntryPointCommand`), **not** from `InitStageCommand` — the rotation must persist across stages within one match.

> **Why a service and not another `static`:** `InitStageCommand._stageNumber` is already a process-global `static` that is not reset per match (a known wart called out in `WhacAMoleStageType.md` §2.5). Do not add a second one. `_stageNumber` itself stays as-is; only the *bonus type choice* moves into the service.

### A.5 `InitStageCommand`

```csharp
private StageType ResolveStageTypeForCurrentStage()
{
    var gamePlayConfig = _gamePlayConfigService.GamePlayConfig;
    var isRotationConfigured = gamePlayConfig.IsBonusStagesEnabled && gamePlayConfig.BonusStageEveryXStages > 0;
    var didReachBonusStage = isRotationConfigured && _stageNumber % gamePlayConfig.BonusStageEveryXStages == 0;

    return didReachBonusStage ? _bonusStageRotationService.ResolveNextBonusStageType() : StageType.DeathMatch;
}
```

Rename `SetupWhacAMoleStageData` → **`SetupBonusStageData`** and make the duration source per-type:
```csharp
private void SetupBonusStageData(StageType stageType)
{
    var simulationState = _matchDataService.SimulationState;
    simulationState.ResetBonusScorePerTeam(_matchDataService.TeamIds);
    simulationState.ResetBonusScoreForAllPlayers();

    if (!stageType.IsBonusStage())
    {
        simulationState.BonusStageEndTick = 0;
        return;
    }

    var gamePlayConfig = _gamePlayConfigService.GamePlayConfig;
    var stageDurationSeconds = gamePlayConfig.PreparationPhaseDuration
        + (stageType == StageType.WhacAMole ? gamePlayConfig.WhacAMole.StageDurationSeconds
                                            : gamePlayConfig.GatePass.StageDurationSeconds);
    var stageDurationTicks = (int)System.MathF.Ceiling(stageDurationSeconds * _networkConfig.TicksPerSeconds);
    simulationState.BonusStageEndTick = _tickService.CurrentTick + stageDurationTicks;
}
```

`GetDefaultEnvironmentLayoutId(stageType)` gains the `GatePass → DefaultGatePassEnvironmentId` branch.

`CreateEnvironmentLayout(...)` gains a `CreateScoreGates(mapSizeMultiplier)` call (implemented in §6.5), after `CreateWalls`.

**Acceptance (A):** With `BonusStageEveryXStages = 3` and both types enabled, stage types cycle `DM, DM, Bonus, DM, DM, Bonus…` where consecutive bonus stages alternate `WhacAMole → GatePass → WhacAMole → …` starting from a random one. GatePass stages draw only from `GatePassLayoutIndexes`. No gate exists yet — the stage is an empty arena with a countdown; that is expected at this checkpoint.

---

## 6. Work Package B — `ScoreGateObstacle` entity

### 6.1 Shared geometry/mass config

`SharedGamePlayConfig` (client needs these to build the view; server needs them to build the body):
```csharp
public UnityEngine.Vector2 ScoreGatePostSize = new UnityEngine.Vector2(1.5f, 1.5f); // one square post, full size
public float ScoreGateGapWidth = 4f;      // clear space between the two posts
public float ScoreGateDensity = 4f;       // "medium mass" — heavier than a frigid block, lighter than a wall
public float ScoreGateRestitution = 0.2f;
public float ScoreGateLinearDamping = 1.5f;   // so a shoved gate drifts and settles instead of sliding forever
public float ScoreGateAngularDamping = 1.5f;  // so a KO spin decays after a couple of turns
```
Tune in the Editor afterwards; flag in the PR that a human must play-test the feel.

### 6.2 State model

New `Shared/Scripts/S2CModels/ScoreGateStateS2C.cs`, modelled on `TalentFrigidBlockStateS2C`:
```csharp
public struct ScoreGateStateS2C : INetSerializable, IEquatable<ushort>
{
    public ushort Id;
    public Vector2 Position;
    public Vector2 Rotation;        // unit facing vector; the gap axis is perpendicular to it
    public ushort LastScoredTeamId; // 0 = never scored; drives the gate tint, and survives rejoin

    // Serialize: (byte)Id, PutVector2Quantized(Position), PutFloat16(Rotation.X/Y), (byte)LastScoredTeamId
}
```

`MatchSimulationStateS2C`:
- `public FixedUnorderedList<ScoreGateStateS2C> ScoreGates;`
- construct with a new `maxScoreGates` parameter threaded from `NetworkConfig.MaxCap.ConcurrentScoreGates` (default `4`) — **update every `new MatchSimulationStateS2C(` call site** (grep it).
- count-prefixed block in `Serialize`/`Deserialize`, next to the `Moles` block.
- helpers `TryGetScoreGateIndexById` / `GetScoreGateById` (copy the mole helper set).
- clear in `ClearObjectStates()`.

`Simulation/Match/Scripts/MatchModel/MatchDataService.cs`: `AddScoreGate(...)` mirroring `AddMole`, with a `_lastScoreGateCreatedId` counter.

### 6.3 Physics body

`Simulation/Scripts/Physics/PhysicsBodyType.cs`:
```csharp
ScoreGate = 22,          // PhysicsBodyType
ScoreGate = 16,          // PhysicsCollisionType
```

`PhysicsCollisionFilters.GetCollisionsCategory`:
```csharp
// The gate is a heavy free body: everything that can shove a wall-like object should be able to shove it,
// and bullets must stop on it instead of flying through the posts.
case PhysicsBodyType.ScoreGate:
    collisionMask = GetCollisionMask(PhysicsCollisionType.PlayerSpaceship)
                    | GetCollisionMask(PhysicsCollisionType.PlayerBullet)
                    | GetCollisionMask(PhysicsCollisionType.PowerUpBall)
                    | GetCollisionMask(PhysicsCollisionType.KOProjectile)
                    | GetCollisionMask(PhysicsCollisionType.FrigidBlock)
                    | GetCollisionMask(PhysicsCollisionType.GrapplingHookProjectile)
                    | GetCollisionMask(PhysicsCollisionType.FishingRodTip)
                    | GetCollisionMask(PhysicsCollisionType.SoulGhost);
    break;
```
and add `| GetCollisionMask(PhysicsCollisionType.ScoreGate)` to the `PlayerSpaceship`, `PlayerBullet`, `PowerUpBall`, `KOProjectile`, `FrigidBlock`, `GrapplingHookProjectile`, `FishingRodTip` and `SoulGhost` cases.

`IPhysicsSimulator` / `PhysicsSimulator` — model on `AddFrigidBlock`:
```csharp
void AddScoreGate(ushort id, Vector2 position, float rotationDegrees, Vector2 postSize, float gapWidth,
                  float density, float restitution, float linearDamping, float angularDamping);
Body GetScoreGate(ushort id);
void RemoveScoreGate(ushort id);
```
Implementation notes:
- `bodyDef.type = BodyType.Dynamic`, `userData = new PhysicsBodyData(id, PhysicsBodyType.ScoreGate)`, linear/angular damping from args.
- **Two** polygon fixtures on the **same body**, so the posts can never drift apart and the whole gate rotates as one rigid piece:
  ```
  var postHalf = postSize * 0.5f;
  var postOffsetX = gapWidth * 0.5f + postHalf.X;
  shapeA.SetAsBox(postHalf.X, postHalf.Y, new Vector2(-postOffsetX, 0f), 0f);
  shapeB.SetAsBox(postHalf.X, postHalf.Y, new Vector2( postOffsetX, 0f), 0f);
  ```
  Both fixtures: `isSensor = false`, same density/restitution, `categoryBits = PhysicsBodyType.ScoreGate.GetCollisionsCategory()`, `maskBits = PhysicsCollisionType.ScoreGate.GetCollisionMask()`.
- **No sensor fixture in the gap.** Pass detection is geometric (§6.4) — see the rationale there.

`StepPhysiscsSimulationCommand.ApplyPhysicsSimulationToMatchModel` — copy body→state each tick, exactly like frigid blocks:
```csharp
for (int i = 0; i < _matchDataService.SimulationState.ScoreGates.Count; i++)
{
    ref var scoreGate = ref _matchDataService.SimulationState.ScoreGates.GetByIndex(i);
    var body = _physicsSimulator.GetScoreGate(scoreGate.Id);
    scoreGate.Position = body.Position;
    scoreGate.Rotation = body.GetAngle().FromAngleRadians();
}
```
Do **not** add a `CopyScoreGateStateToBody` in `CopyDataToSimulation` — the gate is physics-authoritative, like the frigid block.

### 6.4 Talent interactions with the gate — full matrix

The requirement is "the players can interact with the `ScoreGateObstacle` in any way possible … hit it with **any** type of talent". Because almost every talent projectile is a **sensor** (§2), "any type of talent" is *not* something the physics solver gives for free — each talent needs a deliberate decision. This section enumerates **all 17 `TalentType` values** and specifies exactly what must be written for each.

#### 6.4.0 The one shared helper

Every manual push funnels through a single command so the impulse maths exists in exactly one place:

New `Simulation/Match/Scripts/Commands/PushScoreGateCommand.cs`:
```csharp
public class PushScoreGateCommand : BaseCommand, ICommandVoid
{
    // SetScoreGateId(ushort) / SetImpulse(Vector2) / SetWorldContactPoint(Vector2) / SetExtraSpinImpulse(float)
    public void Execute()
    {
        if (!_matchDataService.SimulationState.TryGetScoreGateIndexById(_scoreGateId, out _)) return;

        var body = _physicsSimulator.GetScoreGate(_scoreGateId);
        body.ApplyLinearImpulse(_impulse * body.GetMass(), _worldContactPoint); // off-centre point already induces spin
        if (_extraSpinImpulse != 0f)
        {
            body.ApplyAngularImpulse(_extraSpinImpulse * body.GetInertia());
        }
    }
}
```
Scaling by `GetMass()` / `GetInertia()` means the tuning numbers stay stable when a designer changes `ScoreGateDensity`. Every talent below passes its **own** tuning values from **its own** config block in `TalentsConfig` (e.g. `KOTalentConfig.ScoreGatePushImpulse`) — **never** from `GatePassConfig`. Rationale: it is that talent's feel, and it must be tunable per talent.

`IPhysicsSimulator` needs `Body GetScoreGate(ushort id)` (already added in §6.3) and nothing else.

#### 6.4.1 Matrix

| # | Talent | How it reaches things today | Gate interaction to implement | Where |
|---|---|---|---|---|
| 1 | **Swap** (`SwapField`, sensor) | Swaps caster ↔ nearest enemy by writing `Transform.Position` | **No physical effect on the gate.** But the teleport **must invalidate** both swapped players' tracked previous positions, or the jump is read as a pass (§7.2). | `SwapTalentController` calls `_scoreGatePassTrackerService.InvalidatePreviousPosition(playerId)` for both players |
| 2 | **YearsOfPain** | `RectangleCastByPriority(center, size, angle, teamId, PlayerSpaceship, Mole)` — no contacts at all | Add `ScoreGate` as a **third** priority to the cast; on a gate hit, `PushScoreGateCommand` with impulse along the caster's facing + a spin. Cast priority order: `PlayerSpaceship` → `Mole` → `ScoreGate` (players first so the talent never "wastes" itself on the gate when an enemy is in range). | `YearsOfPainTalentController:112` + `PhysicsSimulator.RectangleCastByPriority` gains a third body-type parameter |
| 4 | **SentryGun** | Fires normal `PlayerBullet`s | **Free** — bullets are solid and already in the gate's mask; `HandleBulletScoreGateCollision` (§6.4.2) covers it. No talent-specific work. | — |
| 5 | **DashPulse** | `TryAddForceToPlayerCommand` on the caster only | **Indirect** — the caster is flung into the gate and the normal player↔gate contact does the work. No talent-specific work. | — |
| 6 | **KO** (`KOProjectile`, **sensor**) | Projectile that spins + pushes the enemy it touches | `HandleKOProjectileScoreGateCollision` → `PushScoreGateCommand` with impulse along the projectile's velocity at the contact point + `ExtraSpinImpulse`, then destroy the projectile exactly as `HandleKOProjectileWallCollision` does. **This is the explicit example in the requirement** ("KO will push it and spin a bit"). | `ProcessCachedCollisionsCommand` + `KOTalentConfig.ScoreGatePushImpulse` / `.ScoreGateSpinImpulse` |
| 7 | **GrapplingHook** (`GrapplingHookProjectile`, **sensor**) | Hook anchors on `Wall`/`FrigidBlock`, then pulls the **caster** toward the anchor | Add `ScoreGate` to `HandleGrapplingHookCollision`'s accepted anchor types, so the hook **anchors on the gate** and reels the caster in — a genuinely new way to fly through your own gate. Because the anchor point is on a *moving* body, the controller must re-read the gate's transform each tick instead of caching a static world point. Optionally also `PushScoreGateCommand` with a small reaction impulse toward the caster (Newton's third law); recommend **on**, at a low value. | `ProcessCachedCollisionsCommand.HandleGrapplingHookCollision` + `GrapplingHookTalentController` anchor tracking |
| 8 | **Umbrella** | `TryAddForceToPlayerCommand` on the caster | **Indirect**, same as DashPulse. No work. | — |
| 9 | **MagneticPull** | `ArcCastByPriority(center, radius, dir, arc, teamId, PlayerSpaceship, Mole)` | Add `ScoreGate` to the cast; on a gate hit, **pull** it toward the caster: `PushScoreGateCommand` with impulse along `(casterPosition - gatePosition).Normalized()`. This is the only talent that can drag the gate *toward* the player, which is a real tactical option (drag the gate somewhere convenient). | `MagneticPullTalentController:111` + third cast priority |
| 10 | **Chicken** (`ChickenEgg`, **static + sensor**) | Lays eggs that break on player contact | **The gate crushes the egg, and the egg spins the gate a bit.** The egg is `Static` and the gate is `Dynamic`, so Box2D **does** pair them (unlike the egg↔mole static-static case that needed `TryBreakChickenEggsOnMolesCommand`). Add `ScoreGate` to the egg's mask, plus `HandleChickenEggScoreGateCollision` that breaks the egg (same removal path as `HandleChickenEggPlayerCollision`) **and** calls `PushScoreGateCommand` with **zero linear impulse and a non-zero `ExtraSpinImpulse`** — a small static egg should visibly twist the gate, not shove it across the arena. Randomise the spin sign (`RNG`) so repeated eggs do not wind the gate up in one direction. Tuning: `ChickenTalentConfig.ScoreGateSpinImpulse`. | `PhysicsCollisionFilters` `ChickenEgg` case + `ProcessCachedCollisionsCommand` + `ChickenTalentConfig` |
| 11 | **WaterGun** | `EllipseCastOnPlayers(...)` — players only, by name and by implementation | Add a gate pass to the stream: either widen it to `EllipseCastByPriority(… PlayerSpaceship, ScoreGate)` or add a second cast. On a gate hit, apply a **per-tick** impulse (`ScoreGatePushForcePerTick * deltaTime`), matching how the talent already pushes enemies continuously rather than in one hit. The water stream slowly nudging the gate across the arena is the flavour to aim for. | `WaterGunTalentController:117` + `PhysicsSimulator` cast |
| 12 | **Headbutt** | Charge → dash; on player contact, spin + push the enemy | Add `HandleHeadbuttScoreGateCollision`: when the caster is mid-dash (`IsDashing`) and contacts the gate, apply a **large** `PushScoreGateCommand` impulse scaled by the charge fraction, plus spin, and end the dash like the enemy-hit path does. Without this the headbutt lands as an ordinary player bump and the charge is wasted — the strongest single-shove tool in the game should read as such. | `ProcessCachedCollisionsCommand.HandleHeadbuttPlayerCollision` sibling + `HeadbuttTalentConfig` |
| 13 | **Rock** | `EnableRockBody` — enlarges the **caster's own** body and raises its density/restitution | **Mostly free** — the rock body is solid, so the solver pushes the gate harder automatically. Two required touch-ups: (a) `HandlePlayerRockCollision` must gain a `ScoreGate` branch mirroring its `FrigidBlock` branch so the *caster's* velocity reflects correctly; (b) verify the rock's raised density actually reaches the gate (it does — density is set on the fixture in `EnableRockBody`, and the solver reads it). | `ProcessCachedCollisionsCommand.HandlePlayerRockCollision` |
| 14 | **FrigidBlock** | Shoots a **solid** dynamic block with real mass | **Free** — both are solid dynamic bodies and each is already in the other's mask (§6.3). The solver does the whole job; a shot block visibly knocks the gate aside. Only verify the block's `IsIdleLongEnoughToBeDestroyed` logic does not fire early while it rests against a slowly-drifting gate. | verify only |
| 15 | **FishingRod** (`FishingRodTip`, **sensor**) | Tip flies out; on `Wall` it retracts, on an enemy it catches and reels them in | **The gate is treated exactly as a wall.** Add `ScoreGate` to `HandleFishingRodTipWallCollision` so the tip retracts instead of passing through — same net event, same retract path, no special casing. The tip is a sensor, so it applies **no** impulse and the gate does not move; that is the intended wall-like behaviour. Do **not** build the *catch-and-reel-the-gate* variant: it would need `FishingRodCaughtEnemyType` to gain a non-player case, the reel logic to drive a Box2D body instead of a `PlayerStateS2C`, and new client art for a gate-on-a-line. | `ProcessCachedCollisionsCommand.HandleFishingRodTipWallCollision` |
| 16 | **Soul** (`SoulGhost`, **sensor**) | Ghost flies until it hits `Wall`/`FrigidBlock`, then the caster respawns at the ghost | Add `ScoreGate` to `HandleSoulGhostWallCollision` so the ghost stops on the gate rather than sailing through it. **No impulse** — the ghost is intangible by design. ⚠️ The soul respawn also moves the caster by writing his position, so it **must invalidate the tracked previous position** exactly like Swap (§7.2). | `ProcessCachedCollisionsCommand` + `SoulTalentController` invalidation |
| 17 | **Frozen** | Caster becomes immobile; `HandlePlayerFrigidBlockCollision` has a frozen special case | Mirror that special case in `HandlePlayerScoreGateCollision`: a frozen player shoved by the gate must behave the same as one shoved by a frigid block. A frozen player is also a heavy obstacle the gate can push around — that falls out of the solver for free. | `ProcessCachedCollisionsCommand.HandlePlayerScoreGateCollision` |
| — | **Bomb / Hammer** | Empty controller stubs, no `TalentType` value | **N/A.** If either is ever implemented, add it to this matrix. | — |

> **Cast-signature note:** three talents (YearsOfPain, MagneticPull, WaterGun) need one more body type in their cast. `ArcCastByPriority` and `RectangleCastByPriority` currently take exactly `firstPriorityBodyType` + `secondPriorityBodyType`. Rather than growing them to three positional parameters — and then four for the next mode — change the signature to take a **pre-allocated priority array** owned by the calling controller (`PhysicsBodyType[] priorityBodyTypes`), allocated once in `ResolveDependencies()`. That keeps zero-alloc, stops the parameter list from growing per stage type, and is the smaller diff overall. `EllipseCastOnPlayers` gets the same treatment and is renamed `EllipseCastByPriority`.

#### 6.4.2 Non-talent handlers in `ProcessCachedCollisionsCommand`

Add to the `Begin` block, next to the frigid-block handlers:

1. **`HandlePlayerScoreGateCollision(objectA, objectB, contact)`** — a near copy of `HandlePlayerFrigidBlockCollision`. Reflect the player's velocity off the manifold normal (the player body's velocity is overwritten from state each tick, so the solver's own response to the *player* is discarded). The **gate** is pushed by the solver naturally — both bodies are solid — so no manual impulse here. Keep the frozen-player special case (§6.4.1 #17).
2. **`HandleBulletScoreGateCollision(objectA, objectB, contact)`** — destroy the bullet on impact (copy `HandleBulletFrigidBlockCollision`). Bullets are solid, so they also nudge the gate; that is desirable and covers SentryGun for free.
3. **`HandlePowerUpBallScoreGateCollision(objectA, objectB, contact)`** — copy `HandlePowerUpBallFrigidBlockCollision` so power-up balls bounce off the posts instead of resting inside them.

All the talent-specific handlers from the matrix are added in this same block, and every one of them must tolerate the "gate already gone this tick" case via `TryGetScoreGateIndexById` (harmless today since gates are never removed mid-stage, but it keeps the handlers uniform with the mole ones).

### 6.5 Spawning the gate from the layout

`EnvironmentLayoutConfig`: add `[TextArea] private string _scoreGatesJson;` + `GetScoreGates()` / `SetScoreGatesJson(...)`, following `GetMoleSpawnPoints` exactly (return `default` on empty string).

New `Shared/Scripts/S2CModels/ScoreGateConfig.cs`:
```csharp
[Serializable]
public class ScoreGateConfig
{
    public ushort Id;
    public Vector2 Position;
    public float RotationDegrees;
}
```

`EnvironmentConfig.SetScoreGates(ScoreGateConfig[] scoreGates, int index)` — copy `SetMoleSpawnPoints`.

`IMatchEnvironmentConfigDataService` / `MatchEnvironmentConfigDataService`: add `ScoreGateConfig[] ScoreGates { get; }`, populated in `InitEnvironmentLayout`.

`InitStageCommand.CreateScoreGates(float mapSizeMultiplier)`:
```csharp
private void CreateScoreGates(float mapSizeMultiplier)
{
    var scoreGateConfigs = _matchEnvironmentConfigDataService.ScoreGates;
    if (scoreGateConfigs.IsNullOrEmpty())
    {
        return;
    }

    var postSize = _sharedGamePlayConfig.ScoreGatePostSize.ToNumericsVector2() * mapSizeMultiplier;
    var gapWidth = _sharedGamePlayConfig.ScoreGateGapWidth * mapSizeMultiplier;

    foreach (var scoreGateConfig in scoreGateConfigs)
    {
        var position = scoreGateConfig.Position * mapSizeMultiplier;
        _matchDataService.AddScoreGate(scoreGateConfig.Id, position, scoreGateConfig.RotationDegrees);
        _physicsSimulator.AddScoreGate(scoreGateConfig.Id, position, scoreGateConfig.RotationDegrees, postSize, gapWidth,
            _sharedGamePlayConfig.ScoreGateDensity, _sharedGamePlayConfig.ScoreGateRestitution,
            _sharedGamePlayConfig.ScoreGateLinearDamping, _sharedGamePlayConfig.ScoreGateAngularDamping);
    }
}
```
It self-gates: only GatePass layouts author `_scoreGatesJson`, so nothing special is needed for other stage types. `RestartStageData()` already calls `_physicsSimulator.ClearAllData()` and `ClearObjectStates()`, so gates are torn down between stages for free — **verify** `ClearObjectStates` clears `ScoreGates`.

**Acceptance (B):** In a forced GatePass stage the gate is visible in the Box2D debug draw and **every row of the §6.4.1 matrix has been walked manually**: ramming shoves it, Rock shoves it harder, Headbutt launches it, KO pushes + spins it, MagneticPull drags it toward the caster, YearsOfPain and WaterGun push it, a shot FrigidBlock knocks it aside, bullets and SentryGun fire stop on it, the grappling hook anchors on it, the fishing-rod tip retracts off it, the soul ghost stops on it, power-up balls bounce off it. Nothing scores yet.

---

## 7. Work Package C — Pass detection, scoring, net event

### 7.1 Detection method — geometric segment crossing (chosen)

Each tick, for each gate, build the **gap segment** in world space from the gate's current transform:
```
right   = gate.Rotation                       // unit vector along the post axis
P0      = gate.Position - right * (gapWidth * 0.5f)
P1      = gate.Position + right * (gapWidth * 0.5f)
```
For each alive player, take the segment `[previousPosition, currentPosition]` (previous position stored by the tracker from last tick) and test **proper segment intersection** against `[P0, P1]`. An intersection means the player crossed the plane of the gate *within the gap* during this tick → that is a pass, in either direction.

> **Why not a sensor fixture in the gap?** A third `isSensor` fixture on the gate body was the obvious alternative, but: (a) sensor contacts are discrete, so a fast player can tunnel across the gap between two 60 Hz steps and score nothing, while the segment test catches exactly that case; (b) scoring on `BeginContact` alone would fire when a player merely *touches* the gap and backs out, so it would need entry-side bookkeeping plus an `EndContact` pairing; (c) it costs an extra fixture and an extra `PhysicsCollisionType` bit (only 31 exist). The geometric test is a few dozen float ops per player per gate and is deterministic.
>
> Known limitation to accept: the test uses the gate's *post-step* transform for both endpoints, so a gate that is itself moving fast while a player crosses it is approximated. At 60 Hz with a damped heavy gate this is not observable.

**Cooldown:** after a player scores on a gate, ignore that player↔gate pair for `GatePassConfig.PassScoreCooldownSeconds`. This stops a player wedged in the gap from farming points on physics jitter.

### 7.2 Tracker service

New `Simulation/Match/Scripts/ScoreGate/IScoreGatePassTrackerService.cs` + impl:
- `CapacityDict<ushort, Vector2> PreviousPositionPerPlayerId` and a `CapacityDict<ushort, int>`-of-`CapacityDict` (or a flat `CapacityDict<int, int>` keyed by `playerId << 16 | gateId`) holding **the tick the cooldown expires** per player-gate pair. Size everything from `MaxCap.ConcurrentPlayers * MaxCap.ConcurrentScoreGates` and allocate in `InitEntryPoint()` — **zero allocation per tick.**
- `void ClearAllData()` called from `InitStageCommand.RestartStageData()` (alongside `_playersInLavaTrackerService.ClearAllData()`), so a player standing where a gate used to be does not score on the first tick of the next stage.
- **`void InvalidatePreviousPosition(ushort playerId)`** — drops the stored previous position so the next tick seeds a fresh one and no crossing is tested for that player this tick.

⚠️ **Teleport invalidation is mandatory, not optional.** The segment test assumes a player's position moves continuously. Three code paths break that assumption by writing `Transform.Position` directly, and each would hand out a **free point** whenever the jump happens to straddle the gate line — even from across the arena:

| Path | File | Fix |
|---|---|---|
| Swap talent swaps caster ↔ enemy | `SwapTalentController.cs:157` | `InvalidatePreviousPosition` for **both** players |
| Teleport gate moves a player to the paired gate | `ProcessCachedCollisionsCommand.cs:814` | `InvalidatePreviousPosition(playerId)` |
| Soul talent respawns the caster at the ghost | `SoulTalentController` (deactivate path) | `InvalidatePreviousPosition(casterPlayerId)` |

The recommended rectangular layout (§10) authors no teleport gates, so only Swap and Soul can fire in practice — but the hook must exist, because a future GatePass layout with teleport gates would otherwise be quietly exploitable. Grep for `Spaceship.Transform.Position =` before finishing this package and confirm every assignment outside the physics copy-back is covered.

### 7.3 `TryScoreGatePassesCommand`

New `Simulation/Match/Scripts/Commands/TryScoreGatePassesCommand.cs`:
- Early-return unless `StageType == GatePass && !IsInPreparationPhase && !IsStageEnded`.
- Loop gates (outer) × alive players (inner), run the segment test + cooldown check, and on a hit call the scoring routine below. Then, unconditionally, refresh `PreviousPositionPerPlayerId` for every player — **including during the preparation phase**, so the first live tick has a sane previous position.
- Registered in `ServerMatchNetworkTickProcessor.InitEntryPoint()` and called in `OnTick` **after** `_stepPhysiscsSimulationCommand` (positions must be post-step) and before `_trySendPlayersLockOnTargetChangedCommand`.

Scoring routine (inline private method or a small `ScoreGatePassCommand`):
```
score            = config.GatePass.ScorePerPass
teamTotal        = simulationState.AddBonusScoreForTeam(player.TeamId, score)   // returns the new total
playerTotal      = simulationState.AddBonusScoreForPlayer(player.Id, score)
gate.LastScoredTeamId = player.TeamId
netEventsDataService.AddScoreGatePassedNetEvent(tick, gateId, player.Id, player.TeamId, (byte)score, teamTotal, playerTotal)
```
(`AddBonusScoreForTeam` currently returns `void`; either make it return the new total like `AddBonusScoreForPlayer` does, or read `BonusScorePerTeamId[teamId]` right after — match whatever Package 0 left in place.)

### 7.4 Net event

Use the **`/add-net-event`** skill. One event, struct:

```csharp
public struct ScoreGatePassedNetEventS2C : INetSerializable, IComparable<ScoreGatePassedNetEventS2C>
{
    public int OccuredOnTick;
    public ushort ScoreGateId;
    public ushort ByPlayerId;
    public ushort ByTeamId;
    public byte ScoreGained;
    public int TeamBonusScoreTotal;
    public int ByPlayerBonusScoreTotal;
}
```
Serialize ids as `byte` (matching `MoleHitNetEventS2C`). The popup position is **not** sent — the client reads the gate's current position from its own model, which is more accurate at render time than a tick-old position.

Touch-points (all seven, per the skill):
1. the event struct above;
2. `NetworkConfig.MaxCap.ScoreGatePassedNetEvents = 64;` plus `ConcurrentScoreGates = 4;`
3. `MatchFullTickPacketS2C`: field, constructor init, **`eventMask2` bit 12** in `CalculateEventMask2()`, `Serialize`, `Deserialize` (+ `else …Clear()`), and the private `Serialized…`/`Deserialized…` pair;
4. `INetEventsDataService`: `…PerClient` property + `AddScoreGatePassedNetEvent(...)`;
5. `NetEventsDataService`: property, pool field, constructor init, `StartSavingClientEvents`, **`StopSavingClientEvents` (clear → return to pool → `Remove(clientId)`)**, the `Add…` method;
6. `ServerMatchNetworkTickProcessor.SendCurrentTickStateToAllClients`: `_fullTickPacket.ScoreGatePassedNetEvents = _netEventsDataService.ScoreGatePassedNetEventsPerClient[clientId];`
7. `MatchFullTickPacketsHandler`: `_cachedUnprocessedScoreGatePassedEvents` field + init + tick filter/sort + dispatch to `_presentationNetEventsHandler.ProcessScoreGatePassedEvents(...)`; and `ICachedPresentationEventsService`/`CachedPresentationEventsService` gain `List<ScoreGatePassedNetEventS2C> ScoreGatePassedNetEvents`.

**Acceptance (C):** Server logs one score per gap traversal, in both directions; brushing the posts scores nothing; sitting in the gap scores at most once per `PassScoreCooldownSeconds`; a player boosted through the gap at maximum speed still scores.

---

## 8. Work Package D — Stage end, gems, mode branching

Most of this is already done by Package 0's generalization. Remaining:

- **`TryEndBonusStageCommand`** already fires on `BonusStageEndTick` for any bonus stage and already awards gems by rank from `BonusScorePerTeamId` — verify the GatePass path end-to-end and that `HideAllMoles()` is skipped for GatePass.
- **`TryHitPlayerCommand:92`** — change `if (_stageDataService.IsWhacAMoleStage)` to `if (_stageDataService.IsBonusStage)`. Players must be invulnerable in GatePass too (their UI slot shows score, not health).
- **`TrySendPlayersLockOnTargetChangedCommand:70`** — three-way:
  ```csharp
  if (_stageDataService.IsWhacAMoleStage)      FindTargetedMolesOfCaster(...);
  else if (!_stageDataService.IsBonusStage)    FindTargetedEnemyIdsOfCaster(...);
  // GatePass: no enemy lock-on — enemies cannot be damaged, so a lock-on reticle on them would lie.
  FindTargetedPowerUpBallsOfCaster(...);       // always
  ```
- **Winner / ties** — unchanged from Whac-A-Mole: `GetLowestTeamIdWithBonusScore` breaks ties deterministically for the single-winner end event, while `AwardGemsByRank` gives tied teams equal gems. No new behaviour, no new decision.
- **`StageEndedCommand.GetPlayerToFocusOn`** — in GatePass everyone is alive, so it resolves to a winning-team player; optionally pass the last scorer as `PlayerIdDoingWinningBlow` from `TryEndBonusStageCommand` if you track it. Nice-to-have, not required.

**Acceptance (D):** A GatePass stage ends exactly when the countdown hits zero, the highest-scoring team is shown as winner, gems match the Whac-A-Mole rank rule (one gem per team strictly outscored), and the match rotates into the next stage.

---

## 9. Work Package E — Presentation

### E.1 Gate MVC

New feature folder `Presentation/Match/Features/ScoreGate/Scripts/Mvc/`, modelled on `Features/FrigidBlock/Scripts/Mvc/`:

- `ScoreGateView : MonoBehaviour` — two post `SpriteRenderer`s (or one prefab child per post) plus whatever frame/glow art the gate gets. Exposes `SetTeamColor(Color color)` which tints the designated renderers, and nothing else. **Dumb view: no logic, no controller references.**
- `ScoreGateController` — pure C#: `CreateView(position, rotation)`, `InterpolateTransform(position, rotation, exponentialDecay)` (copy `FrigidBlockController`), `SetTeamColor(color)`, `Destroy()`.
- `ScoreGatesControllers : IScoreGatesControllers` — `Dictionary<ushort, ScoreGateController>` + `ScoreGatePool`, `InitEntryPoint/InitExitPoint/DestroyAll`, and `bool TryGetScoreGatePosition(ushort id, out Vector2 position)` for the popup anchor.
- The post size/gap for the view come from `SharedGamePlayConfig` × `MapSizeMultiplier`, so the visual always matches the collider (the frigid block does exactly this via `CreateSharedBlockMeshFromColliderSize`).
- Bind in `GamePlayMatchInstaller`: `[SerializeField] private ScoreGateView _scoreGateViewPrefab;` → `Container.BindInterfacesTo<ScoreGatesControllers>().AsSingle().WithArguments(_scoreGateViewPrefab).NonLazy();`
- `InitEntryPoint` / `InitExitPoint` from `StartGamePlayMatchCommand` alongside `_moleControllers`.

### E.2 Client model + per-frame transform

- `Presentation/Match/Scripts/Models/MatchScoreGateModel.cs`: `Id`, `Position`, `Rotation`, `LastScoredTeamId`.
- `IMatchDataService` / `MatchDataService`: `List<MatchScoreGateModel> ScoreGates`, `AddScoreGate`, `RemoveScoreGate`, `GetScoreGate`, cleared in `ClearAll()`.
- `MatchFullTickPacketsHandler`: add `UpdateScoreGatesTransform(simulationState)` next to `UpdateFrigidBlocksTransform(simulationState)` — copies `Position`/`Rotation` from the tick state into the models.
- New `UpdateScoreGatesTransformCommand` (copy `UpdateFrigidBlocksTransformCommand`), registered in `ClientMatchPresentationTickProcessor.ManagedUpdate()` next to the frigid-block one, calling `InterpolateScoreGateTransform` per model.

### E.3 The pass event on the client

**`PresentationMatchNetEventsHandler.ProcessScoreGatePassedEvents`** (receive time — this is where model writes belong):
```csharp
foreach (var netEvent in scoreGatePassedNetEvents)
{
    _matchDataService.SetTeamBonusScore(netEvent.ByTeamId, netEvent.TeamBonusScoreTotal);
    _matchDataService.SetPlayerBonusScore(netEvent.ByPlayerId, netEvent.ByPlayerBonusScoreTotal);
    _matchDataService.GetScoreGate(netEvent.ScoreGateId).LastScoredTeamId = netEvent.ByTeamId;
    _cachedPresentationEventsService.ScoreGatePassedNetEvents.Add(netEvent);
}
```

**`HandleScoreGatePassedNetEventsCommand`** (`ManagedUpdate` time — read-only on the model):
```csharp
foreach (var netEvent in events)
{
    if (_scoreGatesControllers.TryGetScoreGatePosition(netEvent.ScoreGateId, out var gatePosition))
    {
        _bonusScoreEffectController.PlayEffect(netEvent.ScoreGained, gatePosition); // the "+1" pops from the gate
    }

    if (_presentationGamePlayConfig.ColorPerTeamId.TryGetValue(netEvent.ByTeamId, out var teamColor))
    {
        _scoreGatesControllers.SetTeamColor(netEvent.ScoreGateId, teamColor);
    }

    _teamsBoardUIController.UpdateTeamBonusScore(netEvent.ByTeamId, netEvent.TeamBonusScoreTotal);
    _playerUIControllers.UpdatePlayerBonusScore(netEvent.ByPlayerId, netEvent.ByPlayerBonusScoreTotal);
}
_audioService.PlayAudio(AudioClipType.ScoreGatePassed);
events.Clear();
```
Register the command in `ClientMatchPresentationTickProcessor` next to `_handleMoleHitNetEventsCommand`. Add `AudioClipType.ScoreGatePassed` and assign a clip on `CoreAudioClips.asset`.

### E.4 HUD

- **`MatchPlayerUIControllers.AddPlayer`** — the `switch (_matchDataService.StageType)` gains `case StageType.GatePass:` falling in with `WhacAMole` (`ShowBonusScore(...)`, health bar hidden).
- **`UpdateBonusStageCountdownCommand`** — condition becomes `_matchDataService.StageType.IsBonusStage() && !_matchDataService.IsInPreparationPhase`, reading `BonusStageEndTick`. No other change.
- **Team board** — `SetIsBonusScoreShown(stageType.IsBonusStage())`.

### E.5 Rejoin / full-state sync

`SyncMatchSimulationStateCommand`:
- `DestroyAll()` → `_scoreGatesControllers.DestroyAll();`
- `CreateAll()` → `CreateScoreGates(mapSizeMultiplier)`: for each `ScoreGateStateS2C` in the snapshot, `_matchDataService.AddScoreGate(...)`, `_scoreGatesControllers.CreateScoreGate(id, position, rotation)`, and if `LastScoredTeamId != 0` apply that team's colour immediately — this is why `LastScoredTeamId` is part of the serialized state and not just an event side effect.
- `SetupWhacAMoleHud` → `SetupBonusStageHud`, gated on `IsBonusStage()`; the mole-specific `CreateMoles` stays gated on `StageType == WhacAMole`.

**Acceptance (E):** Client shows the gate drifting/rotating smoothly as it is shoved, `+1` pops from the gate on each pass, the gate takes the scoring team's colour, top-middle board and the passing player's UI both update, countdown runs, and a client that rejoins mid-stage sees the gate at the right place with the right colour and the right scores.

---

## 10. Work Package F — Stage authoring (the rectangular GatePass layout)

### F.1 Authoring component + button

New `Shared/LevelEnvironment/Scripts/ScoreGateSpawnPoint.cs` (mirrors `MoleSpawnPoint`):
```csharp
public class ScoreGateSpawnPoint : MonoBehaviour
{
    public ushort Id;
#if UNITY_EDITOR
    private void OnDrawGizmos()  // draw the two posts and the gap from SharedGamePlayConfig so the author sees the real footprint
#endif
}
```

`EnvironmentGenerator`:
```csharp
[SerializeField] private List<ScoreGateSpawnPoint> _scoreGates;

[Button]
public void RefreshScoreGates(int index)
{
    var configs = new ScoreGateConfig[_scoreGates.Count];
    for (int i = 0; i < _scoreGates.Count; i++)
    {
        configs[i] = new ScoreGateConfig(_scoreGates[i].Id,
            _scoreGates[i].transform.position.ToVector2XY().ToNumericsVector2(),
            _scoreGates[i].transform.eulerAngles.z);
    }
    _environmentConfig.SetScoreGates(configs, index);
}
```

### F.2 The layout itself

Author layout index **22** (or the next free index — check `EnvironmentConfig._environmentLayoutConfigs` in the inspector) on the `Environment` prefab (`Shared/LevelEnvironment/Assets/Environment.prefab`), then fill the layout's JSON fields:

- `_environmentHalfSizeJson` — half extents of the rectangle.
- `_wallsJson` — four `PolygonPath2D` walls forming the rectangle border.
- `_stageBoundriesWallsJson` — the same rectangle (used by `EnforceStageBarriersCommand` / outside-stage tracking).
- `_fieldBarriersJson` — one barrier per team; `InitStageCommand.SetupPlayers` spawns each team at its barrier position, so **there must be at least `MaxTeamsAmount` barriers** or teams share a spawn.
- `_cameraBoundariesJson` — via the existing `SaveCameraBoundaries(index)` button.
- `_scoreGatesJson` — **one gate, id `1`, at the arena centre, rotation `0`** via the new `RefreshScoreGates(index)` button.
- Leave `_moleSpawnPointsJson`, `_lavaWallsJson`, `_environmentSpikesJson`, `_teleportGatesJson`, `_rotatingWheelsJson` empty. **No lava and no spikes** — they call `TryHitPlayerCommand`, which is a no-op in bonus stages, but authoring them would still be misleading.
- Talent cards / power-up spawn points: optional. Recommend authoring a few power-up spawn points so the mode has some chaos; talents still work.

Then in the config assets: add `22` to `EnvironmentConfig.GatePassLayoutIndexes` and set `SimulationGamePlayInnerConfig.DefaultGatePassEnvironmentId = 22`.

**Acceptance (F):** With `ShouldChooseRandomStage` on or off, a GatePass stage loads the rectangular arena, every team spawns inside it, the camera frames it, and exactly one gate stands at the centre.

---

## 11. Cross-cutting checklist / gotchas

- [ ] **`/coding-guidlines` followed** — zero-alloc, commands, `Get`≠mutate, `Try` prefixes, unit-suffixed time fields, no magic numbers, method ≤30 / class ≤200 lines.
- [ ] **`MatchDataService._didntPlayYetStageIndexesPerStageType` has a `GatePass` entry** — otherwise the first random GatePass layout roll throws `KeyNotFoundException`.
- [ ] **Every `new MatchSimulationStateS2C(` call site updated** for the `maxScoreGates` argument (grep it).
- [ ] **`ClearObjectStates()` clears `ScoreGates`** — a DeathMatch stage after a GatePass stage must have zero gate bodies and zero gate views.
- [ ] **`ScoreGatePassTrackerService.ClearAllData()` is called from `InitStageCommand.RestartStageData()`** — stale previous positions across a stage boundary would score phantom passes.
- [ ] **Net event uses `eventMask2` bit 12** (bits 0–11 taken). Same bit in `CalculateEventMask2`, `Serialize`, `Deserialize`.
- [ ] **`StopSavingClientEvents` clears, pools and `Remove(clientId)`s** the new per-client list — a missing `Remove` leaks the dictionary entry.
- [ ] **Every row of the §6.4.1 talent matrix is implemented or explicitly marked "free/N-A"** — sensors (KO, GrapplingHook, FishingRodTip, SoulGhost, ChickenEgg, SwapField) produce **no** solver impulse, and cast-based talents (MagneticPull, YearsOfPain, WaterGun) produce **no collision event at all**. Skipping any of them silently leaves "hit it with any type of talent" unmet for that talent.
- [ ] **All gate impulses go through `PushScoreGateCommand`** and scale by `body.GetMass()` / `GetInertia()`, so re-tuning `ScoreGateDensity` does not invalidate every talent's numbers.
- [ ] **Per-talent gate tuning lives in that talent's own config block** in `TalentsConfig`, never in `GatePassConfig`.
- [ ] **`InvalidatePreviousPosition` is wired to all three teleport paths** (Swap, teleport gate, Soul respawn) — otherwise a teleport that straddles the gate line awards a free point (§7.2).
- [ ] **Cast signatures changed to a pre-allocated priority array** rather than a third positional parameter, and the arrays are allocated once in `ResolveDependencies()` (zero-alloc).
- [ ] **Chicken egg spins the gate** — egg breaks, gate gets a spin-only impulse (zero linear, randomised sign). The egg is static and the gate is dynamic, so they *do* pair; no manual overlap scan needed.
- [ ] **Fishing rod tip treats the gate as a wall** — retract only, no impulse, no catch-and-reel.
- [ ] **Player velocity is state-authoritative** — the player↔gate collision must reflect the player's velocity in state (copy the frigid-block handler), or players will slide into the posts.
- [ ] **`FormerlySerializedAs` on every renamed serialized field** in Package 0, and `.cs` + `.meta` moved together for every renamed MonoBehaviour, or prefab references break silently.
- [ ] **`PhysicsCollisionType` has 31 usable bits** — `ScoreGate = 16` is fine, but do not add speculative extra channels.
- [ ] **Presentation write/read split** — gate model mutations in `PresentationMatchNetEventsHandler`, never in `HandleScoreGatePassedNetEventsCommand`.
- [ ] **Config assets need a human pass**: `SimulationGamePlayConfig.asset` (bonus rotation, `GatePassConfig`, default layout id), `SharedGamePlayConfig.asset` (gate size/gap/mass/damping), `EnvironmentConfig` (`GatePassLayoutIndexes` + the layout), `CoreAudioClips.asset` (`ScoreGatePassed`), `GamePlayMatchScene` (gate prefab binding).
- [ ] **`_stageNumber` is still a process-global `static`** — the bonus *rotation* is now per-match (`IBonusStageRotationService`), but the "is this a bonus stage" counter is not. Raise it if per-match reset is required.
- [ ] **Gate mass feel** ("medium mass", "push it slowly") is a play-test decision — ship with the defaults and flag it.

---

## 12. Suggested manual test plan

1. Set `BonusStageEveryXStages = 1` temporarily → every stage is a bonus stage. Verify the type strictly alternates `WhacAMole ↔ GatePass` and that the first one is not always the same across restarts.
2. In GatePass: fly through the gap → team score +1, `+1` pops from the gate, gate turns your team colour, top-middle board and your own UI both update.
3. Fly through backwards → also scores. Brush a post → no score. Park in the gap → at most one score per `PassScoreCooldownSeconds`.
4. **Walk the whole §6.4.1 matrix with `ShouldChooseRandomTalentsForPlayer` off and each talent forced in turn:**
   - Ram it → drifts slowly, you bounce off. As **Rock** → noticeably harder shove. **Frozen** while the gate hits you → same behaviour as a frigid block hitting you.
   - **KO** → pushed and spins, then settles. **Headbutt** (full charge) → the single biggest shove in the game. **DashPulse** / **Umbrella** → indirect shove via your own speed.
   - **MagneticPull** → the gate is dragged *toward* you. **YearsOfPain** → pushed away. **WaterGun** → nudged continuously while the stream is on.
   - **FrigidBlock** → a shot block knocks the gate aside and the block does not despawn early while resting against it. **SentryGun**/normal fire → bullets stop on the posts.
   - **GrapplingHook** → anchors on the gate and reels you in, and the anchor tracks the gate as it moves. **FishingRod** → tip retracts off the gate exactly as it does off a wall, and the gate does **not** move. **Soul** → ghost stops on it.
   - **Chicken** → an egg the gate rolls over breaks **and twists the gate a little** without shoving it sideways; several eggs in a row do not wind it up in one direction.
5. **Teleport exploit check:** stand on one side of the gate and **Swap** with a player on the other side → **no point is awarded**. Same for a **Soul** respawn across the gate line, and for a teleport gate if the layout has one.
6. Let the countdown expire → highest score wins, gems awarded by rank, ties give equal gems, match rotates into the next stage.
7. Two players from different teams alternate passes → the gate colour follows the last scorer.
8. Rejoin mid GatePass stage → gate position/rotation/colour, both score displays and the remaining time are all correct.
9. Regression: a DeathMatch stage still ends by elimination, has no gate, no countdown, health bars back; a Whac-A-Mole stage is unchanged from before Package 0.
