---
name: add-talent
description: Add a new Talent to BattleForce. Use when asked to add a talent, implement a new ability, or create a new talent type.
---

Every talent requires these 5 steps. Beyond them, each talent defines its own net events and presentation wiring — use `/add-net-event` for each one, then write presentation commands to match.

Reference controllers to read before starting:
- `SentryGunTalentController` — activate/deactivate with duration, modifies player state
- `DashPulseTalentController` — single "perform" event, stocks-based cooldown, no `IsCurrentlyActive`
- `YearsOfPainTalentController` — aim-then-release input, fires once, immediate cooldown
- `SwapTalentController` — spawns a world object, collision callback from outside
- `GrapplingHookTalentController` — spawns a projectile with per-tick physics

---

## The 5 common steps

### 1. Add to `TalentType` enum
**File:** `Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/TalentType.cs`

Use the next integer:
```csharp
NewTalent = Next Integer,
```

---

### 2. Create the talent config
**File:** `Core/Game/Domains/GamePlay/Simulation/Scripts/Configurations/Talents/NewTalentConfig.cs`

```csharp
using System;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations.Talents
{
    [Serializable]
    public class NewTalentConfig
    {
        // add talent-specific tuning parameters
    }
}
```

---

### 3. Add config field to `TalentsConfig` AND `TalentsInnerConfig`
Both files have identical field lists — add to **both**:

- `Core/Game/Domains/GamePlay/Simulation/Scripts/Configurations/TalentsConfig.cs`
- `Core/Game/Domains/GamePlay/Simulation/Scripts/Configurations/TalentsInnerConfig.cs`

```csharp
public NewTalentConfig NewTalentConfig;
```

Also add a cooldown entry in the Unity Inspector on the `TalentsConfig` ScriptableObject asset: open `TalentsCooldownsConfigs` (the `[SubclassList]` field) and add a `TalentNormalCooldownConfig` or `TalentStocksCooldownConfig` for `TalentType.NewTalent`. **If this entry is missing the cooldown will be 0 — the talent fires every frame.**

---

### 4. Create the Talent Controller
**File:** `Core/Game/Domains/GamePlay/Simulation/Match/Scripts/Talent/TalentController/NewTalentController.cs`

Implement `ITalentController`:
```csharp
public interface ITalentController
{
    TalentType TalentType { get; }
    void ProcessTalentInput(bool wasTalentInputDownThisTick, bool isTalentInputPressed,
        bool wasTalentInputReleasedThisTick, int tick, float deltaTime);
    void StopIfActive(int tick);   // called when player gets spun
    void OnTick(int tick, float deltaTime);
    void ResetData();
}
```

Use `_matchDataService.SimulationState.GetIsTalentCurrentlyActiveForPlayer` / `SetIsTalentCurrentlyActiveForPlayer` to track active state in simulation state (so it survives rejoin sync). Only needed if the talent has a duration — instant talents (DashPulse, YearsOfPain) skip this entirely.

**Cooldown pattern** (for normal-cooldown talents, copy from SentryGun/Umbrella):
```csharp
if (!casterPlayerState.Spaceship.TalentsState.TryGetTalentIndexByType(TalentType.NewTalent, out int talentIndex))
{
    LogService.LogError($"No NewTalent for player {_casterPlayerId}");
    return;
}
ref var talentModel = ref casterPlayerState.Spaceship.TalentsState.Talents.Get(talentIndex);
var cooldownEndTick = TickUtils.GetTickPassedAfterDuration(tick, talentModel.NormalCooldown.MaxCooldown, _networkConfig.DeltaTime);
talentModel.NormalCooldown.CooldownEndTick = cooldownEndTick;
```

For stocks-based cooldown, see `DashPulseTalentController` — it reads `StocksCooldown.CurrentStocksAmount` and decrements it.

---

### 5. Register in `PlayerTalentControllers`
**File:** `Core/Game/Domains/GamePlay/Simulation/Match/Scripts/Talent/PlayerTalentControllers.cs`

Four changes:

**a) Field:**
```csharp
private readonly NewTalentController _newTalentController;
```

**b) Constructor** (instantiate, passing whatever dependencies the controller needs):
```csharp
_newTalentController = new NewTalentController(netEventsDataService, matchDataService, gamePlayConfigService, networkConfig);
```

**c) `SetCasterId()`:**
```csharp
_newTalentController.SetCasterId(casterPlayerId);
```

**d) `GetTalentByType()` switch:**
```csharp
case TalentType.NewTalent: return _newTalentController;
```

`OnTick` and `ResetData` are already called explicitly per-controller in `PlayerTalentControllers` — add calls there too:
```csharp
// in OnTick():
_newTalentController?.OnTick(tick, deltaTime);

// in ResetData():
_newTalentController.ResetData();
```

If your talent needs an external trigger (collision callback, like Swap's `CompleteSwapTalentWithEnemy` or GrapplingHook's `HitGrapplingHookWithWall`), add a method to `PlayerTalentControllers`, expose it on `IPlayersTalentsManager`, and call it from wherever the collision is detected.

---

## Net events: decide per talent, then use `/add-net-event`

Net events vary entirely by talent — there is no single pattern. Look at existing talents:

| Talent | Net events |
|---|---|
| DashPulse | `PerformDashPulseNetEvent` — one "fired" event, no activate/deactivate |
| SentryGun / Umbrella | `Activate` + `Deactivate` (with cooldown end tick) |
| YearsOfPain | Single event carrying direction + hit result + cooldown end tick |
| Swap | `CreateSwapField` + `DeactivateSwapTalent` (field lifecycle) |
| GrapplingHook | `CreateProjectile` + `HitWall` + `Deactivate` (projectile lifecycle) |

For each net event your talent needs, run the full `/add-net-event` checklist. After each event is wired, write the corresponding presentation-side pieces:
- A `HandleXxxNetEventsCommand` in `Commands/NetEvents/` — reads from `ICachedPresentationEventsService`, acts, clears the list
- A `ProcessXxx` method on `PresentationMatchNetEventsHandler` — filters by tick, adds to cache
- A `private void ProcessXxx` method on `MatchFullTickPacketsHandler` — copies from packet to `_cachedUnprocessedXxx`, calls `_presentationNetEventsHandler.ProcessXxx`
- Field + constructor init + `Execute()` call in `ClientMatchPresentationTickProcessor`

## Gotchas

- **`TalentType` integer must be unique** — check the enum before assigning a value.
- **Both `TalentsConfig` and `TalentsInnerConfig` must be updated** — they have identical field lists and both are used.
- **Cooldown config must exist in the Inspector asset** — missing `TalentsCooldownsConfigs` entry = cooldown 0.
- **`StopIfActive` is called when the player gets spun** — clean up physics bodies and data service entries here. Leave it empty if the talent is instantaneous (DashPulse, YearsOfPain).
- **`IsCurrentlyActive` state lives in `SimulationState`**, not a local field — this ensures it's included in rejoin snapshots.
