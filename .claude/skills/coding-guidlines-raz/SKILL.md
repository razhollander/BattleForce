---
name: coding-guidlines-raz
description: code-guidlines conventions and architecture rules by Raz.
---
## **ENTRY POINTS**

* **Problem:** Too many entry points (Unity methods, input, UI, timers).  
* **Fix:** Kill them. Use **Single Entry Point Principle**.  
* **Rule:** One class rules entry.  
* **Unity Lifecycles:** No Start/Awake/OnEnable everywhere. Use ONE start point. Swap to Setup() / Init().  
* **Death Lifecycles:** No OnDestroy/OnDisable everywhere. Use ONE exit point. Swap to Terminate() / Destroy().  
* **Updates:** No Update/LateUpdate in random classes. Use UpdateSubscriptionService.

---

## **DEPENDENCIES**

* **No Circular Loops:** Logic flows one way. If Class A needs Class B AND Class B needs Class A → merge into ONE class.  
* **No Double Data:** Two classes must not hold same data state.

---

## **CLASS & METHODS**

### **Methods**

* **Short:** Under 30 lines.  
* **One Job:** Split big logic into small methods, even if used once.  
* **No Inner Regions:** Inside method \#region \= bad. Extract to new method.  
* **Outer Regions OK:** Only use \#region outside methods for clean file look.

### **Classes**

* **Short:** Under 200 lines.  
* **Split Views:** Big UI View → break into Inner Views.  
* **Tooltip Heavy?** Complex Tooltip → make it a View.  
* **No Mega-Utils:** No generic Utils. Use InputUtils, TimeUtils.  
* **Small Data:** Split giant data classes into small, specific containers.

---

## **ARCHITECTURE RULES**

* **Statics Bad:** Static classes hide dependencies. Avoid.  
* **Validate at Gate:** Public APIs and Entry Points must check for null/bad input.  
* **Trust Inside:** Once past the gate, internal code does zero validation. Trust data.
* **A branch that should be structurally unreachable logs an error before its early return, never returns silently.** Bonus-stage players have no health, so nothing should ever call the damage path on one - if something does, `LogService.LogError(...)` before the `return` is what surfaces the upstream bug; a silent early-return just hides it until the same mistake causes a harder one to debug.

---

## **CLEAN CODE PHILOSOPHY**

### **Definition**

* Code must read like simple book.

### **Rule of Thumb**

* No overkill. Do not burn down house to kill spider. (E.g., Do not hide all popups just to hide one active popup).

### **Garbage**

* No garbage allocation in the simulation domain! everything should be pre-allocated!

### **Single Responsibility**

* One class, one job, one reason to change.  
* Break big PopupsManager into small workers:  
  * **Data/Logic:** PopupModel, PopupsDataService, PopupsDataValidator, PopupsDataSerializer.  
  * **Actions:** PopupsAnalyticsService, PopupsAssetDownloader, PopupsViewFactory.  
  * **Control:** PopupController, PopupsManager, PopupViewController.  
  * **UI Views:** PopupHeaderPanelView, PopupItemsPanelView, PopupItemView.
* **Split the moment a second concern lands, not later.** Adding a feature to an existing service is the moment to ask whether it is the same job. A spawn timer that also tracks hole cooldowns and golden mole cadence is three services - split it in the same change that grew it, do not wait to be asked.  
* **Generalize a feature-specific name the moment a second feature reuses its data.** A field like `MolesHitScore` staying that name after a second stage type starts writing its own score into it means every one of that second stage's call sites reads as if it were about moles. Rename to the general concept (`StageScore`) in the same change that adds the second user, do not wait for a third.
* **The interface splits with the class.** Each new service gets its own `I...` beside it, and every consumer takes only the interface it actually calls. A consumer that calls one method out of six is the proof the interface holds two jobs.
* **The interface name and the class name must agree.** `IMolesSpawnerService` on a `MolesSpawnTimerService` means one of the two names is lying about the job - rename the pair to what the class actually does.

### **Get methods must not mutate**

* Get means look, no touch. No side effects. Safe to call 100 times.  
* If Get changes state → change name. (E.g., GetAndClear).
* **Seed lazily-initialised state at an init point, never on first read.** "Roll it if it has not been rolled yet" inside a query *is* the mutation - move the roll to the entry point or the stage init and the query becomes pure for free.
* **Then delete the "not set yet" sentinel.** A `-1` that existed only to trigger the lazy seed will otherwise leak into a real comparison and answer the query wrong on the first call.

