## Project Overview

**BattleForce** - Unity 6.000.047f1 desktop multipler game

**Stack**: Unity URP | Zenject (DI) | Awaitable | LiteNetLib + Socket

### Key Paths
- Core services: `Assets/EPF/Game/Core/Subdomains/`
- Domains: `Assets/EPF/Game/Domains/{1-GameLoader, 2-Lobby, 3-Gameplay}`
- Main DI scope: `Assets/EPF/Game/Core/Subdomains/Scope/GameLifetimeScope.cs`

### Critical Rules
- Never use MonoBehaviour lifecycle methods (Awake, Start, OnEnable, OnDisable, OnDestroy)
- Use `var` whenever possible
- No comments unless complex algorithm
- No Tuples - use Result classes
- Commands over Events
- Composition over Inheritance

## Skills

Available via `/skills` command:

| Skill | When to Use |
|-------|-------------|
| `vcontainer` | Creating/modifying services, controllers, MonoBehaviours with DI |
| `commands` | Implementing discrete operations, server calls, complex actions |
| `coding-standards` | Writing new code, reviewing code style, naming conventions |
| `architecture` | Adding new features, understanding project structure, moving code between domains |
| `backend` | Working with PlayFab, Azure Functions, multiplayer networking |

## Lifecycle Quick Reference

```
Constructor/Construct → InitializeEntryPoint() → StartAsyncEntryPoint(CTS) → Terminate()
```

All initialization logic goes in `InitializeEntryPoint()` or `StartAsyncEntryPoint()`, never in constructors.
