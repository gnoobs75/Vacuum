# Vacuum - 3D Space Exploration Game

## Project Overview
Vacuum is a single-player 3D space exploration, mining, combat, and trading game inspired by Eve Online and X4: Foundations. Built with Godot 4 and C#.

## Tech Stack
- **Engine**: Godot 4 (latest stable)
- **Language**: C# (.NET)
- **Rendering**: Godot's Forward+ renderer for realistic PBR visuals
- **Target**: PC (Windows/Linux/Mac)

## Project Structure
```
Vacuum/
├── project.godot
├── src/                    # C# source code
│   ├── Core/               # Game loop, state management, events
│   ├── Systems/            # ECS-like game systems
│   │   ├── Flight/         # Ship movement, physics, controls
│   │   ├── Navigation/     # Warp drive, jump gates, autopilot
│   │   ├── Mining/         # Ore extraction, asteroid depletion
│   │   ├── Combat/         # Weapons, damage, targeting
│   │   ├── Trading/        # Market, buy/sell, cargo
│   │   ├── Industry/       # Refining, manufacturing, blueprints
│   │   └── AI/             # NPC behavior, faction logic
│   ├── Entities/           # Ship, Station, Asteroid, Planet, etc.
│   ├── Data/               # Static data definitions, configs
│   ├── UI/                 # HUD, menus, overlays
│   └── Utils/              # Helpers, math extensions
├── scenes/                 # Godot scene files (.tscn)
│   ├── main/               # Main game scene, loading
│   ├── space/              # Solar system, sectors
│   ├── ships/              # Ship scenes
│   ├── stations/           # Space station scenes
│   ├── asteroids/          # Asteroid belt scenes
│   └── ui/                 # UI scenes
├── assets/                 # Art, models, textures, audio
│   ├── models/
│   ├── textures/
│   ├── shaders/
│   ├── audio/
│   └── particles/
├── data/                   # JSON/Resource data files
│   ├── ships/              # Ship definitions
│   ├── modules/            # Module/fitting definitions
│   ├── ores/               # Ore and mineral definitions
│   ├── items/              # Tradeable items
│   └── solar_systems/      # Solar system layouts
└── tests/                  # Unit tests
```

## Architecture Principles
- **Composition over inheritance**: Use Godot nodes and C# components
- **Data-driven design**: Ship stats, ore types, item prices defined in data files (JSON or Godot Resources)
- **Signal-based communication**: Use Godot signals and C# events for decoupled systems
- **Scene-based organization**: Each major entity is its own scene for reusability
- **Separation of concerns**: Game logic in C# classes, presentation in scenes/shaders

## Coding Conventions
- C# naming: PascalCase for types/methods/properties, camelCase for locals/params, _camelCase for private fields
- One class per file, file name matches class name
- Use `partial class` for Godot node scripts to separate generated code
- Prefer `[Export]` attributes for inspector-exposed fields
- Use nullable reference types (`#nullable enable`)
- XML doc comments on public APIs only when non-obvious
- Keep methods under 30 lines; extract helpers when logic is complex

## Development Phases

### Phase 1: Flight + Solar System (MVP) ✅
- Player ship with 6DOF flight controls (WASD + mouse look)
- Player ship uses Miner.glb 3D model (`assets/models/ships/Miner.glb`)
- Third-person orbit camera with mouse wheel zoom (min 5, max 100 units)
- Solar system with star, planets (orbiting), moons
- Space stations (dockable)
- Asteroid belts (200 asteroids, procedural content)
- Basic HUD (speed, heading, position, warp status, autopilot)
- Warp drive mechanic (fast travel between points of interest)
- Skybox with starfield (3000 procedural stars)
- Escort ships: 3 initial escorts orbit the player in spherical golden-angle patterns (N to spawn 3 more)
- Asteroid belt miners: 2 mining escorts per asteroid with visual mining laser effects (beams, sparks, impact lights)
- Debug stats panel (F3): FPS, frame time, draw calls, objects, primitives, VRAM, node counts, escort/miner counts, memory

### Phase 2: Mining
- Mining laser module (equippable to ship)
- Asteroid types with different ore compositions
- Ore cargo hold with capacity limits
- Ore refining at stations (ore -> minerals)
- Asteroid depletion and respawn

### Phase 3: Trading & Economy
- Station markets with buy/sell orders
- Dynamic pricing based on supply/demand
- Cargo transport between stations
- NPC traders creating market activity
- Credits system

### Phase 4: Combat
- Weapon types: turrets (projectile, energy), missiles, drones
- Shield, armor, hull HP layers
- Targeting system with lock-on
- NPC pirates and faction ships
- Loot drops from destroyed ships

### Phase 5: Industry & Progression
- Manufacturing: minerals -> components -> ships/modules
- Blueprints system
- Ship fitting (high/mid/low slots like Eve)
- Skill or tech tree progression
- Multiple ship classes (frigate, destroyer, cruiser, battlecruiser, battleship)

