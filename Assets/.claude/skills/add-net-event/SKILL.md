---
name: add-net-event
description: Add a new NetEvent to the BattleForce server-to-client network event system. Use when asked to add a net event, network event, S2C event, or extend the tick packet with a new event type.
---

Adding a new S2C NetEvent requires touching 7 files in a specific order. There are two flavors: **struct** (value type, no heap allocation — use for simple fixed-size data) and **class** (reference type — use when the event contains a collection or needs a default constructor for pooling).

`BulletSpawnNetEventS2C` is the canonical struct example.  
`TalentCardObtainedNetEventS2C` is the canonical class example.

---

## Files to touch (in order)

### 1. Create the event type
**Struct path:** `Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/<EventName>NetEventS2C.cs`  
**Class path:** same directory

**Struct template** (copy `BulletSpawnNetEventS2C.cs`):
```csharp
using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct <EventName>NetEventS2C : INetSerializable, IComparable<<EventName>NetEventS2C>
    {
        public int OccuredOnTick;
        // add your fields here

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            // serialize each field
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            // deserialize each field
        }

        public int CompareTo(<EventName>NetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
```

**Class template** (copy `TalentCardObtainedNetEventS2C.cs`):
- Same interface, but `class` instead of `struct`
- Must have a parameterless constructor (used by the pool factory)
- Use `ref var` → `var` when calling `AddAndGet()` in the service

---

### 2. Add the capacity constant to `NetworkConfig.MaxCap`
**File:** `Core/Scripts/Network/NetworkConfig.cs`  
Add inside the `MaxCap` nested class (around line 60–110):
```csharp
public int <EventName>NetEvents = 128; // tune to expected burst size
```

---

### 3. Add the field to `MatchFullTickPacketS2C`
**File:** `Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/MatchFullTickPacketS2C.cs`

**a) Declare the field** (struct → `FixedUnorderedList`, class → `FixedClassUnorderedList`):
```csharp
public FixedUnorderedList<<EventName>NetEventS2C> <EventName>NetEvents;
```

**b) Initialize in constructor** (around line 70–125):
```csharp
<EventName>NetEvents = new FixedUnorderedList<<EventName>NetEventS2C>(maxCap.<EventName>NetEvents);
// For a class type:
// <EventName>NetEvents = new FixedClassUnorderedList<<EventName>NetEventS2C>(maxCap.<EventName>NetEvents, () => new <EventName>NetEventS2C());
```

**c) Register in `CalculateEventMask()`** — pick the next free bit (currently highest used is bit 42):
```csharp
if (<EventName>NetEvents.Count > 0) eventMask |= 1UL << <N>;
```

**d) Add to `Serialize()`**:
```csharp
if ((eventMask & (1UL << <N>)) != 0) Serialized<EventName>NetEvents(writer);
```

**e) Add to `Deserialize()`**:
```csharp
if ((eventMask & (1UL << <N>)) != 0) Deserialized<EventName>NetEvents(reader);
else <EventName>NetEvents.Clear();
```

**f) Add the private serialize/deserialize methods** (follow the existing pattern at the bottom of the file):
```csharp
private void Serialized<EventName>NetEvents(NetDataWriter writer)
{
    writer.Put((byte)<EventName>NetEvents.Count);
    foreach (var e in <EventName>NetEvents.AsSpan())
        e.Serialize(writer);
}

private void Deserialized<EventName>NetEvents(NetDataReader reader)
{
    var count = reader.GetByte();
    <EventName>NetEvents.Clear();
    for (int i = 0; i < count; i++)
    {
        ref var e = ref <EventName>NetEvents.AddAndGet();
        e.Deserialize(reader);
    }
    // For class type: use `var e = <EventName>NetEvents.AddAndGet();` (no ref)
}
```

---

### 4. Add to `INetEventsDataService`
**File:** `Core/Game/Domains/GamePlay/Simulation/Scripts/NetworkManager/INetEventsDataService.cs`

Add the property and the `Add` method:
```csharp
CapacityDict<long, FixedUnorderedList<<EventName>NetEventS2C>> <EventName>NetEventsPerClient { get; }
void Add<EventName>NetEvent(int onTick, /* your params */);
```

---

### 5. Implement in `NetEventsDataService`
**File:** `Core/Game/Domains/GamePlay/Simulation/Scripts/NetworkManager/NetEventsDataService.cs`