---

## **CODE BUILDING BLOCKS**

### **1\. Utils**

* **What:** Static helpers.  
* **Rule:** Only static methods. Only talk to other `Utils` or `Extensions`.  
* **No-No:** No big brain logic. No Unity context jumping.

### **2\. Commands**

* **What:** Execute one thing.  
* **Rule:** Hold data, then `Execute()`. Good for complex, single-fire logic.  
* **Never repeat a sequence \- extract it to a Command.** The moment the same multi-step sequence appears in a second place, it becomes a `Command`. Two copies are already one too many: the second caller is the proof that the sequence is a concept with a name, not a local detail.  
* **A private helper method is only good for one class.** As soon as another class needs the same steps, do not copy the helper and do not make it `public`/`static` \- turn it into a `Command` and delete every copy.  
* **Extract the whole sequence, not part of it.** Every step that must happen together lives inside `Execute()`, so no caller can ever perform three of the four steps.  
* **Take ids, not models.** `SetXId(id)` over passing a state object, so the Command owns the lookup and every caller does the same thing.  
* **Clean up after extracting** \- dependencies that only the extracted code used (services, usings, fields) are now dead in the caller. Delete them (see *Finishing a Change*).  
* **Every setter a Command exposes must be called by every caller, or it should not exist.** An optional setter that gates core logic inside `Execute()` silently keeps its default value forever when a caller forgets it, and the comparison it feeds never matches - the feature it enables quietly never runs. A setter with no caller anywhere is not dead code to leave for later, it is proof the logic behind it is already broken; delete both the setter and the logic it gated, or wire up the missing call, in the same change.

### **3\. Extensions**

* **What:** Static C\# extensions.  
* **Rule:** Add power to specific type (`this GameObject`). Only talk to `Utils`/`Extensions`.

### **4\. Service**

* **What:** Lazy helper. Does one job.  
* **Rule:** Wait to be called. Never start actions alone.  
* **No-No:** No UI View. Mostly no `MonoBehaviour`. Only talk to `Services`/`Utils`.
* **Name methods for what they do, not for who calls them.** `ClearAllCooldowns()` over `InitStage()` - the caller's lifecycle is the caller's business, and the same method usually ends up called from a second moment. `InitEntryPoint()` is the one exception, since being the entry point *is* the job.

---

## **MVC RULES (MODEL-VIEW-CONTROLLER)**

Must follow strict MVC. No break rule.

### **THE DATA (Model Layer)**

* **5\. Model:** Dumb. Holds data only.  
  * Can have Math/Gets/Sets.  
  * **No-No:** No Events\! No Unity\!  
* **6\. DataService:** Boss of Models. Holds lists/dictionaries of Models.  
  * **Rule:** Raises Events when data changes.  
  * **No-No:** No Unity\! Talk only to `Utils`.

### **THE EYES (View Layer)**

* **7\. View:**  
  * Dumb eyes. Must be `MonoBehaviour`.  
  * **Rule:** Only show colors/text. Tell others when user clicks.  
  * **No-No:** Never talk to Brain (`Controller`/`Service`). Never listen to Logic events.  
* **8\. View Controller:**  
  * Boss of specific View.  
  * **Rule:** Do complex visual math (like spinning wheel) so `View` stays dumb.

### **THE BRAIN (Logic Layer)**

* **9\. Controller:**  
  * The Brain. Does feature logic.  
  * **Rule:** Handles entry points. Cannot touch other `Controllers`, but can touch their `DataServices`.  
* **10\. Manager:**  
  * Biggest Brain. Boss of many `Controllers`.

### **11\. Factory**

* **What:** Maker of things.  
* **Rule:** Hide messy `Instantiate` or creation logic inside here.

### **Cross-Feature Talk**

* **What:** How one feature talk to another feature?  
* **Rule:** Do not tangle brains.  
  1. Shoot a `Command`.  
  2. Or raise an event using `Broadcaster`.

---

### **NETWORKING**

* **NetEvents**  
  * Net events can't be missed by the client since we send all the unprocessed net events every tick. Therefore, we ONLY DesirializeDelta object states which are changing constantly like the player's position, and send in a NetEvent things that happen once in a while. This way we save Network bandwidth.

#### **A net event is as small as it can possibly be**

