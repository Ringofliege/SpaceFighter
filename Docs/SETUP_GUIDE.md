# SpaceFighter — Unity Setup Guide

This guide walks through setting up the SpaceFighter project from scratch in Unity. It is written for beginners and assumes no prior Unity networking experience.

---

## 1. Project Setup

### 1.1 Install Unity

1. Download and install **Unity Hub** from [unity.com](https://unity.com).
2. In Unity Hub, install **Unity 6.3 LTS** (version **6000.3.10f1** or later 6000.3.x patch).
3. During installation, ensure the **Windows Build Support** (or your target platform) module is selected.

### 1.2 Create the Project

1. Open Unity Hub → **New Project**.
2. Select the **2D (Built-in Render Pipeline)** template.
3. Name the project `SpaceFighter`.
4. Choose a location and click **Create project**.

> **Why 2D template?** SpaceFighter is a top-down 2D game. The 2D template pre-configures the camera, physics, and sprite defaults.

---

## 2. FishNet Pro Setup

### 2.1 Import FishNet Pro

1. Open the **Unity Asset Store** window (`Window → Asset Store`) or visit [assetstore.unity.com](https://assetstore.unity.com).
2. Search for **FishNet Pro** (by FirstGearGames).
3. Purchase / download and click **Import** into your project.
4. Accept all default import paths.

### 2.2 Configure the NetworkManager

1. In your **Boot** scene (see §4), create an empty GameObject named `NetworkManager`.
2. Add the following components:
   - **NetworkManager** (FishNet) — core networking component.
   - **Tugboat** — FishNet's built-in UDP transport, ideal for LAN play.
3. Configure Tugboat:

| Setting | Recommended Value | Notes |
|---|---|---|
| Port | 7770 | Default FishNet port |
| Maximum Clients | 10 | 5v5 = 10 players |
| IPv4 Bind Address | 0.0.0.0 | Accept connections on all interfaces |

4. On the NetworkManager component, assign Tugboat as the **Transport**.

### 2.3 Prefab Registration

Every networked prefab (ships, projectiles, mines, core) must be registered:

1. Select the **NetworkManager** GameObject.
2. In the **NetworkManager** component, expand **Prefab Objects** → **Default Collection**.
3. Drag each networked prefab into the list.

> **Important:** If a prefab is not registered, `ServerManager.Spawn()` calls will fail silently at runtime.

---

## 3. Folder Structure

Organize the project using the following layout:

```
Assets/
├── Prefabs/
│   ├── Ships/
│   │   ├── Vanguard.prefab
│   │   ├── Striker.prefab
│   │   ├── Disruptor.prefab
│   │   └── Flanker.prefab
│   ├── Projectiles/
│   │   ├── PulseCannon_Projectile.prefab
│   │   ├── BurstRifle_Projectile.prefab
│   │   ├── PrecisionBolt_Projectile.prefab
│   │   ├── SpreadShot_Pellet.prefab
│   │   ├── RicochetShot_Projectile.prefab
│   │   └── PierceCannon_Projectile.prefab
│   ├── Abilities/
│   │   └── EMP_Mine.prefab
│   ├── Environment/
│   │   ├── Asteroid.prefab
│   │   └── OvertimeCore.prefab
│   └── UI/
│       ├── HUD.prefab
│       ├── LobbyPanel.prefab
│       ├── ScoreboardPanel.prefab
│       └── RoundTimerUI.prefab
├── Scenes/
│   ├── Boot.unity
│   ├── Lobby.unity
│   └── Game.unity
├── Scripts/
│   ├── Networking/
│   ├── Player/
│   ├── Combat/
│   ├── Abilities/
│   ├── UI/
│   ├── GameState/
│   └── Utils/
├── Sprites/
│   ├── Ships/
│   ├── Projectiles/
│   ├── Environment/
│   └── UI/
├── Audio/
│   ├── SFX/
│   └── Music/
├── Animations/
└── ScriptableObjects/
    ├── ShipData/
    └── WeaponData/
```

---

## 4. Scenes

SpaceFighter uses three scenes loaded in sequence.

### 4.1 Boot Scene

**Purpose:** Network bootstrap — initializes FishNet and decides host vs. client.

| Contents | Notes |
|---|---|
| `NetworkManager` GameObject | With Tugboat transport attached |
| `BootManager` script | Provides Host / Join UI and triggers `ServerManager.StartConnection()` or `ClientManager.StartConnection()` |
| Main Camera | Splash / title screen |

**Build Index:** 0 (must be the first scene in Build Settings).

### 4.2 Lobby Scene

**Purpose:** Team selection and class selection before the round starts.

| Contents | Notes |
|---|---|
| `LobbyManager` (NetworkBehaviour) | Tracks connected players, team assignments, class selections, ready states |
| Team panels (UI) | Two columns showing Team A and Team B rosters |
| Class selection buttons | Vanguard, Striker, Disruptor, Flanker — enforces team composition limits (max 2 Vanguard, max 2 Disruptor) |
| Ready / Start button | Host sees "Start" (requires all players ready); clients see "Ready" |

Scene is loaded via `SceneManager.LoadGlobalScenes()` (FishNet) after a successful connection in Boot.

### 4.3 Game Scene

**Purpose:** The arena where gameplay takes place.

| Contents | Notes |
|---|---|
| Arena tilemap / walls | Define the playable area and collision boundaries |
| Spawn points (empty GameObjects) | 5 per team, placed symmetrically |
| `RoundManager` (NetworkBehaviour) | Timer, round state machine, overtime logic |
| `OvertimeCore` spawn point | Center of the arena |
| HUD (Canvas) | HP bar, energy bar, heat bar, ability cooldowns, round timer |

Scene is loaded as an additive scene via `SceneManager.LoadConnectionScenes()` when the host starts the round.

---

## 5. Prefabs

### 5.1 Ship Prefabs

Each ship prefab requires the following component stack:

| Component | Purpose |
|---|---|
| `SpriteRenderer` | Visual representation |
| `Rigidbody2D` | Physics-based movement (Dynamic, continuous collision) |
| `Collider2D` (CircleCollider2D or PolygonCollider2D) | Ship collision shape |
| `NetworkObject` | FishNet networked identity |
| `ShipController` (NetworkBehaviour) | Movement, input, and state management |
| `WeaponController` (NetworkBehaviour) | Primary weapon firing, heat tracking |
| `AbilityController` (NetworkBehaviour) | Ability activation and cooldowns |
| `HealthComponent` (NetworkBehaviour) | HP, damage reception, death |
| `EnergyComponent` (NetworkBehaviour) | Energy pool and regeneration |

Create one prefab per class: **Vanguard**, **Striker**, **Disruptor**, **Flanker**. Use ScriptableObjects (`ShipData`) to differentiate stats (HP, speed, DR, etc.).

### 5.2 Projectile Prefabs

| Component | Purpose |
|---|---|
| `SpriteRenderer` | Visual (team-colored) |
| `Rigidbody2D` | Movement (Kinematic) |
| `Collider2D` (trigger) | Hit detection |
| `NetworkObject` | Networked identity |
| `ProjectileController` (NetworkBehaviour) | Damage, lifetime, ricochet logic |

### 5.3 Other Networked Prefabs

| Prefab | Key Components |
|---|---|
| `EMP_Mine` | NetworkObject, Collider2D (trigger), `EMPMineController` |
| `OvertimeCore` | NetworkObject, Collider2D (trigger), `CoreCaptureController` |
| `Asteroid` | NetworkObject, Rigidbody2D (Static), Collider2D, `AsteroidHealth` |

---

## 6. Layers & Collision Matrix

### 6.1 Layer Definitions

| Layer | Index | Used By |
|---|---|---|
| Default | 0 | General objects, UI |
| Player | 8 | All ship prefabs |
| Projectile | 9 | All projectile prefabs |
| Wall | 10 | Arena walls, tilemap colliders |
| Asteroid | 11 | Destructible asteroid obstacles |
| Core | 12 | Overtime Core object |

### 6.2 Collision Matrix

Configure in `Edit → Project Settings → Physics 2D → Layer Collision Matrix`:

|  | Default | Player | Projectile | Wall | Asteroid | Core |
|---|---|---|---|---|---|---|
| **Default** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Player** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Projectile** | ❌ | ✅ | ❌ | ✅ | ✅ | ❌ |
| **Wall** | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Asteroid** | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Core** | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |

Key decisions:
- **Projectile ↔ Projectile: OFF** — projectiles pass through each other.
- **Projectile ↔ Core: OFF** — shooting the core does nothing.
- **Player ↔ Player: ON** — soft ship-to-ship collision with mass-based knockback.
- **Player ↔ Core: ON** — required for capture-range trigger detection.

---

## 7. Input System

SpaceFighter currently uses Unity's **legacy Input Manager** (`UnityEngine.Input`). This is fully supported in Unity 6.3 LTS.

In **Player Settings → Other Settings → Active Input Handling**, select **Both** to ensure the legacy Input Manager is available alongside the new Input System package (which is included in the manifest for future migration).

### 7.1 Input Bindings

| Action | Key / Button | API Call |
|---|---|---|
| Move | `WASD` | `Input.GetKey(KeyCode.W/A/S/D)` |
| Aim | Mouse Position | `Camera.main.ScreenToWorldPoint(Input.mousePosition)` |
| Fire | Left Mouse Button | `Input.GetMouseButton(0)` |
| Ability 1 | Right Mouse Button | `Input.GetMouseButtonDown(1)` |
| Ability 2 | `Q` | `Input.GetKeyDown(KeyCode.Q)` |
| Dash | `Space` | `Input.GetKeyDown(KeyCode.Space)` |
| Stabilize | `E` | `Input.GetKeyDown(KeyCode.E)` |

### 7.2 Optional: Migrate to Input System Package

To migrate to Unity's new Input System package (`com.unity.inputsystem`):

1. Install the Input System package (already in `Packages/manifest.json`).
2. Create an **Input Actions** asset: `Assets/Input/PlayerActions.inputactions`.
3. Define an action map matching the bindings above.
4. Generate a C# class: enable **Generate C# Class** in the asset inspector.
5. Replace `Input.GetKey` calls with the generated action class in each player script.

> **Tip:** Only process input when `base.IsOwner` is true — this prevents one client from controlling another player's ship.

---

## 8. Tags

Define the following tags in `Edit → Project Settings → Tags and Layers`:

| Tag | Applied To |
|---|---|
| `Player` | All ship prefabs |
| `Projectile` | All projectile prefabs |
| `Wall` | Arena wall objects |
| `Asteroid` | Asteroid prefabs |
| `Core` | Overtime Core prefab |
| `SpawnPoint` | Spawn point GameObjects |
| `Mine` | EMP Mine prefab |

Tags are primarily used in `OnTriggerEnter2D` / `OnCollisionEnter2D` callbacks for quick identification:

```csharp
private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Projectile"))
    {
        // Handle hit
    }
}
```

---

## 9. Quick-Start Checklist

Use this checklist to verify your project is correctly configured:

- [ ] Unity 6.3 LTS (6000.3.x) installed
- [ ] 2D template project created
- [ ] FishNet Pro imported from Asset Store
- [ ] Input System package installed
- [ ] NetworkManager GameObject in Boot scene with Tugboat transport
- [ ] Three scenes created: Boot, Lobby, Game
- [ ] All scenes added to Build Settings (Boot at index 0)
- [ ] Layers created: Player (8), Projectile (9), Wall (10), Asteroid (11), Core (12)
- [ ] Collision matrix configured per §6.2
- [ ] Tags created per §8
- [ ] Ship prefabs created with required component stack
- [ ] All networked prefabs registered in NetworkManager's Prefab Objects
- [ ] Input Actions asset created with PlayerActions map
- [ ] Folder structure matches §3
