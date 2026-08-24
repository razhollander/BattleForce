# FreeMarket StageType — Implementation Spec

> **Audience:** agents/engineers implementing this feature.
> **Working directory for all code paths below:** `Assets/`
> Read `Assets/CLAUDE.md` first for the architecture overview, then `WhacAMoleStageType.md` and `GatePassStageType.md` — FreeMarket reuses the stage plumbing both of those built (timer, per-stage-type layout pool, "no damage in this mode" guards, a dynamic pushable non-player body). Both are **already implemented and merged**; this doc assumes their code as it exists today, not as their specs described it.

---

## 1. Feature summary

1. Add **`StageType.FreeMarket = 4`** — a shopping stage, not a scoring stage. No score, no gems, no winner.
2. **Countdown timer**, exactly like Whac-A-Mole / GatePass (same `WhacAMoleEndTick` field, same client widget). When it hits zero the stage ends and the match rotates on.
3. **TalentCards scattered around** the arena (authored per layout, as they already are for DeathMatch layouts) — but in FreeMarket a card is a *purchase*, not an obstacle:
   - Cards become **lock-on targetable** (`LockOnTargetType.TalentCard`).
   - **One hit = the card's talent is granted** to the shooting player (no card health chip-away).
   - After a player obtains a talent, **he cannot shoot again for the rest of the stage** — one purchase per player per FreeMarket stage. **The shot is spent even when the purchase costs the player a talent** (buying at max talents replaces the currently selected one).
   - **Edge case:** a player who hits two cards with a single shot (possible because lock-on shooting fires at *every* shootable locked target in the same tick) gets the talent of the card **closest to him**; the other card is untouched.
4. **TrainingPuppets** — 3 scattered dummy targets so a player can practise the talent he just bought:
   - Lock-on targetable and shootable.
   - Hit by **any** talent, exactly as a player is.
   - A dynamic physics body that can be **pushed and spinned**, with a **configurable mass strictly greater than a player's**.
   - **Indestructible, with a decorative health bar**: 100 health that drops as they take damage and simply stays at 0 — nothing happens when it empties. Hits show the normal hit indicator.
5. **Simulation config:** an enable flag plus the **explicit stage numbers** FreeMarket happens on (e.g. `1, 5, 10`).
6. Author **one FreeMarket layout** (cards + puppet spawn points) in its own environment-layout pool.

Everything runs inside the existing server-authoritative 60 tick/s simulation. The server owns every decision; the client renders state + net events.

---

## 2. Grounding — what already exists and is reused

| Concern | File |
|---|---|
| Stage type enum + `IsBonusStage()` | `Shared/Scripts/Enums/StageType.cs`, `StageTypeExtensions.cs` |
| Stage selection, per-stage-type default layout, layout pools | `Simulation/Match/Scripts/Commands/InitStageCommand.cs` (`ResolveStageTypeForCurrentStage`, `SetupBonusStageData`, `GetDefaultEnvironmentLayoutId`, `GenerateRandomStageId`) |
| Stage counter (1-based, survives `ClearData`) | `Simulation/Match/Scripts/Stage/StageDataService.cs` (`AmountOfStagesEntered`, incremented at the top of `InitStageCommand.Execute`) |
| Per-stage-type layout pool | `Shared/Scripts/Configs/EnvironmentConfig.cs` (`GetLayoutIndexesForStageType`), `Simulation/Match/Scripts/MatchModel/MatchDataService.cs` (`_didntPlayYetStageIndexesPerStageType`) |
| **TalentCards already exist end to end** — state, physics body, spawn from layout, bullet hit, talent grant, client MVC | `Shared/Scripts/S2CModels/TalentCardS2C.cs`, `TalentCardConfig.cs`, `InitStageCommand.CreateTalentCards`, `PhysicsSimulator.AddTalentCard`, `ProcessCachedCollisionsCommand.HandlePlayerBulletTalentCardCollision`, `Presentation/Match/Features/TalentCards/` |
| Talent grant + replacement rules | `Simulation/Match/Scripts/Talent/PlayersTalentsManager.TryAddTalentToPlayer` |
| Card obtained / card hit net events | `Shared/Scripts/S2CModels/PacketEvents/NetEvents/TalentCardObtainedNetEventS2C.cs`, `TalentCardHitNetEventS2C.cs` |
| Lock-on targeting (cone + raycast + per-target arming timer) | `Simulation/Match/Scripts/PlayerLockOnTarget/TrySendPlayersLockOnTargetChangedCommand.cs`, `LockOnTargetTimerService.cs`, `Shared/Scripts/S2CModels/LockOnTargetType.cs`, `ObjectLockedOnTargetS2C.cs` |
| Lock-on shooting dispatch (one branch per target type) | `Simulation/Match/Scripts/Commands/TryShootLockedOnTargetsCommand.cs` |
| Bullet shooting + shoot cooldown | `Simulation/Match/Scripts/Commands/TryPerformShootForPlayerIfNotOnCooldownCommand.cs`, driven from `MatchPlayerInputsPacketsHandler.UpdatePlayerShoot` |
| **Closest analog for the TrainingPuppet body** (dynamic, mass/damping configured, physics-authoritative, body→state each tick, pushed manually by sensor/cast talents) | `ScoreGate`: `PhysicsSimulator.AddScoreGate`, `PushScoreGateCommand`, `StepPhysiscsSimulationCommand.ApplyPhysicsSimulationToMatchModel`, `GatePassConfig` impulse block, `Presentation/Match/Features/ScoreGate/` |
| Countdown end tick + client widget | `MatchSimulationStateS2C.WhacAMoleEndTick`, `Presentation/Match/Scripts/Commands/UpdateMatchTimerCountdownCommand.cs` |
| Stage end / restart | `Simulation/Match/Scripts/Commands/TryEndWhacAMoleStageCommand.cs`, `StageEndedCommand.cs`, `ServerMatchNetworkTickProcessor.TryHandleStageEnded` |
| "No damage in this mode" guard | `Simulation/Match/Scripts/Commands/TryHitPlayerCommand.cs` (`_stageDataService.IsBonusStage`) |
| Layout authoring components + bake buttons | `Shared/LevelEnvironment/Scripts/EnvironmentGenerator.cs`, `MoleSpawnPoint.cs`, `ScoreGateSpawnPoint.cs` |
| Full-state / rejoin rebuild | `Presentation/Match/Scripts/Commands/NetEvents/SyncMatchSimulationStateCommand.cs` |
| Net event checklist | `/add-net-event` skill (`Assets/.claude/skills/add-net-event/`) |

**Facts to respect (verified in the current code, not assumed):**