* **Every byte on the wire is paid 60 times a second, so a net event carries the minimum.** Before adding a field, ask twice: can the client resolve it, and if not, what is the narrowest type that holds it.
* **Cut any field the client can resolve from an id already in the message** (see *DERIVED DATA*). A team id follows from the player id via `IMatchDataService.GetPlayerTeamId`, a caster's position follows from the caster id - send the id, resolve on the client. This holds for the `Handle*NetEventsCommand` too: it may resolve a DataService read-only to derive such a value.
* **Type every remaining field to its real range, never to `int` by default.** Counts, scores, healths and ids that fit in 0-255 are `byte`; up to 65,535 is `ushort`. `Serialize`/`Deserialize` use the matching width (`writer.Put(value)` / `reader.GetUShort()`), and the caller casts at the `Add*NetEvent` call site so the whole signature chain stays narrow.
* **Not sure a value fits the narrower type? Ask Raz, do not pick the safe wide one.** The ceiling is a design decision (a score cap, a max entity count), not something to guess from.

#### **A serialisable type owns its own wire format**

* **A struct that travels inside another serialisable type implements `INetSerializable` itself.** The containing `Serialize`/`Deserialize` loop is then one call per element - `list.GetByIndex(i).Serialize(writer)` / `list.AddAndGet().Deserialize(reader)` - and the format of that struct exists in exactly one file.
* **Hand-inlining an element's fields inside its container writes the same wire format twice.** The two copies stay consistent with each other only until the next field is added, and nothing fails loudly when they stop - both sides of the drifted copy still agree, so the field simply never arrives on that path.
* **A deserializer that assigns a default or a sentinel to a field instead of reading it is a drifted copy, not a design.** The field is on the wire everywhere else; that path just never got updated.
* **Adding a field to a serialisable struct must be a one-file change.** If a second `Serialize` needs the same field appended, move the format onto the type first, then add the field once.
* **Serialize elements in place, never through a local copy.** A `var element = list[i]` copies the struct, and a `ref var` filled field by field is a half-built value until the last line - both are just extra chances to forget a field.

#### **Net event naming**

* **Name a net event for who did what, subject first, in active voice:** `PlayerPassedScoreGateNetEventS2C`, not `ScoreGatePassedNetEventS2C`. The actor leads, the thing it acted on follows.
* **Name the event for what actually happened, not for the action that triggered it.** An event that only ever fires once its target is destroyed should say so (`MoleKilledNetEvent`), even if the code path that raises it is a "hit" - a hit the target survives is a different event (`GoldenMoleDamagedNetEvent`) with a different name, so the name of the first one should not be a generic word that also covers the second.
* **A tick or duration field name states which edge of the transition it marks.** `EmergeOnTick` reads as when the emerge starts; if the field actually holds when the emerge animation finishes, name it `FinishEmergingOnTick`. Do the same for every "on tick" field describing a wind-up, a close or a hide - name it for the edge it is, so it cannot be misread as the other edge of the same transition.
* **A rename carries the whole family in the same change** - the struct and its file (`git mv` the `.meta` too), the `MatchFullTickPacketS2C` list field, the `NetworkConfig.MaxCap` entry, the per-client queues, `Add*NetEvent`, `Process*Events`, `Serialized/Deserialized*`, the `Handle*NetEventsCommand` and its file, and every local. Names that are not about the event itself (an `AudioClipType`, a `Play*Animation` method) stay as they are.

## **C\# CODING STANDARDS**

### **1\. NAMING CONVENTIONS**

* **Prefixes:** `I`Interface, Enum`Type`, `_`privateField, `On`EventHandler, `Try`Method (if early exit).  
* **Suffixes:** `Event` (for Events). Append the data structure type (e.g., `OffersDictionary`, `EnemyArray`, `WindowPrefab AnimationCancellationToken`). `Coroutine` (for Unity Coroutines). `Async` (For Task/UniTask).   
* **Casing:** `ALL_CAPS` (Constants), `PascalCase` (Properties/Enums), `camelCase` (Locals).  
* **Booleans:** Prefix with question words (`is`, `can`, `has`).  
* **Specificity:** No abbreviations (`Pref`, `GO`, `Pos`). Include time units in names (e.g., `cooldownInSeconds`).  
* **Comments:** Avoid as much as possible! Instead of comments, the code must self-document via long/clear variable/method/class names. If you must have a comemnt, it should explain *why*, not *what*.

#### **A comment is a name that was not extracted**