**a) Add the property + pool field:**
```csharp
public CapacityDict<long, FixedUnorderedList<<EventName>NetEventS2C>> <EventName>NetEventsPerClient { get; }
private readonly ConcurrentPool<FixedUnorderedList<<EventName>NetEventS2C>> _<camelCase>NetEventsListPool;
```

**b) Initialize in constructor:**
```csharp
<EventName>NetEventsPerClient = new CapacityDict<long, FixedUnorderedList<<EventName>NetEventS2C>>(maxConcurrentPlayers);
_<camelCase>NetEventsListPool = new ConcurrentPool<FixedUnorderedList<<EventName>NetEventS2C>>(
    () => new FixedUnorderedList<<EventName>NetEventS2C>(networkConfig.MaxCap.<EventName>NetEvents), maxConcurrentPlayers);
```

**c) In `StartSavingClientEvents()`:**
```csharp
if (!<EventName>NetEventsPerClient.ContainsKey(clientId))
    <EventName>NetEventsPerClient.Add(clientId, _<camelCase>NetEventsListPool.Get());
```

**d) In `StopSavingClientEvents()`:**
```csharp
var <camelCase>List = <EventName>NetEventsPerClient[clientId];
<camelCase>List.Clear();
_<camelCase>NetEventsListPool.Return(<camelCase>List);
<EventName>NetEventsPerClient.Remove(clientId);
```

**e) Add the `Add` method:**
```csharp
public void Add<EventName>NetEvent(int onTick, /* your params */)
{
    foreach (var kvp in <EventName>NetEventsPerClient)
    {
        ref var packet = ref kvp.Value.AddAndGet();
        packet.OccuredOnTick = onTick;
        // assign your params
    }
}
```

---

### 6. Wire into `MatchFullTickPacketsHandler` (client-side tick processing)
**File:** `Core/Game/Domains/GamePlay/Presentation/Match/Scripts/Network/PacketsHandlers/MatchFullTickPacketsHandler.cs`

**a) Add cached list field** (around line 40–80):
```csharp
private readonly CapacityList<<EventName>NetEventS2C> _cachedUnprocessed<EventName>Events;
```

**b) Initialize in constructor** (follow existing pattern):
```csharp
_cachedUnprocessed<EventName>Events = new CapacityList<<EventName>NetEventS2C>(networkConfig.MaxCap.<EventName>NetEvents);
```

**c) In the tick-processing method**, copy from the packet into the cached list and pass to `_presentationNetEventsHandler` — follow how `BulletSpawnNetEvents` or `TalentCardObtainedNetEvents` are handled.

---

### 7. Add to `CachedPresentationEventsService` (and its interface)
**File:** `Core/Game/Domains/GamePlay/Presentation/Scripts/PresentationEvents/CachedPresentationEventsService.cs`  
**Interface:** `ICachedPresentationEventsService.cs` (same directory)

```csharp
public List<<EventName>NetEventS2C> <EventName>NetEvents { get; } = new();
```

Add the matching property to the interface too.

---

## Gotchas

- **Bit index in the event mask must be unique.** The mask is a `ulong` (64 bits).
- **Struct vs class `AddAndGet()`**: For structs use `ref var packet = ref list.AddAndGet()`. For classes use `var packet = list.AddAndGet()` (no `ref`).
- **`StopSavingClientEvents` must remove the key.** Every entry added in `StartSavingClientEvents` must be cleared, returned to pool, and `Remove(clientId)` called in `StopSavingClientEvents`. Missing a `Remove` leaks the dictionary entry.
- **`FixedUnorderedList` vs `FixedClassUnorderedList`**: Structs use `FixedUnorderedList<T>`, classes (reference types) use `FixedClassUnorderedList<T>`. The class variant pre-allocates instances via a factory lambda and reuses them — pass `() => new T()` to the constructor.
- **Serialization order must match.** The bit you pick in `CalculateEventMask` must be the same bit checked in `Serialize`, `Deserialize`, and the client-side handler. Double-check all three.
- **`MatchMakingFullTickPacketS2C` is separate.** If this event should also fire during matchmaking (pre-match phase), repeat the same pattern in `MatchMakingFullTickPacketS2C.cs` and its handler.
