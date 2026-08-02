# BattleForce — Codebase Guide

## What this project is

BattleForce is a multiplayer 2D spaceship arena game built in Unity. It runs a **server-authoritative, tick-based simulation** at 60 ticks/second. The server runs the full physics simulation and game logic; clients receive state snapshots and net events each tick and run only presentation logic.

The working directory for all code work is `Assets/`. All paths below are relative to it.

---

## Domain structure

All game code lives under `Core/Game/Domains/GamePlay/` and is split into three domains:

```
Core/Game/Domains/GamePlay/
├── Simulation/   — server-side physics, game rules, talent logic, input processing
├── Presentation/ — client-side rendering, UI, visual effects, MVC controllers
└── Shared/       — types used by both: S2C/C2S network models, enums, configs
```

`Core/Scripts/` and `Core/Game/Scripts/` contain cross-cutting framework infrastructure (DI installers, services, utilities) under the `CoreDomain` namespace.

---

## Simulation domain

**Entry point:** `Simulation/Match/Scripts/Initiator/ServerMatchEntryPointCommand.cs`
**Tick driver:** `Simulation/Match/Scripts/NetworkManager/TickHandlers/ServerMatchNetworkTickProcessor.cs`

Zero grabage allocation. Allocate all the data needed in advance inside entry points and not during the gameplay.

Each tick the server:
1. Processes incoming client input packets
2. Steps the Box2D physics simulation (`IPhysicsSimulator`)
3. Runs talent logic (`IPlayersTalentsManager` → `PlayerTalentControllers` → individual `ITalentController` impls)
4. Resolves collisions (`ProcessCachedCollisionsCommand`)
5. Builds a `MatchFullTickPacketS2C` per connected client and sends it

**Physics:** `Simulation/Scripts/Physics/IPhysicsSimulator.cs` — thin interface over Box2D.NetStandard. Entity IDs are `ushort`. All physics shapes (players, bullets, walls, talent projectiles) are added/removed through this interface.

**Match state:** `Shared/Scripts/S2CModels/MatchSimulationStateS2C.cs` — the single source of truth for all in-game state. Contains `FixedClassUnorderedList<PlayerStateS2C>`, bullets, talent cards, projectiles, swap fields, etc. State is serialised directly into tick packets for rejoin sync.

**Talents:** Each talent is a class implementing `ITalentController` in `Simulation/Match/Scripts/Talent/TalentController/`. All controllers for one player live in `PlayerTalentControllers`. See `/add-talent` skill.

**Configuration:** ScriptableObjects loaded via `ISimulationGamePlayConfigService`:
- `Core/Scripts/Network/NetworkConfig.cs` — tick rate, MaxCap (capacity limits for all entity types and net event lists), server address
- `Simulation/Scripts/Configurations/TalentsConfig.cs` — all talent tuning and cooldown configs
- `Shared/Scripts/Configs/SharedGamePlayConfig.cs` — shared limits (max players, max talents per player, team IDs, environment settings)

---

## Presentation domain

**Entry point:** `Presentation/Match/Scripts/TickProcessor/ClientMatchPresentationTickProcessor.cs`. Or use the InitEntry/ExitPoints of the framework. No Update/OnDestroy/Awake/Etc.
**Tick driver:** `ManagedUpdate()` on that class, called by Unity's update loop each frame

Each frame the client:
1. `MatchFullTickPacketsHandler` receives tick packets, filters stale events by tick, copies events to `_cachedUnprocessed*` lists, and calls `PresentationMatchNetEventsHandler` methods to populate `ICachedPresentationEventsService`
2. `ClientMatchPresentationTickProcessor.ManagedUpdate()` executes one `Handle*NetEventsCommand` per event type — each command reads from the cache, updates MVC controllers, and calls `events.Clear()`
3. MVC controllers update Unity GameObjects (transforms, animations, VFX)