* **A block comment above a chunk of code is that chunk asking to be a method.** Extract it and let the method name carry what the comment said - `KeepMultiplierTextUpright()`, `PlacePostsOnBothSidesOfGap()`, `SnapBeamToFlashedPunchState()`. The comment is then deleted, not moved above the new method.
* **A comment explaining an expression is a missing local.** Name the intermediate value instead of describing the formula (`postOffsetXFromGapCenter`, `beamThickness`, `postScaleForUnitAuthoredSprite`).
* **A comment explaining a condition is a missing property.** `IsPassAnimationPlaying` over a comment above the `if`.
* **A comment explaining a constant or a field belongs in its name.** `PASS_COLOR_DURATION_IN_SECONDS` + "the flash eases back to the beam colour" is `PASS_FLASH_FADE_DURATION_IN_SECONDS`; `_baseLineColor` + "the colour the next pass returns to" is `_restingLineColor`.
* **Prefer deleting the reason for the comment over rewording it.** A comment justifying why the code is correct usually points at code that could just be correct without the caveat - reading `transform.rotation` needs no note about the parent being unrotated, while `localRotation` does.
* **Cleaning comments never changes behaviour or public API.** Extracted methods are `private`, serialized field names stay untouched so prefab references survive, and callers are left alone.


#### **Name a query for the fact it checks, not for the conclusion its caller draws**

* **A boolean method is named after what it verifies, using only the data its own class owns.** A class holding timers can answer `IsTargetTimerEnded`; it cannot answer `IsTargetShootable` - shootability is the caller's interpretation of that fact, and the next caller will draw a different conclusion from the same answer. Name it for the fact and both callers read correctly.
* **The conclusion belongs at the call site, in a local named for what it means there.** That is also what keeps the query reusable - a query named after one caller's decision is a query nobody else can call without lying.
* **An existence check over a collection reads `HasAny...`.** `HasNonRetainedTarget` reads as a statement about *the* target; `HasAnyNonRetainedTarget` cannot be misread as being about one particular element.
* **A method on a `*Controllers` or collection class that acts on a single element says so in its name** (`...OfPlayer`, `...OfCaster`), matching its siblings, so it is never read as acting on all of them.

#### **One concept, one name along the whole call chain**

* **The same value keeps the same name in the interface, the implementation and every private helper it is passed to.** A list that is `outputTargetedObjects` in the public method and `validTargetedObjects` in the helper it calls forces the reader to re-establish, at every hop, that these are the same list.
* **Name a parameter for what the data is, not for the role it plays at one call site.** `targetsInConeSight` over `output...` or `valid...`: role names go stale the moment the method both reads and appends to the list, and "valid" never says which validity.
* **A parameter rename carries into the `I...` declaration in the same change**, so the interface and the implementation never disagree about what the argument is.

### **3\. PROGRAMMING RULES**

* **Booleans:** Extract any logic into local `var` or properties. Do not calculate or check a condition with '=='/'>''<' inside the `if()` statement.  
  * **This covers method calls too, not only operators.** `if (targets.ContainsTarget(key))` becomes `var isTargetStillInConeSight = targets.ContainsTarget(key);` whenever the method's own name does not state what a `true` means *here* - the local carries the meaning the call cannot.  
* **Branch on the flag that actually distinguishes the cases, never on two numbers that happen to coincide.** A comparison like `remainingLives > damagePerHit` can look correct only because one entity type's life count happens to equal the damage constant - tune either number, or add a third case, and the coincidence breaks silently. Check the explicit flag (`IsGolden`) first, and only compare the numbers once inside the branch where they are actually meaningful.  
* **If Statements:** Braces `{}` required unless it is a simple early return. Prefer early returns over nesting. Put expected logic in `if`, edge cases in `else`.  
* **Constants:** No magic numbers. Ever.  
* **Events & Delegates:** \* `Action` delegates must be `private` and injected via Setup/Constructors.  
  * Events must use the `event` keyword (e.g., `public event Action MyEvent;`).  
  * Use `System.Action`, not `UnityAction`.  
* **Access Modifiers:** Default to `private` and `readonly` whenever possible.  
* **General Safety & Perf:**  
  * Use `var`.  
  * Use C\# sugar (`??`, `?.Invoke()`).  
  * No `Tuples` (create a class/struct instead).  
  * No `AddComponent` or `GetComponent` at runtime.  
  * No `RemoveAllListeners()`.  
  * Cache repeated logic.  
  * No methods called in constructors. Only assign fields.  
  * Pass explicit callbacks (`OnSuccess`, `OnFail`) instead of generic status wrappers.  
  * Never use destroyCancellationToken.
  * Never check if(gameObject.activeInHierarchy/activeSelf)
  * A method that gets a CancellationToken as a parameter is always async.
  * Use v.NormalizeSafe() instead of Vector2.Normalize(v)
  * For every Register should also be Unregister, same fore events/listeners.
  * Don't use _Time in shaders, instead update the shader from the outside in an Update/Tween
  * In the presentation domain, use _stageCancellationTokenProvider to get a cancellation token
