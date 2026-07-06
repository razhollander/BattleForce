---
name: add-power-up
description: Add a new PowerUp type to BattleForce. Use when asked to add a power-up, implement a new consumable pickup ability, or create a new PowerUpType.
---

Every new PowerUp requires the 5 core steps below. If the PowerUp needs a custom activation net event (like SonicSlap does), run `/add-net-event` for it and then wire the 4 presentation pieces described at the end.

Reference implementation to read before starting:
- `SonicSlapPowerUpController` — the only PowerUp currently. Reads all enemies, flips their velocity/direction, fires a custom net event with affected player IDs.

**How PowerUps differ from Talents:**
- One PowerUp per player at a time, stored directly in `PlayerSpaceshipStateS2C.Spaceship.CurrentPowerUp`.
- No cooldown, no stocks, no aim state — the PowerUp is consumed immediately on use.
- The ball-spawn / ball-obtain / player-powerup-changed events are **already wired** — you only need to add activation-specific events for the new PowerUp's effect.
- `PlayerPowerUpChangedNetEvent` fires automatically from `PlayersPowerUpsManager` whenever a PowerUp is granted or cleared — no controller code needed for that.

---

## The 5 core steps

### 1. Add to `PowerUpType` enum
**File:** `Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PowerUpType.cs`

Use the next integer:
```csharp
NewPowerUp = 2, // next after SonicSlap = 1
```

---

### 2. Create the PowerUp controller
**File:** `Core/Game/Domains/GamePlay/Simulation/Match/Scripts/PowerUp/PowerUpController/NewPowerUpController.cs`

Implement `IPowerUpController`:
```csharp
public interface IPowerUpController
{
    PowerUpType PowerUpType { get; }
    void SetCasterId(ushort casterPlayerId);
    void Perform(int tick);
}
```

SonicSlap pattern — instantaneous effect on all enemies:
```csharp
using Core.Game.Domains.GamePlay.Shared.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Scripts.Network;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PowerUp.PowerUpController
{
    public class NewPowerUpController : IPowerUpController
    {
        private readonly IMatchDataService _matchDataService;
        private readonly INetEventsDataService _netEventsDataService;
        private ushort _casterPlayerId;

        public PowerUpType PowerUpType => PowerUpType.NewPowerUp;

        public NewPowerUpController(IMatchDataService matchDataService, INetEventsDataService netEventsDataService, NetworkConfig networkConfig)
        {
            _matchDataService = matchDataService;
            _netEventsDataService = netEventsDataService;
        }

        public void SetCasterId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
        }

        public void Perform(int tick)
        {
            var casterTeamId = _matchDataService.SimulationState.GetPlayerById(_casterPlayerId).TeamId;

            foreach (var playerState in _matchDataService.SimulationState.Players.AsSpan())
            {
                if (playerState.TeamId == casterTeamId) continue;
                // apply effect to enemy playerState here
            }

            _netEventsDataService.AddNewPowerUpActivatedNetEvent(tick, _casterPlayerId /*, ...*/);
        }
    }
}
```

If the effect needs a pre-allocated collection (like SonicSlap's `_cachedAffectedPlayerIds`), allocate it in the constructor with the appropriate `MaxCap` value — **never allocate during `Perform`**.

---

### 3. Register in `PlayerPowerUpControllers`
**File:** `Core/Game/Domains/GamePlay/Simulation/Match/Scripts/PowerUp/PlayerPowerUpControllers.cs`

Four changes:

**a) Field:**
```csharp
private readonly NewPowerUpController _newPowerUpController;
```

**b) Constructor:**
```csharp
_newPowerUpController = new NewPowerUpController(matchDataService, netEventsDataService, networkConfig);
```

**c) `SetCasterId()`:**
```csharp
_newPowerUpController.SetCasterId(casterPlayerId);
```

**d) `GetPowerUpByType()` switch:**
```csharp
case PowerUpType.NewPowerUp: return _newPowerUpController;
```

---

### 4. Add to `ObtainablePowerUps` in the simulation config
**File:** `Core/Game/Domains/GamePlay/Simulation/Scripts/Configurations/PowerUpsConfig.cs` (ScriptableObject)

The `ObtainablePowerUps` array is the whitelist of types that can be randomly spawned on the field. Open the `PowerUpsConfig` ScriptableObject asset in the Unity Inspector and add `NewPowerUp` to the array. If you skip this the new type can never be picked up in-game.

---

### 5. Add sprite to the presentation config
**File:** `Core/Game/Domains/GamePlay/Presentation/Scripts/ScriptableObjects/PowerUpsConfig.cs` (ScriptableObject)

Open the `PowerUpsConfig` ScriptableObject asset in the Unity Inspector (Presentation version) and add a `PowerUpType → Sprite` entry for `NewPowerUp`. This sprite appears on the player's HUD when they hold the PowerUp. The `HandlePlayerPowerUpChangedNetEventsCommand` already reads this mapping — no code change needed.