- `StageType` today is `None=0, DeathMatch=1, WhacAMole=2, GatePass=3`. It is serialized as a **byte** — never renumber.
- `StageTypeExtensions.IsBonusStage()` is `{WhacAMole, GatePass}` and it means **"scoring stage: no health, countdown, gems by rank"**. **FreeMarket must NOT join that set** — it would inherit gems-by-rank and the score HUD. It needs its own predicates (§5.1).
- `StageDataService.AmountOfStagesEntered` is incremented **before** `ResolveStageTypeForCurrentStage()` runs, so inside that method the counter is the **1-based number of the stage being created**. `FreeMarketStageNumbers = [1,5,10]` therefore means "the 1st, 5th and 10th stage of the match".
- `MatchDataService._didntPlayYetStageIndexesPerStageType` is a hard-coded dictionary with only `DeathMatch`, `WhacAMole`, `GatePass` keys. **Adding `FreeMarket` is mandatory** or the first random FreeMarket layout roll throws `KeyNotFoundException`.
- **`MatchFullTickPacketS2C.eventMask2` bits 0–14 are taken** (Soul 0–1, Rock 2–3, Lava 4–5, Frozen 6–7, Moles 8–11, ScoreGatePassed 12, GateTrapClosing 13, BarrelDash 14). **The next free bit is 15.**
- `PhysicsBodyType` highest used value is `ScoreGate = 22`; `PhysicsCollisionType` highest used is `ScoreGate = 16`. Collision types are **bit indexes** into a 32-bit field → 31 is the ceiling.
- `LockOnTargetType` is `Heart=0, PowerUpBall=1, StartMatchWall=2, Mole=3`.
- `PhysicsSimulator.RayCast` filters by the **body-type array** it is given, not by collision categories — so making a body lock-on-able only requires adding its `PhysicsBodyType` to `TrySendPlayersLockOnTargetChangedCommand._cachedBodyTypesRayCastCanHit`, no filter changes.
- The TalentCard body is **Static**, non-sensor, and its category only admits `PlayerBullet`. It is raycast-visible today already.
- **`TryShootLockedOnTargetsCommand` loops over *all* shootable locked targets** and fires at each in the same tick. That loop *is* the "one shot hit two cards" edge case — there is no bullet-based way for a single shot to hit two cards.
- The two card-hit paths land at **different points in the tick**: the lock-on shot runs inside `ProcessPackets` (before the physics step), the bullet↔card collision runs inside `StepPhysiscsSimulationCommand` → `ProcessCachedCollisionsCommand` (after it). Any "one purchase per stage" rule must be enforced by state that both paths read, not by ordering.
- `MatchSimulationStateS2C` sizes each player's lock-on list as `maxPlayers - 1 + maxPowerUpBalls + maxMoles`, and `MaxCap.ConcurrentLockOnTargets` mirrors that sum. **Both must grow** for the two new target types, or the `FixedUnorderedList` overflows the first time a player sees enough targets.
- `StageEndedCommand.GetPlayerToFocusOn()` has no "no winner" path: with `winningTeamId = 0` every loop fails, it logs an error and returns `null`, and the caller dereferences `playerToFocusOn.Id` → **NullReferenceException**. The **client** already handles `WinningTeamId == 0` gracefully (`isThereOnlyOneTeam` → skip the winners UI and the camera zoom), so only the server side needs a fix.
- `TalentCardObtainedNetEventS2C` already carries `TalentCardId`, `ObtainedByPlayerId`, the player's full talent list and `DidReplaceTalent` — **no new net event is needed for the purchase itself.**
- `PlayersTalentsManager.TryAddTalentToPlayer` returns **false** when the player already owns that talent, and silently **replaces the currently selected talent** when he is at `MaxConcurrentTalentsForPlayer`. Both cases matter for a shop (§6.4).
- **`FishingRodCaughtEnemyType` already has a non-player case** (`None=0, Player=1, Mole=2`), and `FishingRodTalentController` already has a `CatchMole` path next to `CatchEnemy`. Catching a TrainingPuppet is therefore an established pattern, not new machinery — the puppet is the *moving* variant (its caught-phase branch follows the target's position, like the player branch, unlike the nailed-down mole).
- `Presentation/Match/Features/HitDamageIndicatorEffect/` already exists (`IHitDamageIndicatorEffectController` + pool + view) and is what the puppet's hit numbers reuse.
- There is **no TalentCard authoring component today** — `EnvironmentGenerator` bakes walls, power-ups, moles, score gates and gate traps, but `_talentCardsJson` has no `Refresh…` button. FreeMarket needs one (§9).

---

## 3. Coding conventions every package MUST follow

**Invoke the `/coding-guidlines` skill before writing code.** The rules most likely to bite here:

- **Zero garbage allocation in the Simulation domain.** No `new`, LINQ, boxing, closures or `params` on any per-tick path. Every buffer (closest-card resolution, puppet lists) is pre-allocated from `NetworkConfig.MaxCap` in a constructor / `ResolveDependencies()` / entry point.
- **Commands:** one job, fluent `SetX(...)` setters, `Execute()`. Create them once via `_commandFactory.CreateCommandVoid<T>()` — never per tick.
- **`Get*` never mutates**; `Try` prefix for anything that can early-exit.
- **Naming:** enums suffixed `Type`, booleans prefixed `is/can/has`, time fields carry their unit (`StageDurationSeconds`), no abbreviations, no magic numbers — every tuning value lives in `FreeMarketConfig`.
- **Never write `Velocity`/`AngularVelocity` on a player directly** — use `AddForceToPlayerCommand` / `SpinPlayerCommand`. The **TrainingPuppet is not a player**; it is driven through its Box2D body and is exempt (same exemption the ScoreGate has).
- Presentation is **render-only**. Model mutation belongs in `PresentationMatchNetEventsHandler.Process*Events`; `Handle*NetEventsCommand` only reads and clears. See the "Who writes to `IMatchDataService`" table in `Assets/CLAUDE.md`.
- Method ≤ 30 lines, class ≤ 200 lines.

---

## 4. Work packages & dependency order

```
A. Foundation: StageType.FreeMarket, scheduling config, layout pool, timer, stage end
      │
      ├── B. TalentCard as a purchase: lock-on target type, one-hit grant,
      │      one-purchase-per-stage lock, closest-card tie-break
      │
      ├── C. TrainingPuppet entity: state, physics body, spawn, lock-on,
      │      bullet hits, per-talent interaction matrix
      │
      └── D. Presentation: puppet MVC, card/lock-on visuals, "shot spent" HUD,
             countdown, stage-end, rejoin
                   │
                   └── E. Stage authoring: TalentCard + TrainingPuppet spawn
                          components, the FreeMarket layout
```

A is blocking for everything. B and C are independent of each other and can run in parallel. D depends on B and C's state/events existing. E can start as soon as A lands (the layout can be authored before the entities behave correctly).

---

## 5. Work Package A — Foundation

### 5.1 Enum + predicates

`Shared/Scripts/Enums/StageType.cs`:
```csharp
public enum StageType
{
    None = 0,
    DeathMatch = 1,
    WhacAMole = 2,
    GatePass = 3,
    FreeMarket = 4,
}
```

`Shared/Scripts/Enums/StageTypeExtensions.cs` — **leave `IsBonusStage()` alone** and add two orthogonal predicates:
```csharp
// A Bonus Stage is a scoring stage (Whac-A-Mole, GatePass): gems are awarded by rank at the end.
// FreeMarket is deliberately NOT one - nobody scores and nobody wins it.
public static bool IsBonusStage(this StageType stageType) => stageType == StageType.WhacAMole || stageType == StageType.GatePass;

// Stages that run on a countdown instead of on elimination.
public static bool HasStageCountdownTimer(this StageType stageType) => stageType.IsBonusStage() || stageType == StageType.FreeMarket;

// Stages where players take no damage and cannot be eliminated.
public static bool IsPlayerDamageDisabled(this StageType stageType) => stageType.IsBonusStage() || stageType == StageType.FreeMarket;
```
Every call site that currently asks `IsBonusStage()` must be re-read and moved to whichever predicate it actually meant:

| Call site | Today | Becomes |
|---|---|---|
| `TryHitPlayerCommand` damage guard | `_stageDataService.IsBonusStage` | `IsPlayerDamageDisabled` |
| `TryEndWhacAMoleStageCommand.isCountdownOver` | `StageType.IsBonusStage()` | `HasStageCountdownTimer()` (and the gems/winner part branches on `IsBonusStage()` inside — §5.5) |
| `InitStageCommand.SetupBonusStageData` end-tick guard | `IsBonusStage()` | `HasStageCountdownTimer()` |
| `StageEndedCommand.GetPlayerToFocusOn` | `IsBonusStage()` | unchanged + a FreeMarket branch (§5.5) |
| `MatchPlayerUIControllers` score-instead-of-health switch | `WhacAMole`/`GatePass` cases | FreeMarket hides health but shows **no score** (§8.4) |
| `TrySendPlayersLockOnTargetChangedCommand` target selection | two-way branch | three-way (§6.2) |
| Client countdown / team board gating | `IsBonusStage()` | countdown → `HasStageCountdownTimer()`; team board score → keep `IsBonusStage()` |

`IStageDataService` / `StageDataService`: add `bool IsFreeMarketStage => _matchDataService.SimulationState.StageType == StageType.FreeMarket;` next to `IsWhacAMoleStage`.

### 5.2 Config

`Simulation/Scripts/Configurations/SimulationGamePlayInnerConfig.cs`:
```csharp
// FreeMarket is scheduled by explicit stage numbers, not by a cadence, because it is a shopping break
// the designer wants at specific points of the match (e.g. the opening stage and then every few stages).
public bool IsFreeMarketStageEnabled = true;
[ConditionalField(nameof(IsFreeMarketStageEnabled), true)]
public List<int> FreeMarketStageNumbers = new List<int> { 1, 5, 10 };
[ConditionalField(nameof(IsFreeMarketStageEnabled), true)]
public int DefaultFreeMarketEnvironmentId = 24;   // the layout authored in Package E
public FreeMarketConfig FreeMarket;
```

New `Simulation/Scripts/Configurations/FreeMarketConfig.cs`:
```csharp
[System.Serializable]
public class FreeMarketConfig
{
    public float StageDurationSeconds = 25f;
    // A card is bought in one hit here, so the shared TalentCardHealth chip-away does not apply.
    public bool ShouldRandomizeCardTalents = true; // re-roll each card's talent per stage instead of using the authored one
    public int TrainingPuppetsAmount = 3;

    // TrainingPuppet body. Mass is the headline value: it MUST stay above the player's effective mass so a puppet
    // shoves back instead of flying off on the first ram. When TrainingPuppetMass > 0 it overrides the density-derived
    // mass, exactly like ScoreGateMass does for the gate.
    public float TrainingPuppetMass = 4f;
    public float TrainingPuppetRadius = 1.1f;
    // Decorative only: the bar drains as the puppet is hit and stays at 0. A puppet is never destroyed, so this
    // number changes how long the bar reads as "fresh", nothing else.
    public ushort TrainingPuppetMaxHealth = 100;
    public float TrainingPuppetHealthRegenPerSecond = 0f; // 0 = the bar stays where the players left it
    public float TrainingPuppetDensity = 3f;
    public float TrainingPuppetRestitution = 0.4f;
    public float TrainingPuppetLinearDamping = 1.2f;   // a shoved puppet drifts and settles
    public float TrainingPuppetAngularDamping = 1.2f;  // a spun puppet decays after a couple of turns

    // How hard each talent shoves the puppet. Values are impulse PER UNIT MASS (spin PER UNIT INERTIA), so they stay
    // meaningful when the mass changes. Mirrors the GatePassConfig block - same rationale, same units, same names.
    public float KOPushImpulse = 72f;
    public float KOSpinImpulse = 36f;
    public float HeadbuttPushImpulse = 42f;
    public float HeadbuttSpinImpulse = 12f;
    public float ChickenEggSpinImpulse = 4f;
    public float GrapplingHookReactionImpulse = 24f;
    public float MagneticPullImpulse = 72f;
    public float YearsOfPainPushImpulse = 96f;
    public float YearsOfPainSpinImpulse = 36f;
    public float WaterGunPushImpulsePerSecond = 120f;
    public float NukePushImpulse = 120f;
    public float NukeSpinImpulse = 45f;
}
```
> Per-talent impulses live here rather than in each `TalentsConfig` block because that is what the shipped `GatePassConfig` does for the ScoreGate. Consistency with the existing precedent beats the theoretical "tune it next to the talent" argument; do not split the two conventions.

### 5.3 Layout pool

`Shared/Scripts/Configs/EnvironmentConfig.cs`:
```csharp
public List<int> FreeMarketLayoutIndexes;

case StageType.FreeMarket: return FreeMarketLayoutIndexes;   // in GetLayoutIndexesForStageType
```
`Simulation/Match/Scripts/MatchModel/MatchDataService.cs` — add the dictionary key (mandatory, see §2):
```csharp
{ StageType.FreeMarket, new List<int>() },
```
`InitStageCommand.GetDefaultEnvironmentLayoutId` gains `case StageType.FreeMarket: return gamePlayConfig.DefaultFreeMarketEnvironmentId;`.

### 5.4 Stage selection

`InitStageCommand.ResolveStageTypeForCurrentStage()` — **FreeMarket is checked first** and wins over the bonus cadence, because its stage numbers are explicit while the bonus cadence is periodic and can slide:
```csharp
private StageType ResolveStageTypeForCurrentStage()
{
    var gamePlayConfig = _gamePlayConfigService.GamePlayConfig;
    var currentStageNumber = _stageDataService.AmountOfStagesEntered;

    // Explicit stage numbers beat the periodic bonus cadence: when both land on the same stage the shop wins,
    // and the bonus stage simply happens on its next multiple.
    if (gamePlayConfig.IsFreeMarketStageEnabled && gamePlayConfig.FreeMarketStageNumbers.Contains(currentStageNumber))
    {
        return StageType.FreeMarket;
    }

    var isRotationConfigured = gamePlayConfig.AreBonusStagesEnabled && gamePlayConfig.BonusStageEveryXStages > 0;
    var didReachBonusStage = isRotationConfigured && currentStageNumber % gamePlayConfig.BonusStageEveryXStages == 0;

    return didReachBonusStage ? _bonusStageRotationService.ResolveNextBonusStageType() : StageType.DeathMatch;
}
```
`List<int>.Contains` on a designer-sized list is fine here — this runs once per stage, not per tick.

`SetupBonusStageData(stageType)` — rename to **`SetupStageTimerAndScoreData`** and extend:
```csharp
if (!stageType.HasStageCountdownTimer())
{
    simulationState.WhacAMoleEndTick = 0;
    return;
}

var stageDurationSeconds = gamePlayConfig.PreparationPhaseDuration + GetStageDurationSeconds(stageType);
```
with `GetStageDurationSeconds` switching `WhacAMole → WhacAMole.StageDurationSeconds`, `GatePass → GatePass.StageDurationSeconds`, `FreeMarket → FreeMarket.StageDurationSeconds`. The score resets (`ResetMolesHitPerTeam` / `ResetMolesHitScoreForAllPlayers`) stay unconditional — they are already called for every stage type.

`CreateEnvironmentLayout(...)` gains `CreateTrainingPuppets(mapSizeMultiplier)` (§7.4) after `CreateTalentCards`.

`CreateTalentCards` gains the FreeMarket branches (§6.1): one-hit health and optional talent randomisation.

`RestartStageData()` gains `_freeMarketPurchaseService.ClearAllData();` (§6.3) next to the other tracker clears.

### 5.5 Stage end

`TryEndWhacAMoleStageCommand` (keep the file name; it is already the generic countdown-end command in practice — or `git mv` it *and its `.meta`* to `TryEndTimedStageCommand`, which is the better name and is preferred):

```csharp
var isCountdownOver = simulationState.StageType.HasStageCountdownTimer()
                      && !simulationState.IsInPreparationPhase
                      && !_stageDataService.IsStageEnded
                      && _processedTick >= simulationState.WhacAMoleEndTick;
if (!isCountdownOver) return;

if (simulationState.StageType == StageType.WhacAMole) { HideAllMoles(); }

var winningTeamId = (ushort)0;   // FreeMarket has no winner and awards no gems

if (simulationState.StageType.IsBonusStage())
{
    var highestMolesHit = GetHighestMolesHit();
    winningTeamId = GetLowestTeamIdWithMolesHit(highestMolesHit);
    AwardGemsByRank();
}

_stageEndedCommand.SetWinningTeamId(winningTeamId).SetProcessedTick(_processedTick).Execute();
```

`StageEndedCommand.GetPlayerToFocusOn()` — add the FreeMarket branch **before** the existing loops, so it never falls through to the "somehow didn't find player" error:
```csharp
// FreeMarket has no winner, so there is nobody to zoom on. The client skips the winners UI and the camera
// zoom entirely when WinningTeamId == 0; the focus id is only carried so the packet stays well-formed.
if (_matchDataService.SimulationState.StageType == StageType.FreeMarket)
{
    return _matchDataService.SimulationState.Players.Count > 0 ? _matchDataService.SimulationState.Players[0] : null;
}
```
and make the caller tolerate `null` (`playerToFocusOn?.Id ?? 0`) so an empty match cannot NRE. The client needs **no change** for this: `HandleStageEndNetEventsCommand` already treats `WinningTeamId == 0` as "no winner" and skips both the `StageEndedUiController.Show` call and the camera zoom.

**Acceptance (A):** With `FreeMarketStageNumbers = [1,5,10]`, stage 1 is a FreeMarket stage, stages 2–4 follow the normal DeathMatch/bonus rotation, stage 5 is FreeMarket again. A FreeMarket stage loads its own layout pool, runs a countdown, ends when the countdown expires with no winner UI, no gems, and the match rotates to the next stage. Players take no damage during it. No cards behave specially yet and no puppets exist — expected at this checkpoint.

---

## 6. Work Package B — TalentCard as a purchase

### 6.1 One-hit cards

`InitStageCommand.CreateTalentCards` today passes `Talents.TalentCardHealth` to every card. In FreeMarket a card must fall to a single hit:
```csharp
var isFreeMarketStage = stageType == StageType.FreeMarket;
var cardHealth = isFreeMarketStage ? (ushort)1 : _gamePlayConfigService.GamePlayConfig.Talents.TalentCardHealth;
var talentType = isFreeMarketStage && freeMarketConfig.ShouldRandomizeCardTalents
    ? _freeMarketCardTalentRoller.ResolveNextTalentType()
    : talentCard.TalentType;
```
Pass `stageType` into `CreateEnvironmentLayout`/`CreateTalentCards` (it is already resolved at the top of `Execute`). Health `1` keeps the *existing* bullet path working unchanged — one bullet already deals `PlayerBullet.HitDamage ≥ 1`, so the card is obtained on the first hit with **zero changes** to `HandlePlayerBulletTalentCardCollision`.

`ShouldRandomizeCardTalents` needs a small roller (`Simulation/Match/Scripts/FreeMarket/IFreeMarketCardTalentRoller`): pre-allocated `List<TalentType>` of all real talents (excluding the `Bomb`/`Hammer` stubs), shuffled with `RNG.Shuffle` at stage init, handed out without repetition, refilled when exhausted. Zero allocation per card.

### 6.2 Cards as lock-on targets

`Shared/Scripts/S2CModels/LockOnTargetType.cs`:
```csharp
TalentCard = 4,
TrainingPuppet = 5,   // Package C
```

`TrySendPlayersLockOnTargetChangedCommand`:
- add `PhysicsBodyType.TalentCard` and `PhysicsBodyType.TrainingPuppet` to `_cachedBodyTypesRayCastCanHit`;
- make the target selection three-way:
```csharp
if (_stageDataService.IsWhacAMoleStage)
{
    FindTargetedMolesOfCaster(...);
}
else if (_stageDataService.IsFreeMarketStage)
{
    FindTargetedTalentCardsOfCaster(...);      // skipped entirely once the player has spent his shot (§6.3)
    FindTargetedTrainingPuppetsOfCaster(...);
}
else if (!_stageDataService.IsBonusStage)
{
    FindTargetedEnemyIdsOfCaster(...);
}

FindTargetedPowerUpBallsOfCaster(...);         // always, unchanged
```
- `FindTargetedTalentCardsOfCaster` / `FindTargetedTrainingPuppetsOfCaster` are copies of `FindTargetedMolesOfCaster`: cone test → raycast → confirm the ray hit that exact body type + id → `AddLockedOnTarget(...)`. Cards use their state `Position`; puppets use their (physics-driven) state `Position`.
- **A card whose talent the caster already owns is not lockable by him.** `FindTargetedTalentCardsOfCaster` skips it (`TalentsState.TryGetTalentIndexByType(card.TalentType, out _)`). The shot is spent on any card the player actually hits (§6.4), so the shop must not offer him a reticle on a card that cannot give him anything. Other players still see and can buy that same card.
- **Enemy lock-on stays off in FreeMarket** — players cannot damage each other there, so a reticle on an enemy would lie.

**Capacity:** `MaxCap.ConcurrentLockOnTargets` and the `PlayerStateS2C` lock-on list size in `MatchSimulationStateS2C` must both grow by `ConcurrentTalentCards + ConcurrentTrainingPuppets`. They are allocated once per match, so the extra headroom costs memory only — but skipping it overflows a `FixedUnorderedList` in a stage full of cards.

### 6.3 One purchase per player per stage

Add to `PlayerSpaceshipStateS2C`:
```csharp
public bool HasSpentFreeMarketShot;
```
- Included in `Serialize`/`Deserialize` and `GetClone` (full state → rejoin shows the right HUD), **not** in `SerializeDeltas` — it changes at most once per player per stage, and the `TalentCardObtained` net event already tells the client the moment it flips.
- Reset to `false` in `InitStageCommand.SetupPlayers` alongside `IsSpinned` / `IsExposedToLava`.

Gate every shooting path on it:

| Path | Guard |
|---|---|
| `TryPerformShootForPlayerIfNotOnCooldownCommand.Execute` | early-return when `HasSpentFreeMarketShot` |
| `TryShootLockedOnTargetsCommand.Execute` | early-return when `HasSpentFreeMarketShot` |
| `TrySendPlayersLockOnTargetChangedCommand` `canPlayerFindTargets` | `&& !playerState.Spaceship.HasSpentFreeMarketShot` — a player who cannot shoot should not keep a reticle |

> The flag lives on the player, not in a side service, precisely because the two card-hit paths run at different points in the tick (§2) and because rejoin has to restore it. Do **not** try to enforce this with the shoot cooldown — a huge cooldown would still tick down and would be invisible to the lock-on path.

The flag is set in **exactly one place**: the talent-grant funnel below.

### 6.4 The grant funnel + the closest-card rule

New `Simulation/Match/Scripts/Commands/TryObtainTalentCardCommand.cs` — the single place a card is bought, mirroring how `TryHitMoleCommand` funnels every mole hit:

```
SetTalentCardId(ushort) / SetByPlayerId(ushort) / SetProcessedTick(int)

Execute():
  if card id no longer exists                          -> log topic, return
  if player.Spaceship.HasSpentFreeMarketShot           -> return          (FreeMarket only)

  var didGrantTalent = _playersTalentsManager.TryAddTalentToPlayer(card.TalentType, playerId, tick, out _, out didReplace)

  if didGrantTalent:
      netEvents.AddTalentCardObtainedNetEvent(tick, cardId, playerId, playerTalents, didReplace)
      remove card from state + remove its physics body

  if stage is FreeMarket: player.Spaceship.HasSpentFreeMarketShot = true   // spent either way, see below
```

**Spending rules (decided, not open):**

- **At max talents** — `TryAddTalentToPlayer` replaces the currently selected talent and returns `true`. The purchase goes through, `DidReplaceTalent` is `true`, and **the shot is spent**. Trading a talent for a talent is the cost of shopping while full; the client already renders the swap from the event's talent list.
- **Talent already owned** — `TryAddTalentToPlayer` returns `false`, no talent is granted and **the card survives**, but **the shot is still spent**. This is only reachable by aiming a bullet at the card by hand: lock-on filters those cards out for that player (§6.2), so a player cannot lose his shot to one through the normal flow.
- **Nothing hit** — a shot that hits no card at all never touches the flag. A player can miss and try again; he only loses the shot by landing it on a card.

Then rewire both hit paths through it:
- `ProcessCachedCollisionsCommand.HandlePlayerBulletTalentCardCollision` — keep the bullet destruction and the `Health` chip-away for non-FreeMarket stages, and replace the inline `TryAddTalentToPlayer` + `DestroyTalentCard` block with a `TryObtainTalentCardCommand` call.
- `TryShootLockedOnTargetsCommand` — new `case LockOnTargetType.TalentCard:` branch.

**The closest-card rule** lives in `TryShootLockedOnTargetsCommand`, because that loop is the only way one shot reaches two cards:
```csharp
// A player can be locked on several cards at once and this loop fires at every shootable target in the same
// tick, so in FreeMarket the shot buys exactly one card: the one closest to the shooter.
public void Execute()
{
    ...
    var closestTalentCardId = ushort.MaxValue;
    var closestTalentCardDistanceSquared = float.MaxValue;

    for (int i = 0; i < targetedObjects.Count; i++)
    {
        ...
        case LockOnTargetType.TalentCard:
            TryTrackClosestTalentCard(targetId, casterPosition, ref closestTalentCardId, ref closestTalentCardDistanceSquared);
            break;
    }

    if (closestTalentCardId != ushort.MaxValue)
    {
        _tryObtainTalentCardCommand.SetTalentCardId(closestTalentCardId).SetByPlayerId(_casterPlayerId).SetProcessedTick(_processedTick).Execute();
    }
}
```
Distances use `Vector2.DistanceSquared(casterPosition, card.Position)` — no square roots, no allocation. Ties (two cards exactly equidistant) resolve to the **lower card id**, so server and client agree deterministically.

`_lockOnTargetTimerService.ResetTimer(...)` must still be called for **every** target in the loop, including the card that lost the tie-break — the shot was fired, so every reticle re-arms.

**Acceptance (B):** In a FreeMarket stage a player locks a card, the reticle arms, one shot grants that card's talent and destroys the card. The player's reticles disappear and further shoot input does nothing for the rest of the stage. A player locked on two cards at once buys only the closer one and the other card stays on the field. A card whose talent the player already owns is never offered as a lock-on target, and bullet-hitting one by hand still spends his shot without granting anything. Bullet-shot cards behave identically to locked-on ones. In DeathMatch stages talent cards behave exactly as before (multi-hit health, no shot lock).

---

## 7. Work Package C — TrainingPuppet

### 7.1 State model

New `Shared/Scripts/S2CModels/TrainingPuppetStateS2C.cs`, modelled on `ScoreGateStateS2C`:
```csharp
public struct TrainingPuppetStateS2C : INetSerializable
{
    public ushort Id;
    public Vector2 Position;
    public Vector2 Rotation;         // unit facing vector, so the client can show it spinning
    public float AngularVelocity;    // optional, only if the client wants to drive a spin animation
    public ushort Health;            // decorative: drains on hits, floors at 0, never destroys the puppet
    public ushort MaxHealth;
    // Serialize: (byte)Id, PutVector2Quantized(Position), rotation as angle16, Health, MaxHealth
}
```
`Health` is part of the serialized state (not only of the hit event) so a rejoining client draws the bar at the value the puppet is actually at.

`MatchSimulationStateS2C`:
- `public FixedUnorderedList<TrainingPuppetStateS2C> TrainingPuppets;`
- constructed from a new `maxTrainingPuppets` parameter threaded from `NetworkConfig.MaxCap.ConcurrentTrainingPuppets` (default `8`) — **update every `new MatchSimulationStateS2C(` call site** (grep it);
- count-prefixed block in `Serialize`/`Deserialize`, next to the `ScoreGates` block;
- `TryGetTrainingPuppetIndexById` / `GetTrainingPuppetById` helpers (copy the score-gate set);
- cleared in `ClearObjectStates()`.

`Simulation/Match/Scripts/MatchModel/MatchDataService.cs`: `AddTrainingPuppet(...)` mirroring `AddScoreGate`.

### 7.2 Physics body

```csharp
// PhysicsBodyType
TrainingPuppet = 23,
// PhysicsCollisionType
TrainingPuppet = 17,
```

`PhysicsCollisionFilters.GetCollisionsCategory` — the puppet stands in for a player, so it accepts everything a player accepts:
```csharp
// A practice dummy has to be reachable by everything that can reach a player: rams, bullets, talent projectiles
// and the solid bodies. It is dynamic and heavy, so the solver handles the rams and the bullets on its own.
case PhysicsBodyType.TrainingPuppet:
    collisionMask = GetCollisionMask(PhysicsCollisionType.PlayerSpaceship)
                    | GetCollisionMask(PhysicsCollisionType.PlayerBullet)
                    | GetCollisionMask(PhysicsCollisionType.KOProjectile)
                    | GetCollisionMask(PhysicsCollisionType.FrigidBlock)
                    | GetCollisionMask(PhysicsCollisionType.GrapplingHookProjectile)
                    | GetCollisionMask(PhysicsCollisionType.FishingRodTip)
                    | GetCollisionMask(PhysicsCollisionType.SoulGhost)
                    | GetCollisionMask(PhysicsCollisionType.ChickenEgg)
                    | GetCollisionMask(PhysicsCollisionType.Wall)
                    | GetCollisionMask(PhysicsCollisionType.ScoreGate);
    break;
```
and add `| GetCollisionMask(PhysicsCollisionType.TrainingPuppet)` to the `PlayerSpaceship`, `PlayerBullet`, `Wall`, `KOProjectile`, `FrigidBlock`, `GrapplingHookProjectile`, `FishingRodTip`, `SoulGhost` and `ChickenEgg` cases.

`IPhysicsSimulator` / `PhysicsSimulator` — copy `AddScoreGate` structurally:
```csharp
void AddTrainingPuppet(ushort id, Vector2 position, float radius, float mass, float density,
                       float restitution, float linearDamping, float angularDamping);
Body GetTrainingPuppet(ushort id);
void RemoveTrainingPuppet(ushort id);
```
- `BodyType.Dynamic`, one `CircleShape` fixture (`isSensor = false`), `userData = new PhysicsBodyData(id, PhysicsBodyType.TrainingPuppet)`.
- When `mass > 0`, override the density-derived mass with `SetMassData` exactly as `AddScoreGate` does — that is what makes "greater mass than a player" a single tunable number.
- **Do not** disable rotation: "can be spinned" means the body must have real rotational inertia and angular damping.

`StepPhysiscsSimulationCommand.ApplyPhysicsSimulationToMatchModel` — copy body→state each tick, like the score gates:
```csharp
puppet.Position = body.Position;
puppet.Rotation = body.GetAngle().FromAngleRadians();
```
There is **no** state→body copy: the puppet is physics-authoritative.

### 7.3 Lock-on + bullets

- Lock-on: `LockOnTargetType.TrainingPuppet` + `FindTargetedTrainingPuppetsOfCaster` (§6.2), and a `case LockOnTargetType.TrainingPuppet:` in `TryShootLockedOnTargetsCommand` that pushes the puppet away from the caster and emits the hit net event (§7.5). A locked-on shot must **not** consume the FreeMarket purchase — only cards do that.
- Bullets: new `HandleBulletTrainingPuppetCollision` in `ProcessCachedCollisionsCommand` — destroy the bullet (copy `HandleBulletFrigidBlockCollision`); the solver already imparts the nudge because both bodies are solid.
- Player rams: new `HandlePlayerTrainingPuppetCollision` — a near copy of `HandlePlayerScoreGateCollision`. **Required**, because player bodies get their velocity overwritten from state every tick (`CopyPlayerStateToBody`), so the solver's response to the *player* is discarded unless the handler reflects the player's velocity in state. Keep the frozen-player special case the frigid-block/score-gate handlers have.

### 7.4 Spawning

`EnvironmentLayoutConfig`: `_trainingPuppetSpawnPointsJson` + `GetTrainingPuppetSpawnPoints()` / `SetTrainingPuppetSpawnPointsJson(...)` (copy `GetMoleSpawnPoints`, return `default` on empty).
`Shared/Scripts/S2CModels/TrainingPuppetSpawnPointConfig.cs`: `{ Vector2 Position; }`.
`EnvironmentConfig.SetTrainingPuppetSpawnPoints(...)`, `IMatchEnvironmentConfigDataService.TrainingPuppetSpawnPoints`.

`InitStageCommand.CreateTrainingPuppets(float mapSizeMultiplier)`:
- Early-return when the stage is not FreeMarket **or** the layout authors no puppet spawn points.
- Spawn `min(FreeMarketConfig.TrainingPuppetsAmount, spawnPoints.Length)` puppets at distinct spawn points, chosen with `RNG.Shuffle` over a pre-allocated index buffer so a layout may author more than 3 points and still place exactly 3 per stage. Log an error if the layout authors fewer points than `TrainingPuppetsAmount`.
- Ids are `1..N` from the spawn index; radius/mass/damping from `FreeMarketConfig`, position scaled by `mapSizeMultiplier`.

Teardown is free: `RestartStageData()` already calls `_physicsSimulator.ClearAllData()` and `ClearObjectStates()` — **verify** `ClearObjectStates` clears `TrainingPuppets`.

### 7.5 "Hit by any talent, like a player" — the interaction matrix

Almost every talent projectile is a Box2D **sensor** (`KOProjectile`, `GrapplingHookProjectile`, `FishingRodTip`, `SoulGhost`, `SwapField`, `ChickenEgg`), so it produces contact *events* but **zero solver impulse**; and three talents (`MagneticPull`, `YearsOfPain`, `WaterGun`) reach their targets through **geometric casts** and produce no contact at all. "Any talent hits the puppet" therefore has to be written per talent — exactly as it was for the ScoreGate.

**The good news: this work is 90% a repeat of the ScoreGate matrix.** For every talent, the ScoreGate integration already exists and is merged; the puppet integration is the same edit one line below it.

#### 7.5.0 The one shared funnel

New `Simulation/Match/Scripts/Commands/TryHitTrainingPuppetCommand.cs` — the single place a puppet is hit, so the push, the decorative damage and the client feedback can never drift apart (same role `TryHitMoleCommand` plays for moles, and it absorbs what would otherwise be a `PushScoreGateCommand` clone):

```
SetTrainingPuppetId(ushort) / SetByPlayerId(ushort) / SetDamage(ushort) / SetImpulse(Vector2)
/ SetWorldContactPoint(Vector2) / SetExtraSpinImpulse(float) / SetProcessedTick(int)

Execute():
  if puppet id no longer exists                 -> log topic, return
  body = _physicsSimulator.GetTrainingPuppet(id)
  body.ApplyLinearImpulse(_impulse * body.GetMass(), _worldContactPoint)   // off-centre point already induces spin
  if _extraSpinImpulse != 0: body.ApplyAngularImpulse(_extraSpinImpulse * body.GetInertia())
  puppet.Health = (ushort)Math.Max(0, puppet.Health - _damage)             // floors at 0, the puppet lives on
  netEvents.AddTrainingPuppetHitNetEvent(tick, id, byPlayerId, _damage, puppet.Health, puppet.MaxHealth, _worldContactPoint)
```

Impulses scale by `GetMass()` / `GetInertia()` so re-tuning `TrainingPuppetMass` does not invalidate every talent's numbers (same convention as `PushScoreGateCommand`). **Damage is the same number the call site would pass to `TryHitPlayerCommand`** — `PlayerBullet.HitDamage` for bullets, `PlayerSpaceship.LockOnTargetHitDamage` for a locked-on shot, each talent's own hit damage for talents — so the bar drains at a rate players can read against their own health bar. A talent that pushes without damaging (soul ghost stop, grappling-hook anchor) passes damage `0`; the event still fires so the client can react, but the bar does not move.

| # | Talent | Puppet interaction | Where |
|---|---|---|---|
| 1 | **Swap** | Swap targets an *enemy player*; a puppet is not a player. **No interaction** — leave it. (If designers want "swap with a puppet", that is a separate feature - see §12.) | — |
| 2 | **YearsOfPain** | Add `TrainingPuppet` to the cast priority list; on a hit, `PushTrainingPuppetCommand` with `YearsOfPainPushImpulse` along the caster's facing + `YearsOfPainSpinImpulse`. | `YearsOfPainTalentController` + `RectangleCastByPriority` |
| 3 | **SentryGun** | **Free** — fires normal bullets, covered by `HandleBulletTrainingPuppetCollision`. | — |
| 4 | **DashPulse / Umbrella / BarrelDash** | **Indirect** — the caster is flung into the puppet and the solid-body contact does the work. | — |
| 5 | **KO** (sensor) | `HandleKOProjectileTrainingPuppetCollision` → push + spin (`KOPushImpulse` / `KOSpinImpulse`), then destroy the projectile exactly as the wall/gate paths do. | `ProcessCachedCollisionsCommand` |
| 6 | **GrapplingHook** (sensor) | Anchor on the puppet like it anchors on the gate, re-reading the puppet's transform each tick (moving anchor), plus a small `GrapplingHookReactionImpulse` toward the caster. | `HandleGrapplingHookCollision` + controller anchor tracking |
| 7 | **MagneticPull** (cast) | Add `TrainingPuppet` to the cast; pull the puppet toward the caster with `MagneticPullImpulse`. | `MagneticPullTalentController` |
| 8 | **Chicken** (static sensor egg) | Egg breaks on contact and twists the puppet: zero linear impulse, `ChickenEggSpinImpulse` with a randomised sign. Egg is `Static`, puppet is `Dynamic`, so Box2D **does** pair them — no manual overlap scan (unlike the egg↔mole case). | filters + `ProcessCachedCollisionsCommand` |
| 9 | **WaterGun** (cast) | Add the puppet to the stream cast; apply `WaterGunPushImpulsePerSecond * deltaTime` continuously while the stream is on. | `WaterGunTalentController` |
| 10 | **Headbutt** | `HandleHeadbuttTrainingPuppetCollision`: while dashing, a large `HeadbuttPushImpulse` scaled by charge + `HeadbuttSpinImpulse`, and end the dash like the enemy-hit path. | `ProcessCachedCollisionsCommand` |
| 11 | **Rock** | **Mostly free** — the enlarged, denser caster body shoves harder through the solver. Add a `TrainingPuppet` branch to `HandlePlayerRockCollision` so the caster's velocity reflects correctly. | `ProcessCachedCollisionsCommand` |
| 12 | **FrigidBlock** | **Free** — solid vs solid. Verify the block's idle-destroy timer does not fire early while resting against a drifting puppet. | verify only |
| 13 | **FishingRod** (sensor) | **Player-like: the tip catches the puppet and the second cast throws it.** See §7.6 — this is the one talent that needs more than a matrix row. | `ProcessCachedCollisionsCommand` + `FishingRodTalentController` |
| 14 | **Soul** (sensor) | Ghost stops on the puppet (add it to `HandleSoulGhostWallCollision`), no impulse — the ghost is intangible by design. | `ProcessCachedCollisionsCommand` |
| 15 | **Frozen** | A frozen player shoved by a puppet behaves as it does when shoved by a frigid block — mirror that special case in `HandlePlayerTrainingPuppetCollision`. | `ProcessCachedCollisionsCommand` |
| 16 | **Nuke (power-up)** | Add puppets to the nuke sweep alongside score gates: `NukePushImpulse` + `NukeSpinImpulse` in a random direction per puppet. | `NukePowerUpController` |
| — | **Bomb / Hammer** | Empty controller stubs, no `TalentType` value. **N/A.** | — |

New net event **`TrainingPuppetHitNetEventS2C`** (via `/add-net-event`, **`eventMask2` bit 15**) so the client can drain the bar, pop a hit number and play the reaction:
```csharp
public struct TrainingPuppetHitNetEventS2C : INetSerializable, IComparable<TrainingPuppetHitNetEventS2C>
{
    public int OccuredOnTick;
    public ushort TrainingPuppetId;
    public ushort ByPlayerId;
    public ushort Damage;
    public ushort RemainingHealth;
    public ushort MaxHealth;
    public Vector2 HitPosition;      // PutVector2Quantized - anchors the hit indicator
}
```
It is fired **only** from `TryHitTrainingPuppetCommand`, so every talent, bullet and ram gets the feedback for free. `MaxCap.TrainingPuppetHitNetEvents = 128;` and `MaxCap.ConcurrentTrainingPuppets = 8;`.

### 7.6 FishingRod catches a puppet (player-like)

The enum already has a non-player case, so this is an extension of an existing shape, not new machinery:

- `FishingRodCaughtEnemyType`: add `TrainingPuppet = 3`.
- `ProcessCachedCollisionsCommand`: new `HandleFishingRodTipTrainingPuppetCollision`, a copy of `HandleFishingRodTipMoleCollision`, calling a new `CatchTrainingPuppet(puppetId, tick)` on the caster's controller. Do **not** route the puppet into `HandleFishingRodTipWallCollision`.
- `FishingRodTalentController.CatchTrainingPuppet(ushort puppetId, int tick)` — a copy of `CatchMole` (same phase/flag/net-event shape), with `CaughtEnemyType = TrainingPuppet` and the puppet's current position as the tip position.
- `UpdateCaughtPhase` — the puppet is the **moving** caught target, so it follows the player branch, not the mole branch: `projectile.Position = puppet.Position;` each tick, and it bails out with `DeactivateTalent` only if the puppet id somehow vanished (it cannot today, but keep the guard uniform with the mole path).
- `ProcessThrowCastInput` / the throw — a puppet *can* be thrown, unlike a nailed-down mole, so mirror `PerformThrowEnemy` rather than `PerformSpinCaughtMole`:
  ```csharp
  var throwDirection = GetThrowDirection(casterPlayerState, puppet.Position);
  _tryHitTrainingPuppetCommand
      .SetTrainingPuppetId(projectile.CaughtEnemyId)
      .SetByPlayerId(_casterPlayerId)
      .SetDamage(0)                                   // the throw itself does not chip the bar; the landing hits do
      .SetImpulse(throwDirection * config.ThrowPushForce)
      .SetWorldContactPoint(puppet.Position)
      .SetExtraSpinImpulse(RNG.NextFloat(config.ThrowMinSpin, config.ThrowMaxSpin))
      .SetProcessedTick(tick)
      .Execute();
  _netEventsDataService.AddFishingRodThrowNetEvent(tick, _casterPlayerId, projectile.CaughtEnemyId, FishingRodCaughtEnemyType.TrainingPuppet, throwDirection);
  DeactivateTalent(tick);
  ```
  Reuse the existing `FishingRodTalentConfig.ThrowPushForce` / `ThrowMinSpin` / `ThrowMaxSpin` — a thrown puppet should feel like a thrown player, and it already lands heavier because of its mass.
- Client: `HandleFishingRodCaughtEnemyNetEventsCommand`, `HandleFishingRodThrowNetEventsCommand` and the caught-target aim arrow all switch on `FishingRodCaughtEnemyType` — each needs a `TrainingPuppet` case anchoring on `ITrainingPuppetControllers` instead of `IMatchPlayerControllers`. Grep `FishingRodCaughtEnemyType` in `Presentation/` and cover every hit.

**Acceptance (C):** In a forced FreeMarket stage, 3 puppets stand at authored positions. Every row of the matrix has been walked manually: ramming pushes a puppet and bounces the player, Rock pushes harder, KO pushes and spins it, Headbutt launches it, MagneticPull drags it in, YearsOfPain and WaterGun push it, a shot FrigidBlock knocks it aside, bullets stop on it, the grappling hook anchors on it and tracks it as it drifts, the fishing rod tip retracts off it, the soul ghost stops on it, a chicken egg twists it, the nuke launches all three. A puppet always feels heavier than a player. Puppets are gone in the next stage.

---

## 8. Work Package D — Presentation

### 8.1 TrainingPuppet MVC

New `Presentation/Match/Features/TrainingPuppet/Scripts/Mvc/`, modelled on `Features/ScoreGate/Scripts/Mvc/`:
- `TrainingPuppetView : MonoBehaviour` — sprite/animator, a hit-reaction trigger and a **health bar** (reuse the player health-bar prefab/component so the fill, colour and tween read identically). Dumb view, no logic.
- `TrainingPuppetController` — `CreateView(position, rotation)`, `InterpolateTransform(position, rotation, exponentialDecay)`, `SetHealth(current, max)`, `PlayHitReaction()`, `Destroy()`.
- `TrainingPuppetControllers : ITrainingPuppetControllers` — `Dictionary<ushort, TrainingPuppetController>` + a pool, `InitEntryPoint/InitExitPoint/DestroyAll`, `bool TryGetPosition(ushort id, out Vector2 position)`.
- The bar is **decorative**: it drains to 0 and stops. No death animation, no destroy path, no "puppet destroyed" event — if the art wants a "worn out" look at 0 health, that is a view-side state change only.
- View scale comes from `FreeMarketConfig.TrainingPuppetRadius × MapSizeMultiplier`, so the art always matches the collider.
- Bind in `GamePlayMatchInstaller` with a `[SerializeField] TrainingPuppetView _trainingPuppetViewPrefab;`, init/exit from `StartGamePlayMatchCommand` / `ExitGamePlayMatchCommand` next to the score gates.

### 8.2 Client model + per-frame transform

- `Presentation/Match/Scripts/Models/MatchTrainingPuppetModel.cs`: `Id`, `Position`, `Rotation`, `Health`, `MaxHealth`.
- `IMatchDataService` / `MatchDataService`: `List<MatchTrainingPuppetModel> TrainingPuppets` + `AddTrainingPuppet` / `RemoveTrainingPuppet` / `GetTrainingPuppet`, cleared in `ClearAll()`.
- `MatchFullTickPacketsHandler`: `UpdateTrainingPuppetsTransform(simulationState)` next to `UpdateScoreGatesTransform`.
- New `UpdateTrainingPuppetsTransformCommand` (copy the score-gate one), registered in `ClientMatchPresentationTickProcessor.ManagedUpdate()`.

### 8.3 Events on the client

- `ProcessTrainingPuppetHitEvents` in `PresentationMatchNetEventsHandler` (receive-time model write): `GetTrainingPuppet(id).Health = netEvent.RemainingHealth;` then cache the event.
- `HandleTrainingPuppetHitNetEventsCommand` (read-only): `SetHealth(...)` on the controller, `PlayHitReaction()`, pop the damage number through the existing `IHitDamageIndicatorEffectController` at `netEvent.HitPosition` (skip the number when `Damage == 0` — a push with no damage still shakes the puppet but must not print "0"), play the hit SFX, `events.Clear()`.
- **The purchase needs no new event**: `HandleTalentCardObtainedNetEventsCommand` already runs on `TalentCardObtainedNetEventS2C`. Extend `PresentationMatchNetEventsHandler.ProcessTalentCardObtainedEvents` to also set `HasSpentFreeMarketShot = true` on that player's model (receive-time write, per the CLAUDE.md split), and the command to update the HUD (§8.4).

### 8.4 HUD

- `MatchPlayerUIControllers.AddPlayer` — `case StageType.FreeMarket:` hides the health bar (players are invulnerable) and shows **no** score slot. Optionally show a small "shot available / spent" pip; recommended, and it is the only feedback a player has for why his fire button stopped working (§12).
- Countdown: gate `UpdateMatchTimerCountdownCommand` on `StageType.HasStageCountdownTimer()` rather than `IsBonusStage()`.
- Team board: keep the bonus-score row gated on `IsBonusStage()` — FreeMarket shows no scores.
- Lock-on reticle art: cards and puppets go through the existing `LockOnTargetSight` / `PlayersLockOnTargetEffectControllers` path, which is target-type agnostic — verify it does not switch on `LockOnTargetType` anywhere before assuming it is free.

### 8.5 Rejoin / full-state sync

`SyncMatchSimulationStateCommand`:
- `DestroyAll()` → `_trainingPuppetControllers.DestroyAll();`
- `CreateAll()` → `CreateTrainingPuppets(mapSizeMultiplier)` from the snapshot's `TrainingPuppets` list, applying each puppet's `Health`/`MaxHealth` immediately so a rejoining client sees the drained bars rather than three fresh ones.
- Talent cards are already recreated from the snapshot — verify the FreeMarket card art/lock-on state comes back correctly.
- Restore the per-player `HasSpentFreeMarketShot` into the client model so a rejoining player's HUD matches the server.

**Acceptance (D):** A client sees 3 puppets drifting and spinning smoothly as they are hit, reticles on cards and puppets, a talent card popping into his talent bar on purchase, his shoot indicator turning spent, the countdown running, and a mid-stage rejoin restoring puppets, cards, talents, spent-shot state and remaining time.

---

## 9. Work Package E — Stage authoring

### 9.1 Authoring components

New `Shared/LevelEnvironment/Scripts/TalentCardSpawnPoint.cs`:
```csharp
public class TalentCardSpawnPoint : MonoBehaviour
{
    public ushort Id;
    public TalentType TalentType;   // used as-is when FreeMarketConfig.ShouldRandomizeCardTalents is off
#if UNITY_EDITOR
    private void OnDrawGizmos()     // draw the card footprint from TalentsInnerConfig width/height
#endif
}
```
New `TrainingPuppetSpawnPoint.cs` — position-only, gizmo circle of `FreeMarketConfig.TrainingPuppetRadius` (copy `MoleSpawnPoint`).

`EnvironmentGenerator` gains two serialized lists and two `[Button]` bake methods, `RefreshTalentCards(int index)` and `RefreshTrainingPuppetSpawnPoints(int index)`, copying `RefreshScoreGates` exactly. **`RefreshTalentCards` is the first talent-card bake button in the project** — DeathMatch layouts got their `_talentCardsJson` some other way, so verify the existing layouts' JSON round-trips through the new button before re-baking any of them.

### 9.2 The layout

Author layout index **24** (or the next free index — check `EnvironmentConfig._environmentLayoutConfigs`) on `Shared/LevelEnvironment/Assets/Environment.prefab`:
- `_environmentHalfSizeJson`, `_wallsJson`, `_stageBoundriesWallsJson` — a plain closed arena; a shop needs no hazards.
- `_fieldBarriersJson` — at least `MaxTeamsAmount` barriers, or teams share a spawn (`InitStageCommand.SetupPlayers` spawns each team at its barrier).
- `_cameraBoundariesJson` — via the existing `SaveCameraBoundaries(index)` button.
- `_talentCardsJson` — the scattered cards, spread so no two are close enough to be locked at once from a normal approach (the closest-card rule exists for when they are, but the common case should be unambiguous).
- `_trainingPuppetSpawnPointsJson` — at least `TrainingPuppetsAmount` points, placed in open space away from the cards so practising does not knock a puppet into the shop.
- Leave lava, spikes, teleport gates, rotating wheels and gate traps empty. No power-up spawn points either (a nuke in the shop is chaos with no upside; §12).

Then: add `24` to `EnvironmentConfig.FreeMarketLayoutIndexes` and set `SimulationGamePlayInnerConfig.DefaultFreeMarketEnvironmentId = 24`.

**Acceptance (E):** With `ShouldChooseRandomStage` on or off, a FreeMarket stage loads the shop arena, every team spawns inside it, the camera frames it, the authored cards are there and exactly `TrainingPuppetsAmount` puppets stand at authored positions.

---

## 10. Cross-cutting checklist / gotchas

- [ ] **`/coding-guidlines` followed** — zero-alloc, commands, `Get`≠mutate, `Try` prefixes, unit-suffixed time fields, no magic numbers, method ≤30 / class ≤200 lines.
- [ ] **`FreeMarket` is NOT in `IsBonusStage()`** — it must never award gems or show a score HUD. Every existing `IsBonusStage()` call site was re-read and moved to `HasStageCountdownTimer()` / `IsPlayerDamageDisabled()` where that is what it meant.
- [ ] **`MatchDataService._didntPlayYetStageIndexesPerStageType` has a `FreeMarket` entry** — otherwise the first random FreeMarket layout roll throws `KeyNotFoundException`.
- [ ] **Every `new MatchSimulationStateS2C(` call site updated** for the `maxTrainingPuppets` argument (grep it).
- [ ] **Lock-on capacity grown** in both `MaxCap.ConcurrentLockOnTargets` and the `PlayerStateS2C` construction inside `MatchSimulationStateS2C` — cards **and** puppets.
- [ ] **`ClearObjectStates()` clears `TrainingPuppets`** — the next stage must have zero puppet bodies and zero puppet views.
- [ ] **`HasSpentFreeMarketShot` is reset in `InitStageCommand.SetupPlayers`** — otherwise a player who bought in stage 1 can never shoot again for the whole match.
- [ ] **`HasSpentFreeMarketShot` is in full `Serialize`/`Deserialize` + `GetClone`, not in the deltas.**
- [ ] **All three shooting paths are gated** (bullet shoot, lock-on shoot, lock-on targeting) — a lingering reticle on a player who cannot shoot is a bug report waiting to happen.
- [ ] **The card grant goes through `TryObtainTalentCardCommand` only** — both the bullet path and the lock-on path, so the spent-shot flag can never be set in two places.
- [ ] **Closest-card tie-break is deterministic** (lower id wins) and the *losing* card's lock-on timer is still reset.
- [ ] **The shot is spent on any landed card hit**, including the already-owned case (card survives, nothing granted) and the at-max case (a talent is replaced). Only a shot that hits no card leaves the flag alone.
- [ ] **Cards whose talent the caster already owns are filtered out of his lock-on targets** — otherwise the normal flow hands him a reticle on a card that can only cost him his shot.
- [ ] **Puppet damage is decorative** — `Health` floors at 0 and nothing else happens: no destroy path, no removal, no death event.
- [ ] **Every puppet hit goes through `TryHitTrainingPuppetCommand`** (push + damage + net event in one place), and pushes with no damage pass `Damage = 0` rather than skipping the event.
- [ ] **Puppet damage numbers match the player numbers** each call site would pass to `TryHitPlayerCommand`.
- [ ] **`FishingRodCaughtEnemyType.TrainingPuppet` is handled everywhere the enum is switched on**, server and client (`grep FishingRodCaughtEnemyType`) — the caught phase must follow the puppet's *moving* position, unlike the mole branch.
- [ ] **New net event uses `eventMask2` bit 15** (bits 0–14 taken). Same bit in `CalculateEventMask2`, `Serialize`, `Deserialize`.
- [ ] **`StopSavingClientEvents` clears, pools and `Remove(clientId)`s** the new per-client list — a missing `Remove` leaks the dictionary entry.
- [ ] **Every row of the §7.5 matrix is implemented or explicitly marked "free/N-A"** — sensors give **no** solver impulse and cast talents give **no** collision event at all.
- [ ] **All puppet impulses go through `PushTrainingPuppetCommand`** and scale by `body.GetMass()` / `GetInertia()`, so re-tuning the mass does not invalidate every talent's numbers.
- [ ] **Player velocity is state-authoritative** — `HandlePlayerTrainingPuppetCollision` must reflect the player's velocity in state, or players slide into puppets.
- [ ] **`StageEndedCommand` no longer NREs with `winningTeamId == 0`** and the client's existing `WinningTeamId == 0` path is verified end to end.
- [ ] **`PhysicsCollisionType` has 31 usable bits** — `TrainingPuppet = 17` is fine; do not add speculative channels.
- [ ] **Presentation write/read split** — puppet and card model mutations in `PresentationMatchNetEventsHandler`, never in a `Handle*NetEventsCommand`.
- [ ] **Config assets need a human pass**: `SimulationGamePlayConfig.asset` (FreeMarket flag, stage numbers, `FreeMarketConfig`, default layout id), `NetworkConfig.asset` (`ConcurrentTrainingPuppets`, the new net-event cap, lock-on cap), `EnvironmentConfig` (`FreeMarketLayoutIndexes` + the layout), `CoreAudioClips.asset` (puppet hit, purchase), `GamePlayMatchScene` (puppet prefab binding).
- [ ] **Puppet mass feel** ("heavier than a player, pushable, spinnable") is a play-test decision — ship the defaults and flag it in the PR.

---

## 11. Suggested manual test plan

1. Set `FreeMarketStageNumbers = [1]` → the match opens on FreeMarket. Verify the shop arena, the countdown, invulnerable players, no team score anywhere.
2. Lock on a card → reticle arms → shoot → the talent lands in your bar, the card disappears, the reticle set empties, and further shooting does nothing for the rest of the stage.
3. **Edge case:** line up so two cards are locked at once, shoot → only the **closer** card is bought; the other is still standing and buyable by someone else.
4. A card whose talent you already own gives you **no reticle**; bullet one by hand anyway → the card survives, no talent is granted, and your shot is gone.
5. Buy while already holding `MaxConcurrentTalentsForPlayer` talents → the currently selected talent is replaced, the shot is spent, and the client shows the swapped talent bar.
6. Practise on the puppets with the freshly bought talent — walk the whole §7.5 matrix: ram, Rock, KO, Headbutt, MagneticPull, YearsOfPain, WaterGun, FrigidBlock, bullets/SentryGun, GrapplingHook, FishingRod (catch → throw), Soul, Chicken egg, nuke. Every hit pops a damage number and drains the bar.
7. Empty a puppet's bar → it sits at 0 and keeps taking pushes exactly as before; nothing is destroyed and no error is logged.
8. Confirm a puppet is **visibly heavier** than a player: same ram sends a player much further than a puppet.
8. Let the countdown expire → no winner UI, no gems, no camera zoom, the match rotates into stage 2 as a normal DeathMatch.
9. Rejoin mid-FreeMarket → cards, puppets, your talents, your spent-shot state and the remaining time are all correct.
10. Scheduling: set `FreeMarketStageNumbers = [4]` with `BonusStageEveryXStages = 4` → stage 4 is FreeMarket (not a bonus stage) and stage 8 is a bonus stage.
11. Regression: DeathMatch talent cards still take multiple bullets and never lock the shooter's gun; Whac-A-Mole and GatePass are untouched.

---

## 12. Decisions taken (do not re-open during implementation)

| # | Question | Decision |
|---|---|---|
| 1 | Buying at max talents | The purchase goes through (the currently selected talent is replaced) and **the shot is spent**. §6.4 |
| 2 | Buying a talent you already own | Nothing is granted, the card survives, **the shot is still spent**. Lock-on filters those cards out for that player so the normal flow cannot waste a shot on one. §6.2, §6.4 |
| 3 | Puppet durability | **Indestructible**, with a **decorative 100-health bar** that drains on hits and floors at 0 — nothing happens when it empties. Hits pop the normal hit indicator. §7.1, §7.5.0, §8.1 |
| 4 | FishingRod vs a puppet | **Player-like**: the tip catches the puppet and the second cast throws it, reusing the existing `FishingRodCaughtEnemyType` shape. §7.6 |
| 5 | Card talent rolls | **Dumb random** per stage, from the full talent pool, no filtering against what players already own. §6.1 |
| 6 | FreeMarket vs bonus-stage cadence | **FreeMarket wins**; the bonus stage happens on its next multiple. §5.4 |

Still worth a designer's eye at play-test time, but not blocking:

- **Feedback for a spent shot** (§8.4) — a pip on the player UI is recommended; without it the fire button just stops working with no explanation.
- **Swap vs a puppet** — currently no interaction (a puppet is not a player). A separate feature if it is ever wanted.
- **Power-ups in the shop** — the spec authors no spawn points. The trade-off is chaos versus being the only place to practise a nuke.
- **Stage duration** (`25s`) and **puppet mass** (`4`) are pure feel values.
