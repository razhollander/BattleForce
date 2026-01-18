## Project Overview

**BattleForce** - Unity 6.000.047f1 desktop multipler game

**Stack**: Unity URP | Zenject (DI) | Awaitable | LiteNetLib + Socket

## Domains
- Presentation - The client, written in Unity
- Simulation - The server, written in pure C#
- Shared - Classes used by client and server

## Key Paths
- Presentation: `Assets/Core/Game/Domains/GamePlay/Presentation`
- Simulation: `Assets/Core/Game/Domains/GamePlay/Simulation`
- Shared: `Assets/Core/Game/Domains/GamePlay/Shared`

## Critical Rules
- Never use MonoBehaviour lifecycle methods. 
For Awake/Start/OnEnable use InitEntryPoint. 
For OnDisable/OnDestroy use InitExitPoint.
- Use `var` whenever possible
- No comments unless complex algorithm
- No Tuples - use Result classes
- Awlays prefer Commands over Events, CommandsFactory folder for reference: BattleForce/Assets/Core/Scripts/Services/CommandFactory
- Composition over Inheritance

## Principles

- **Clean Architecture** + **SOLID**
- **Composition over Inheritance**: Build complex behaviors by combining smaller components
- **Dependency Rule**: Source code dependencies point inward toward higher-level policies
- **Commands over Events**: Always prefer launching a Command over firing an event
- **Controller → View delegation**: Controllers pass delegates to Views, never the other way around

## SOLID Quick Reference
- **S**ingle Responsibility: One class, one job
- **O**pen/Closed: Open for extension, closed for modification
- **L**iskov Substitution: Subtypes substitutable for base types
- **I**nterface Segregation: Many specific interfaces > one general interface
- **D**ependency Inversion: Depend on abstractions (via VContainer)


# How to Implement a New `XNetEventS2C`

This guide explains how to implement a new network event (server-to-client) in the game, using `TalentCardHitNetEventS2C` as a reference.

## 1. Define the Event Struct

Create a new file in `Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/PacketEvents/NetEvents/`.
The struct must implement `INetSerializable` and `IComparable<T>`.

**Example:** `MyNewNetEventS2C.cs`

```csharp
using System;
using LiteNetLib.Utils;

namespace Core.Game.Domains.GamePlay.Shared.S2CModels.PacketEvents.NetEvents
{
    public struct MyNewNetEventS2C : INetSerializable, IComparable<MyNewNetEventS2C>
    {
        public int OccuredOnTick;
        // Add your specific fields here (e.g., Id, Position, Health)
        public ushort EntityId;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(OccuredOnTick);
            writer.Put(EntityId);
            // Serialize your fields
        }

        public void Deserialize(NetDataReader reader)
        {
            OccuredOnTick = reader.GetInt();
            EntityId = reader.GetUShort();
            // Deserialize your fields
        }

        public int CompareTo(MyNewNetEventS2C other)
        {
            return OccuredOnTick.CompareTo(other.OccuredOnTick);
        }
    }
}
```

## 2. Configure Max Capacity

Add a configuration for the maximum number of these events per frame/packet in `Assets/Core/Scripts/Network/NetworkConfig.cs` inside the `MaxCap` class.

```csharp
[Serializable]
public class MaxCap
{
    // ... other caps
    public int MyNewNetEvents = 128; // Choose an appropriate limit
}
```

## 3. Update `FullTickPacket`

Modify `Assets/Core/Game/Domains/GamePlay/Shared/Scripts/S2CModels/FullTickPacket.cs` to include a list of your new events.

1.  **Add Field:**
    ```csharp
    public FixedUnorderedList<MyNewNetEventS2C> MyNewNetEvents;
    ```
2.  **Initialize in Constructor:**
    ```csharp
    public FullTickPacket(MaxCap maxCap, SharedGamePlayConfig sharedGamePlayConfig)
    {
        // ...
        MyNewNetEvents = new FixedUnorderedList<MyNewNetEventS2C>(maxCap.MyNewNetEvents);
    }
    ```
3.  **Update `Serialize` method:**
    ```csharp
    public void Serialize(NetDataWriter writer)
    {
        // ...
        SerializedMyNewNetEvents(writer);
    }

    private void SerializedMyNewNetEvents(NetDataWriter writer)
    {
        writer.Put((byte) MyNewNetEvents.Count);
        foreach (var myEvent in MyNewNetEvents.AsSpan())
        {
            myEvent.Serialize(writer);
        }
    }
    ```
4.  **Update `Deserialize` method:**
    ```csharp
    public void Deserialize(NetDataReader reader)
    {
        // ...
        DeserializedMyNewNetEvents(reader);
    }

    private void DeserializedMyNewNetEvents(NetDataReader reader)
    {
        MyNewNetEvents.Clear();
        var count = reader.GetByte();
        for (var i = 0; i < count; i++)
        {
            ref var myEvent = ref MyNewNetEvents.AddAndGet();
            myEvent.Deserialize(reader);
        }
    }
    ```

## 4. Server-Side Storage (Simulation)

Update `Assets/Core/Game/Domains/GamePlay/Simulation/Scripts/MatchModel/MatchNetEventsDataService.cs` and `IMatchNetEventsDataService.cs`.

1.  **Add Property to Interface & Class:**
    ```csharp
    CapacityDict<ushort, FixedUnorderedList<MyNewNetEventS2C>> MyNewNetEventsPerPlayer { get; }
    ```
2.  **Add Pool Field:**
    ```csharp
    private readonly ConcurrentPool<FixedUnorderedList<MyNewNetEventS2C>> _myNewEventListPool;
    ```