* **Reflection**: Never at runtime\!


---

## **IDENTITY**

* **Identify a thing by a stable id, never by a value that describes it.** A position, a name or an index in a runtime list drifts, gets quantized on the wire or gets reordered. Matching "the closest one" is a bug waiting for the first rounding error.
* **One identity per concept, everywhere.** The state, every message about it and everything that references it all name it the same way, so no consumer has to translate, search or match.
* **Ids start at 1.** Zero stays free to mean "none", which is also the value of an unset field.
* **Author ids in config and bake them with the data.** When a config gains an id field, migrate the already baked assets in the same change - never leave data waiting for a manual re-bake.

---

## **DERIVED DATA**

* **Never send what the receiver can resolve on its own.** If a value follows from an id both sides already have, send the id.
* **Caching a derived value is a local decision, not a wire one.** Keep it on the state only when it is read in per-tick loops, mark it `// server only`, and keep it out of Serialize/Deserialize.
* **No double storage of the same fact** (see *No Double Data*): whoever owns a fact owns it alone, everybody else reads it from there or receives it as an argument.

---

## **TUNING VALUES BOTH DOMAINS AGREE ON**

* **A number the server and the client must agree on lives in the Shared config, never as a `const` in one domain.** A private `const` next to the code that uses it is invisible to the other domain, so the second domain writes its own copy and the two silently drift apart on the first tuning change.
* **Put it in the Shared config the moment a second domain needs the same number**, and delete the `const` in the same change - do not leave the original in place "for now".
* **Both sides read it from the config, nobody re-derives it.** The simulation that applies the value and the presentation that displays it resolve the same field (see *No Double Data*).
* **Type the field like the data it feeds** - the same width as the state field it is applied to, so no cast is invented at the call site.
* **Once it is configurable, stop assuming its literal value.** Code written around a hardcoded `1` usually breaks at 2 - decrements underflow, `> 0` checks stop meaning "survived". Rewrite the surrounding logic to hold for any authored value in the same change.
* **Server-only tuning stays server-only.** Only the value both sides read is promoted; the rest stays in the simulation config (same split as the gate geometry vs. its solver tuning).

---

## **HANDING SOMETHING OVER: REUSED IDS, SLOTS AND POOLED OBJECTS**

Whenever one thing is handed from one occupant to the next - a spawn point taken by a new entity, a pooled view, a recycled id:

* **Track the containers, not the occupants.** One dictionary keyed by the container's stable id, built once. No second dictionary keyed by the occupant, and no scans - every lookup is one hit.
* **A container knows whether it is occupied and by whom** - the occupant's identity only, never a copy of the occupant's data.

---

## **MESSAGES CARRY THEIR OWN DATA**

* **A message is self-sufficient.** A net event carries which thing it is about, plus every value its `Handle*NetEventsCommand` draws.
* **Never look a model up in order to handle a message.** The thing it describes may already be gone - destroyed on the same tick, drained in a different order - and then the handler silently does nothing. Read it off the message.
* **Model writes still belong to the receive-time handler** (see the DataService split): the command reads the message, not the model.

---

## **DATA OWNERSHIP IN PRESENTATION**

* **The DataService model is the only place entity data is kept.** A controller must not hold its own copy of a value a model already has.
* **A controller holds only what is its own:** which entity it currently represents, and the state of its own visuals.
* **What is needed after an await goes in as an argument**, so it lives in that method's locals instead of becoming a field on the controller.

---

## **ASYNC SEQUENCES & CANCELLATION**

* **One `CancellationTokenSource` per running sequence, per owner.** Starting the next sequence cancels and disposes the previous one and creates a new source, linked to the lifetime token of the scope.
* **Never check state after an await.** If a continuation must not run, cancel its token. `if (_state != X) return;` after every await is the bug, not the fix - and once cancellation does the job, delete the state that existed only for those checks.
* **Undo partial visual state in a `finally`.** A cancelled tween or animation stops wherever it happens to be, so restoring it in a `finally` covers every interruption path at once.
* **Cancel on destroy, despawn and return to pool**, so no continuation can ever touch a recycled object.
* **Link, never share.** Awaiting on a long-lived token piles up its `Register` callbacks for the whole scope; a per-sequence linked source drops them when it is disposed.