**MVC pattern:** Every visual entity (player, bullet, talent projectile, environment object) has a `View` (MonoBehaviour with references to renderers/animators), a `Controller` (pure C# logic), and a `Controllers` collection managing pooling. Bound in `Presentation/Match/Scripts/ZenjectInstallers/GamePlayMatchInstaller.cs`.

**Cached events service:** `Presentation/Scripts/PresentationEvents/CachedPresentationEventsService.cs` — simple `List<T>` per event type, bridging the packet handler and the command layer.

### Who writes to `IMatchDataService`

`IMatchDataService` (`Presentation/Match/Scripts/DataService/`) is the client-side mirror of the match state. There is a strict split between the two layers that touch it:

| Layer | Allowed to |
|---|---|
| `PresentationMatchNetEventsHandler` (step 1, packet-receive time) | **Write** — add/remove/mutate the client match data, then cache the net event |
| `Handle*NetEventsCommand` (step 2, `ManagedUpdate`) | **Read only** — resolve MVC controllers, play SFX/VFX, `Clear()` the cache |

**Rule:** every mutation of the client match model belongs in the matching `Process*Events` method of `PresentationMatchNetEventsHandler`, next to the `_cachedPresentationEventsService.X.Add(netEvent)` line. A `Handle*NetEventsCommand` may resolve `IMatchDataService` to *read* a model, but must not add, remove, or mutate one — if a command has no other reason to hold `IMatchDataService`, drop the dependency entirely.

**Why:** packets are received and drained at a different point in the frame than the commands that consume them, and several packets can arrive before a single `ManagedUpdate`. Keeping writes at receive time means the model is always up to date by the time any command — including commands for *other* event types, and per-frame transform/update commands — reads it. It also keeps a single, greppable place per entity where its client-side lifetime is defined, instead of splitting "add" into the handler and "remove" into a command.

Examples of the split:
- `ProcessFishingRodCaughtEnemyEvents` sets `GetFishingRodTip(id).Phase = FishingRodTipPhase.CaughtEnemy`; `HandleFishingRodCaughtEnemyNetEventsCommand` then only *reads* that tip to spawn the aim arrow and stop the reel SFX.
- `ProcessDeactivateSoulTalentEvents` calls `RemoveSoulGhost(id)`; `HandleDeactivateSoulTalentNetEventsCommand` only calls `DestroySoulGhost(id)` on the controllers.
- `ProcessDestroyFrigidBlockEvents` calls `RemoveFrigidBlock(id)`; `HandleDestroyFrigidBlockNetEventsCommand` only destroys the view.

**Exception:** `SyncMatchSimulationStateCommand` writes to `IMatchDataService` by design — it is the full-state/rejoin sync path, not a net-event handler, and rebuilds the whole client model from a state snapshot.

---

## Net event system (S2C)

Net events carry discrete occurrences from server to client inside `MatchFullTickPacketS2C`. Each tick packet has a `ulong` bitmask; only non-empty event lists are serialised.

**Key files:**
- `Shared/Scripts/S2CModels/MatchFullTickPacketS2C.cs` — declares all event list fields, serialize/deserialize, event mask
- `Simulation/Scripts/NetworkManager/INetEventsDataService.cs` + `NetEventsDataService.cs` — server-side per-client event queues with `ConcurrentPool<FixedUnorderedList<T>>`
- `Presentation/Match/Scripts/Network/PacketsHandlers/MatchFullTickPacketsHandler.cs` — client-side unpacking

To add a new net event: use the `/add-net-event` skill.  
To add a new talent (which defines its own events): use the `/add-talent` skill.

---

## Dependency injection

**Framework:** Zenject (`Core/Plugins/Zenject/`)  
**Root installer:** `CoreDomain/Scripts/ZenjectInstallers/CoreInstaller.cs` — binds framework services (logger, scene loader, audio, command factory, update service, state machine)  
**Match installer:** `Presentation/Match/Scripts/ZenjectInstallers/GamePlayMatchInstaller.cs` — binds all presentation MVC controllers and injects view prefabs  
**Simulation installer:** `Simulation/Scripts/ZenjectInstallers/` — binds simulation services

`ICommandFactory` is bound `CopyIntoAllSubContainers` so commands can be created anywhere in the hierarchy. Commands extend `BaseCommand` and call `_diContainer.Resolve<T>()` in `ResolveDependencies()`.

---

## MatchMaking domain

`Presentation/MatchMaking/` mirrors the Match domain structure but for the pre-game lobby phase. Players can move around a preview of the arena. Uses `MatchMakingFullTickPacketS2C` and its own tick processor and net events handler.

---

## Key data types

| Type | Purpose |
|---|---|
| `FixedUnorderedList<T>` | Fixed-capacity value-type list (no GC, O(1) remove via swap-and-pop) |
| `FixedClassUnorderedList<T>` | Same but for reference types — pre-allocates instances via factory lambda |
| `FixedOrderedList<T>` | Fixed-capacity sorted list (used for bullets, requires `IComparable`) |
| `CapacityDict<K,V>` | Fixed-capacity dictionary |
| `CapacityList<T>` | Resizable list with capacity hint |
| `ConcurrentPool<T>` | Thread-safe object pool — used by `NetEventsDataService` for per-client event lists |

All network types implement `INetSerializable` with `Serialize(NetDataWriter)` / `Deserialize(NetDataReader)`.

---

## Common simulation patterns

**Spinning a player:** Always use `SpinPlayerCommand` — never write to `AngularVelocity` directly. It handles the `IsSpinned` flag and fires `AddPlayerSpinnedStartedNetEvent` internally. Create it in the constructor via `commandFactory.CreateCommandVoid<SpinPlayerCommand>()`, call as `.SetPlayer(id).SetSpinAmount(signedAmount).SetTick(tick).Execute()`. The spin amount is signed — positive and negative values spin in opposite directions.

**Applying force to a player:** Use `AddForceToPlayerCommand` — never write to `Velocity` directly. It adds to the player's current velocity (`Velocity +=`) and optionally turns off the engine. Create in constructor via `commandFactory.CreateCommandVoid<AddForceToPlayerCommand>()`, call as `.SetPlayerId(id).SetForce(direction * magnitude).ShouldTurnOffEngine(false).Execute()`.

**Random direction (Vector2):** `RNG.NextFloat(0f, 360f).AngleToVector()` — `AngleToVector` is a `float` extension in `Core/Scripts/Extensions/FloatExtensions.cs` that returns `System.Numerics.Vector2`. Passing 0–360 produces a uniformly distributed random direction (same pattern used by `InitStageCommand`).

---

### NAMING CONVENTIONS

* **Prefixes:** `I`Interface, Enum`Type`, `_`privateField, `On`EventHandler, `Try`Method (if early exit).  
* **Suffixes:** `Event` (for Events). Append the data structure type (e.g., `OffersDictionary`, `EnemyArray`, `WindowPrefab AnimationCancellationToken`). `Coroutine` (for Unity Coroutines). `Async` (For Task/UniTask).   
* **Casing:** `ALL_CAPS` (Constants), `PascalCase` (Properties/Enums), `camelCase` (Locals).  
* **Booleans:** Prefix with question words (`is`, `can`, `has`).  
* **Specificity:** No abbreviations (`Pref`, `GO`, `Pos`). Include time units in names (e.g., `cooldownInSeconds`).  
* **Comments:** Code must self-document via long/clear names. Avoid comments, if you have no choice then use Comments to explain *why*, not *what*.

---

## Skills available

- `/add-net-event` — add a new S2C net event (7-file checklist, bitmask registration)
- `/add-talent` — add a new talent (enum, config, controller, registration + per-talent net events)
- `/add-power-up` — add a new PowerUp type (enum, controller, PlayerPowerUpControllers registration, config + optional activation net event)
- `/coding-guidlines` — coding rules to follow when writing code

