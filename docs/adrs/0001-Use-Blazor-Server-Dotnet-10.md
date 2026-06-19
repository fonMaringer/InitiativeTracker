# ADR-0001: Use Blazor Server with .NET 10

**Status:** Accepted
**Date:** 2025-06-14
**Author:** InitiativeTracker Team
**Consulted:** Project architecture review
**Deciders:** Project owner

---

## Context

The InitiativeTracker is a single-user Dungeon Master tool for managing D&D-style combat rounds, miniatures, items, and spells. The project needed a web-based UI technology stack that:

1. Shares the same language (C#) as the business logic layer
2. Provides a rich interactive experience without writing separate frontend code
3. Keeps the entire application under one solution with minimal build complexity
4. Runs comfortably on a single server (developer's machine or home DM box)

The decision was made between Blazor Server, Blazor WebAssembly, and SPA frameworks (React/Vue/Angular) paired with a backend API.

## Decision

We chose **Blazor Server** as the primary rendering technology, targeting **.NET 10** with C# 14 language features. Key aspects of this decision:

- **Render mode:** Interactive Server (`@rendermode InteractiveServer`) for all pages that require stateful UI updates
- **Runtime:** .NET 10.0 (`net10.0` TFM), leveraging latest compiler and framework improvements
- **Language version:** C# 14 (primary constructors, collection expressions, extension everything syntax)

All connected clients communicate with the server over a SignalR circuit. UI components are rendered on the server as HTML diffs and pushed to the browser. Business logic executes directly on the server without additional API boundaries.

## Consequences

### Positive

- **Single language surface:** C# throughout — no TypeScript/JavaScript duplication for business rules
- **Fast development velocity:** Components, services, and data access all live in one project with tight feedback loops
- **Immediate SignalR capabilities:** Real-time UI push (initiative tick countdown, state broadcast) is built-in, zero extra setup
- **Thin build pipeline:** One `.slnx`, one compilation unit, no separate frontend build step or NPM dependency chain
- **Strong typing end-to-end:** Domain entities flow directly into Blazor component parameters with compile-time safety
- **.NET 10 performance improvements:** Faster JIT, improved SignalR throughput, better startup times

### Negative

- **Server memory pressure:** Each connected circuit holds a snapshot of component state server-side; while this is a single-user tool now, adding multi-seat support will linearly increase memory consumption per connected user
- **Requires persistent connection:** If the SignalR connection drops (network blip), the UI shows a toast and requires reconnect. Offline operation is not possible with Blazor Server alone
- **Tied to .NET runtime hosting:** Cannot deploy as static files; always needs an active `.exe` / Kestrel process behind it

### Neutral

- **JS interop still needed:** Browser-native features (printer API, file picker for miniature images, clipboard access) require `IJSRuntime` calls, which adds a thin JavaScript boundary that must be maintained
- **Third-party component libraries** are limited compared to the React/Vue ecosystem; we landed on Blazor.Bootstrap 3.5.0 which covers our needs but may lack cutting-edge components

---

## Alternatives Considered

| Option | Pros | Cons |
|--------|------|------|
| **Blazor Server (Chosen)** | Single language, built-in SignalR, fast dev iteration | Memory per circuit, always-online requirement |
| Blazor WebAssembly | Runs in browser, no server state | Larger initial download (~7 MB .NET runtime), slower cold start, sandbox prevents direct file/DB access |
| React + REST API | Mature frontend ecosystem, offline capability | Two languages, duplicated DTO layer, separate build pipeline |
| MAUI / Avalonia (desktop) | Native desktop window, offline-first | Platform lock-in, no browser-based sharing for co-DMs |

---

## References

- [System Design Document](../SYSTEM_DESIGN.md)
- [.NET 10 Release Notes](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/)
- [Blazor Server rendering mode docs](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes?view=aspnetcore-10.0#blazor-server-rendering-mode)