#### **Every async method takes a token**

* **An async method always takes a `CancellationToken` and forwards it to every `await` inside it.** No async method obtains its own token internally, and never `destroyCancellationToken`. This is the other direction of "a method that gets a CancellationToken is always async" - both hold.
* **The token comes from the owner.** In presentation, the `*Controllers` class injects `IStageCancellationTokenProvider` and passes the token down to the controller and the view. A view only creates its own source when it links one per sequence off a token it was given.

#### **Tweens are awaited, never fired and forgotten**

* **Every tween and sequence is awaited with `.WithCancellationSafe(cancellationToken)`.** A tween started and left running is unowned - it survives despawn, pooling and stage exit.
* **What happens after the animation goes after the `await`, not in `OnComplete`.** Sequencing reads top to bottom, and a cancelled animation simply never reaches the next line - no callback fires on a recycled object.
* **Stop a tween by cancelling its token, not by `Kill()`.** `WithCancellationSafe` kills it on cancel, so the `Tween`/`Sequence` field and every `Kill()` call disappear with it. Keep the field only when something else genuinely reads the tween's state.
* **A synchronous entry point that starts an animation restarts its `CancellationTokenSource` and calls the async method with `.Forget()`** (`AwaitableUtils.Forget` swallows the cancellation and logs anything else). Never `_ = SomethingAsync()` and never `async void`.
* **An endless animation is awaited too.** A `SetLoops(-1)` tween ends only by cancellation, which is exactly what its token is for.


---

## **MUTATING STATE SAFELY**

* **Copy out of a `ref` before removing.** Fixed unordered lists remove by swap-and-pop, so a `ref` into one points at a different element right after a removal - read what you still need into locals first.
* **Pre-allocate buffers at the entry point**, cleared on reuse. Never allocate them per tick, per stage or per round - a buffer that is re-`new`ed whenever the data outgrows it is still a per-stage allocation.
* **Size every buffer from a `NetworkConfig.MaxCap` entry.** If no cap fits, add one in the same change instead of measuring the runtime data - the cap is the contract, the amount that happens to be loaded is not.
* **Cap what the array is indexed by, not what it holds.** An array keyed by hole id is sized by the highest authored hole id, not by how many moles are alive at once; picking the wrong one silently drops every write past the end. Ids start at 1, so the length is cap + 1.
* **Never mutate a dictionary while enumerating it, not even values-only.** Collect the keys into a pre-sized buffer (`stackalloc` in the simulation domain) in one pass, then apply the mutation over that buffer in a second pass. Relying on `foreach (var key in dict.Keys) dict[key] = ...` being safe today ties correctness to framework internals nobody should have to reason about at the call site.

---

## **CHECK ON CHANGE, NOT EVERY TICK**

* **A condition that can only start at a known moment is checked at that moment, never polled.** Two entities overlapping, a slot becoming free, a value crossing a threshold - if it can only become true when something is created, moved or changed, the check belongs at that change. A per-tick `foreach` over everything is O(a x b) every tick to find something that changes a handful of times per match.
* **Find every moment it can start, and hook them all.** An overlap between two kinds of things starts either when the first appears or when the second does, so both creation points get the check - one entity against the existing list of the other. Miss one side and the case silently stops working.
* **Hook the moment the thing becomes real, not the moment it is created.** An entity that spawns hidden/disabled and turns active later is only checkable when it turns active - that is the event, not the spawn.
* **Only poll what genuinely changes every tick** - anything driven by physics or continuous movement. Everything static after creation is event driven.
* **The check itself is one Command per trigger**, holding the trigger's id (see *Commands*), so each creation site stays one line.
* **Removing inside the loop the check triggers?** Iterate backwards (see *Copy out of a `ref` before removing*), and stop early once the thing being checked against is gone.
* **A one-shot effect belongs in the method that starts it, not in `OnTick`.** An effect that should happen once, at the moment something activates, is a single call at that activation site - the same call left inside `OnTick` instead fires every tick for the whole duration, so a discrete "hit" turns into 60 hits a second and any incremental-damage or survives-N-hits mechanic on the receiving end is bypassed entirely.

---

## **FINISHING A CHANGE**

* **Delete what the change made dead** - an enum, a method, an interface member that no longer has a caller.