3.  **Initialize in Constructor:**
    ```csharp
    MyNewNetEventsPerPlayer = new CapacityDict<ushort, FixedUnorderedList<MyNewNetEventS2C>>(maxConcurrentPlayers);
    _myNewEventListPool = new ConcurrentPool<FixedUnorderedList<MyNewNetEventS2C>>(() => new FixedUnorderedList<MyNewNetEventS2C>(networkConfig.MaxCap.MyNewNetEvents), maxConcurrentPlayers);
    ```
4.  **Update `StartSavingPlayerEvents`:**
    ```csharp
    if (!MyNewNetEventsPerPlayer.ContainsKey(playerId))
    {
        MyNewNetEventsPerPlayer.Add(playerId, _myNewEventListPool.Get());
    }
    ```
5.  **Update `StopSavingPlayerEvents`:**
    ```csharp
    var myList = MyNewNetEventsPerPlayer[playerId];
    myList.Clear();
    _myNewEventListPool.Return(myList);
    MyNewNetEventsPerPlayer.Remove(playerId);
    ```
6.  **Update `RemoveAllEventsOlderThanTick`:**
    ```csharp
    if (MyNewNetEventsPerPlayer.TryGetValue(playerId, out var myEvents))
    {
        for (int i = myEvents.Count - 1; i >= 0; i--)
        {
            if (myEvents[i].OccuredOnTick < tick)
            {
                myEvents.RemoveAt(i);
            }
        }
    }
    ```
7.  **Add Method to Add Event:**
    ```csharp
    public void AddMyNewNetEvent(int onTick, ushort entityId, /* other params */)
    {
        foreach (var kvp in MyNewNetEventsPerPlayer)
        {
            ref var packet = ref kvp.Value.AddAndGet();
            packet.OccuredOnTick = onTick;
            packet.EntityId = entityId;
            // ... set other fields
        }
    }
    ```
8. Setup the Events in ServerNetworkTickProcessor.SendCurrentTickStateToAllClients `_fullTickPacket`

## 5. Client-Side Handling (Presentation)

### Update `CachedPresentationEventsService`

Update `Assets/Core/Game/Domains/GamePlay/Presentation/Scripts/PresentationEvents/CachedPresentationEventsService.cs` and `ICachedPresentationEventsService.cs`.

1.  **Add Property:**
    ```csharp
    List<MyNewNetEventS2C> MyNewNetEvents { get; set; }
    ```
    Initialize it in the class: `MyNewNetEvents { get; set; } = new();`

### Update `SimulationNetEventsHandler`

Update `Assets/Core/Game/Domains/GamePlay/Presentation/Scripts/Network/PacketsHandlers/SimulationNetEventsHandler.cs`.

1.  **Add Process Method:**
    ```csharp
    public void ProcessMyNewNetEvents(CapacityList<MyNewNetEventS2C> myNetEvents)
    {
        if (myNetEvents.IsNullOrEmpty())
        {
            return;
        }

        foreach (var myEvent in myNetEvents)
        {
             // 1. Add to cached events for View processing if needed
            _cachedPresentationEventsService.MyNewNetEvents.Add(myEvent);

            // 2. Update Model state immediately
            // var entity = _matchDataService.GetEntity(myEvent.EntityId);
            // entity.State = myEvent.NewState;
        }
    }
    ```

### Update `FullTickPacketsHandler`

Update `Assets/Core/Game/Domains/GamePlay/Presentation/Scripts/Network/PacketsHandlers/FullTickPacketsHandler.cs`.

1.  **Add Cache Field:**
    ```csharp
    private readonly CapacityList<MyNewNetEventS2C> _cachedUnprocessedMyNewEvents;
    ```
2.  **Initialize in Constructor:**
    ```csharp
    _cachedUnprocessedMyNewEvents = new CapacityList<MyNewNetEventS2C>(networkConfig.MaxCap.MyNewNetEvents);
    ```
3.  **Add Process Method:**
    ```csharp
    private void ProcessMyNewEvents(FixedUnorderedList<MyNewNetEventS2C> myEvents)
    {
        _cachedUnprocessedMyNewEvents.Clear(); // Reuse list
        foreach (var myEvent in myEvents.AsSpan())
        {
            // Logic to determine if event should be processed based on tick
            // (See existing ProcessTalentCardHitEvents for example)
             if (IsTickValidToProcess(myEvent.OccuredOnTick))
            {
                _cachedUnprocessedMyNewEvents.Add(myEvent);
            }
        }
        _simulationNetEventsHandler.ProcessMyNewNetEvents(_cachedUnprocessedMyNewEvents);
    }
    ```
4.  **Call from `Handle`:**
    ```csharp
    ProcessMyNewEvents(packet.MyNewNetEvents);
    ```
### Add a new NetEventCommand
Unser Assets/Core/Game/Domains/GamePlay/Presentation/Scripts/Commands/NetEventsCommands add a new command that will handle the visual part of the event.
Take for example HandleTalentCardHitNetEventsCommand.cs.

### Execute the Command 
In `ClientPresentationTickProcessor.cs`:
1. Add a new private field for the command.
2. Init the command in the `ClientPresentationTickProcessor` constructor.
3. Execute the command insdie the `ManagedUpdate()` method.
Take reference from `_handleTalentCardHitNetEventsCommand`

## 6. Usage

Now you can trigger the event from the simulation code (e.g., in a System or Command) using `MatchNetEventsDataService.AddMyNewNetEvent(...)`.
The event will be serialized, sent to clients, and processed by `SimulationNetEventsHandler` where you can update the presentation model.