---

## Activation net event (if the PowerUp has a visual/audio effect)

SonicSlap fires a `SonicSlapActivatedNetEventS2C` to tell clients to play a sound. Do the same for any PowerUp whose effect needs client-side feedback.

### Step A — Run `/add-net-event`
This covers the 7-file checklist: the S2C model, `MaxCap` capacity, `MatchFullTickPacketS2C` (field + bitmask + serialize/deserialize), `INetEventsDataService` (property + method), `NetEventsDataService` (impl), `MatchFullTickPacketsHandler` (field + init + `Process*` private method), and `CachedPresentationEventsService` (list property + interface).

For a PowerUp activation event, use a **class** (not struct) only if it needs a collection (like SonicSlap's `AffectedPlayerIds`). Otherwise use a **struct**.

### Step B — Add `Process*` to `PresentationMatchNetEventsHandler`
**File:** `Core/Game/Domains/GamePlay/Presentation/Match/Scripts/Network/PacketsHandlers/PresentationMatchNetEventsHandler.cs`

Follow the `ProcessSonicSlapActivatedEvents` pattern:
```csharp
public void ProcessNewPowerUpActivatedEvents(CapacityList<NewPowerUpActivatedNetEventS2C> events)
{
    if (events.IsNullOrEmpty()) return;
    foreach (var netEvent in events)
        _cachedPresentationEventsService.NewPowerUpActivatedNetEvents.Add(netEvent);
}
```

Then call it from the tick-processing method in `MatchFullTickPacketsHandler` (the `/add-net-event` skill's step 6c).

### Step C — Create `HandleNewPowerUpActivatedNetEventsCommand`
**File:** `Core/Game/Domains/GamePlay/Presentation/Match/Scripts/Commands/NetEvents/HandleNewPowerUpActivatedNetEventsCommand.cs`

Follow the `HandleSonicSlapActivatedNetEventsCommand` pattern:
```csharp
using Core.Game.Domains.GamePlay.Presentation.Scripts.PresentationEvents;
using CoreDomain.Scripts.Services.AudioService;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Commands.NetEvents
{
    public class HandleNewPowerUpActivatedNetEventsCommand : BaseCommand, ICommandVoid
    {
        private ICachedPresentationEventsService _cachedPresentationEventsService;
        private IAudioService _audioService;

        public override void ResolveDependencies()
        {
            _cachedPresentationEventsService = _diContainer.Resolve<ICachedPresentationEventsService>();
            _audioService = _diContainer.Resolve<IAudioService>();
        }

        public void Execute()
        {
            if (_cachedPresentationEventsService.NewPowerUpActivatedNetEvents.Count == 0)
                return;

            _audioService.PlayAudio(AudioClipType.NewPowerUp, AudioChannelType.Fx);
            // apply any per-event visual feedback here

            _cachedPresentationEventsService.NewPowerUpActivatedNetEvents.Clear();
        }
    }
}
```

### Step D — Register in `ClientMatchPresentationTickProcessor`
**File:** `Core/Game/Domains/GamePlay/Presentation/Match/Scripts/TickProcessor/ClientMatchPresentationTickProcessor.cs`

Three changes following the `_handleSonicSlapActivatedNetEventsCommand` pattern:

**a) Field:**
```csharp
private readonly HandleNewPowerUpActivatedNetEventsCommand _handleNewPowerUpActivatedNetEventsCommand;
```

**b) Constructor init:**
```csharp
_handleNewPowerUpActivatedNetEventsCommand = commandFactory.CreateCommandVoid<HandleNewPowerUpActivatedNetEventsCommand>();
```

**c) Execute in `ManagedUpdate()`** (add next to `_handleSonicSlapActivatedNetEventsCommand.Execute()`):
```csharp
_handleNewPowerUpActivatedNetEventsCommand.Execute();
```

---

## Gotchas

- **`PowerUpType` integer must be unique** — check the enum before assigning a value.
- **Never allocate during `Perform()`** — pre-allocate any scratch buffers in the controller's constructor using `MaxCap` values from `NetworkConfig`.
- **`SetCasterId` is called before every `Perform`** — the controller is shared across all uses by the same player. No need to store it until `Perform`.
- **`ObtainablePowerUps` is the spawn whitelist** — forgetting to add your type here means it can never appear on the field, even though all other code is correct.
- **`PlayerPowerUpChangedNetEvent` fires automatically** — `PlayersPowerUpsManager.TryGrantRandomPowerUp` and `ProcessPowerUpInput` already handle sending this event and clearing `CurrentPowerUp`. Your controller only fires its own activation event.
- **No aiming for PowerUps** — `IsPlayerAimingPowerUp` always returns `false`. PowerUps are instant-use only. Do not add aiming logic.
- **Sprite must be in the Presentation `PowerUpsConfig`**, not the Simulation one — they are two separate ScriptableObjects with the same class name in different namespaces.
