# CONTEXT.md — InitiativeTracker Business Context

## 1. What Is This Application?

InitiativeTracker is an all-in-one web-based Dungeon Master (DM) toolkit for Dungeons & Dragons 5th Edition (D&D 5e). It consolidates tools that were previously scattered across separate applications: initiative tracking, miniature printing templates, and physical reference cards for items and spells.

The application runs locally on the DM's machine as a Blazor Server web app (.NET 10) with SQLite persistence.

---

## 2. Target Users

| Role | Who | Primary Tasks |
|------|-----|---------------|
| Dungeon Master (sole user) | Single DM per running instance. One app, one person. | Manages initiative order during combat, tracks HP/AC mid-fight, prepares printable miniatures and item/spell cards before sessions |

This is explicitly a **single-user tool**. There are no player-facing views, co-DM roles, or multi-seat support in scope.

---

## 3. Core Problem Statement

The DM previously relied on multiple disconnected tools:
- A whiteboard, spreadsheet, or separate initiative tracker app for combat management
- Word documents with manual tables for printing miniature templates
- Custom spreadsheets or notebooks for item and spell reference cards

InitiativeTracker unifies these into one application, reducing context-switching and ensuring the DM has everything in a single window during a live session.

---

## 4. Key Business Modules

### 4.1 Initiative Tracker
Manages the combat encounter round-by-round. The DM can:
- Add creatures to the initiative list automatically from the ttg.club bestiary API (these get an auto-rolled d20 + Dex modifier value)
- Add player characters and other non-bestiary creatures via manual initiative entry
- Navigate turn order with "Next" / "Previous"
- Modify HP and AC mid-combat
- Reorder, remove, sort, and clear entries

**Dice rolling:** Creatures added from the bestiary have a known Dexterity modifier and receive an automatic d20 roll. Player characters and creatures not in the bestiary are entered with a manually typed initiative value.

### 4.2 Miniature Printing
Allows the DM to maintain a library of miniature images and generate print-ready HTML sheets. Each miniature entry has a crop region for precise printing. Printed miniatures come in pairs (one upright, one rotated 180°), sized by D&D creature size category (Tiny → Gargantuan).

**Image sources:** Currently uploaded manually from local files. Future: support for downloading images by URL (including ttg.club sources).

### 4.3 Item Cards
Physical poker-sized cards (2.5" × 3.5") for magic items and equipment. Useful as reference handouts during play. Supports rich HTML descriptions via WYSIWYG editor. DM composes a preparation list with quantities, then generates print HTML that opens in a new browser tab.

### 4.4 Spell Cards
Physical poker-sized cards (2.5" × 3.5") for spells. Shows components (Verbal / Somatic / Material), class eligibility, and rich description. Prepared and printed the same way as item cards — preparation list → new tab → Ctrl+P.

---

## 5. Business Rules

### Initiative
- Single global initiative list. Future consideration: multiple named encounter lists.
- List persists across app restarts via SQLite (loaded in-memory at startup, flushed on shutdown).
- Legendary actions, lair actions, and multi-action creatures are handled by the DM manually — no dedicated mechanism is planned.
- In-memory state will eventually need auto-save to mitigate data loss from ungraceful shutdown.

### Print Generation
- Miniature print sheets: grouped by creature size, two images per cell (upright + rotated 180°), 1–2px border around each image
- Item/Spell cards: poker-size (2.5" × 3.5"), rounded corners, page-break-aware layout
- Generated HTML always opens in a **new browser tab** for the DM to review and manually print via Ctrl+P — no automated print dialog

### Data Sources
- ttg.club bestiary API is used live for creature lookups during session prep or combat
- Only minimal data needed for the initiative list (name, Dex mod, HP) is cached from external sources — full bestiary records are NOT stored offline

---

## 6. Operational Context

| Aspect | Detail |
|--------|--------|
| Deployment | Local machine, self-hosted, single-user |
| Concurrent users | 1 (the DM). Multiple browser tabs are fine; they share the same SignalR circuit state |
| Data persistence | SQLite file (`initiativetracker.db`), in-memory initiative list with startup load + shutdown flush |
| Crash tolerance | Currently no auto-save — losing in-memory state on crash is undesirable and planned to be addressed in a future release |
| Platform | Windows (primary development target) |

---

## 7. Non-Goals

InitiativeTracker is explicitly **NOT**:
- A campaign management tool (no tracking of story, loot inventory, NPC relationships, etc.)
- A character sheet manager or builder
- A VTT platform or integrator (no FoundryVTT, D&D Beyond, Roll20 sync)
- A cloud-synced or multi-user collaboration service
- A player-facing application

It is purely a **DM-side toolkit** focused on combat tracking and physical print prep.

---

## 8. Future Considerations

| Feature | Notes |
|---------|-------|
| Multiple initiative lists | Named encounter slots (e.g., "Ambush", "Boss Fight") — replaces single global list |
| Auto-save mechanism | Periodic or event-driven flush of in-memory initiative state to SQLite to prevent data loss |
| Image download by URL | Support fetching miniature images from remote URLs, including ttg.club |
| Tablet-responsive UI | Optional: optimized mobile view for use at the table — not near-term priority |