### Phase 6: Advanced Systems
- Multiple solar systems with jump gates
- Faction reputation system
- Mission/contract system
- Station services (repair, insurance, cloning)
- Fleet command (hire wingmen)

---

## Game Design Reference

### From Eve Online
- **Ship Fitting System**: High/mid/low slots + rig slots. Ships have turret and launcher hardpoints. Modules consume CPU and powergrid. Each ship has bonuses that encourage specific roles.
- **Ship Classes**: Frigate (fast, small) -> Destroyer -> Cruiser -> Battlecruiser -> Battleship -> Capital. Tech 1 (basic), Tech 2 (specialized), Faction (powerful/expensive).
- **Weapon Types**: Turrets (projectile, hybrid, energy lasers) with tracking speed vs target signature. Missiles (no tracking, explode on arrival, damage based on target sig vs explosion radius). Drones (autonomous, come in light/medium/heavy/sentry).
- **Mining**: Mining lasers extract ore from asteroids in cycles. Ore is refined into minerals at stations. Minerals are the base materials for all manufacturing. Different ore types yield different minerals. Rarer ores found in more dangerous space.
- **Security Zones**: High-sec (safe, low reward), Low-sec (dangerous, better reward), Null-sec (lawless, best reward). Adapting for single player: zone difficulty tiers around the solar system or across systems.
- **Economy**: Player-driven market with buy/sell orders. Regional price differences enable trade routes. Manufacturing transforms raw materials into products.
- **Exploration**: Scanning probes find hidden sites. Sites contain hackable containers, rare loot, wormholes. Risk/reward scaling with space danger level.

### From X4: Foundations
- **Station Building**: Modular station construction with docks, production, storage, habitation modules. Stations are profit centers that produce goods for sale. Worker population affects efficiency.
- **Dynamic Universe**: NPCs trade, fight, and build independently. Factions expand/contract based on economic and military success. The economy is a real simulation, not scripted.
- **Ship Management**: Player can own and command multiple ships. Ships can be assigned tasks (trade, mine, patrol). First-person cockpit view with full ship control. Can board and capture enemy ships.
- **Economy**: Full production chains (raw material -> intermediate -> final product). Supply and demand affect prices. Player factories compete with NPC factories. Trade routes emerge from price differences.
- **Sector Design**: Universe divided into sectors connected by gates/highways. Each sector has its own economy, stations, hazards. Sectors are controlled by different factions.

### Key Mechanics for Single-Player Adaptation
- Replace player-driven economy with NPC faction economies
- Security zones become difficulty tiers (inner system = safe, outer = dangerous)
- Faction standings unlock access to better equipment/stations
- Progression through ship upgrades rather than skill training time
- AI pilots can be hired to crew additional ships
- Dynamic events: pirate raids, faction wars, asteroid discoveries

### Solar System Design
- **Star**: Central body, provides light/heat, hazard zone near surface
- **Inner Planets**: Rocky, higher security, more stations, safer mining
- **Asteroid Belt**: Between inner and outer planets, primary mining zone
- **Outer Planets**: Gas giants with moons, lower security, rarer resources
- **Kuiper Belt / Edge**: Furthest out, most dangerous, best resources
- **Points of Interest**: Stations, gates, anomalies, derelicts, hidden sites
- **Scale**: Use logarithmic or compressed scale (not real distances) for gameplay

### Ship Module Slot System
```
High Slots:    Weapons (turrets, launchers), mining lasers, cloaking
Mid Slots:     Shield modules, propulsion (afterburner, warp), electronic warfare, scanners
Low Slots:     Armor plates, damage mods, cargo expanders, mining upgrades
Rig Slots:     Permanent ship modifications (speed, tank, damage bonuses)
Drone Bay:     Autonomous combat/mining drones (capacity varies by ship)
Cargo Hold:    General storage for items, ore, loot
```

### Ore and Mineral Reference
```
Ore Type        Location        Minerals Yielded         Rarity
─────────────────────────────────────────────────────────────────
Veldspar        Inner (safe)    Tritanium                Common
Scordite        Inner (safe)    Tritanium, Pyerite       Common
Pyroxeres       Inner-Mid       Pyerite, Mexallon        Uncommon
Plagioclase     Mid belt        Tritanium, Mexallon      Uncommon
Kernite         Mid-Outer       Mexallon, Isogen         Moderate
Jaspet          Outer           Mexallon, Zydrine        Rare
Hemorphite      Outer           Isogen, Nocxium          Rare
Hedbergite      Edge            Zydrine, Megacyte        Very Rare
Mercoxit        Edge (hazard)   Morphite                 Extremely Rare
```

### Mineral Hierarchy (used in manufacturing)
```
Tritanium  -> Basic hulls, ammunition, common modules
Pyerite    -> Weapons, electronics
Mexallon   -> Propulsion, shields
Isogen     -> Advanced electronics, sensors
Nocxium    -> Armor plating, advanced weapons
Zydrine    -> Capital components, advanced modules
Megacyte   -> Tech 2 components, capital ships
Morphite   -> Tech 2 ships and modules (rarest)
```
