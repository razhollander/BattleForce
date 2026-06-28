---
name: code-review
description: Supersonic Genesis code-review conventions and architecture rules.
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

### **Get methods must not mutate**

* Get means look, no touch. No side effects. Safe to call 100 times.  
* If Get changes state → change name. (E.g., GetAndClear).

---

## **CODE BUILDING BLOCKS**

### **1\. Utils**

* **What:** Static helpers.  
* **Rule:** Only static methods. Only talk to other `Utils` or `Extensions`.  
* **No-No:** No big brain logic. No Unity context jumping.

### **2\. Commands**

* **What:** Execute one thing.  
* **Rule:** Hold data, then `Execute()`. Good for complex, single-fire logic.

### **3\. Extensions**

* **What:** Static C\# extensions.  
* **Rule:** Add power to specific type (`this GameObject`). Only talk to `Utils`/`Extensions`.

### **4\. Service**

* **What:** Lazy helper. Does one job.  
* **Rule:** Wait to be called. Never start actions alone.  
* **No-No:** No UI View. Mostly no `MonoBehaviour`. Only talk to `Services`/`Utils`.

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

## **C\# CODING STANDARDS**

### **1\. NAMING CONVENTIONS**

* **Prefixes:** `I`Interface, Enum`Type`, `_`privateField, `On`EventHandler, `Try`Method (if early exit).  
* **Suffixes:** `Event` (for Events). Append the data structure type (e.g., `OffersDictionary`, `EnemyArray`, `WindowPrefab AnimationCancellationToken`). `Coroutine` (for Unity Coroutines). `Async` (For Task/UniTask).   
* **Casing:** `ALL_CAPS` (Constants), `PascalCase` (Properties/Enums), `camelCase` (Locals).  
* **Booleans:** Prefix with question words (`is`, `can`, `has`).  
* **Specificity:** No abbreviations (`Pref`, `GO`, `Pos`). Include time units in names (e.g., `cooldownInSeconds`).  
* **Comments:** Code must self-document via long/clear names. Comments explain *why*, not *what*.

### **3\. PROGRAMMING RULES**

* **Booleans:** Extract complex logic into local `var` or properties. Do not calculate inside the `if()` statement.  
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
* **Reflection**: Never at runtime\!

