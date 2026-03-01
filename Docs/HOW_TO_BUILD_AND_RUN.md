# SpaceFighter — How to Build and Run

This document covers building the game, running host and client instances, local testing workflows, and common pitfalls.

---

## 1. How to Build

### 1.1 Configure Build Settings

1. Open `File → Build Settings`.
2. Add all three scenes in order:

| Index | Scene |
|---|---|
| 0 | `Assets/Scenes/Boot.unity` |
| 1 | `Assets/Scenes/Lobby.unity` |
| 2 | `Assets/Scenes/Game.unity` |

3. Select your target platform (e.g., **Windows, Mac, Linux**).
4. Click **Player Settings** and configure:

| Setting | Value |
|---|---|
| Product Name | SpaceFighter |
| Resolution | 1920 × 1080 (or desired) |
| Fullscreen Mode | Windowed (recommended for testing) |
| Run In Background | **ON** (critical for multiplayer) |

> **Run In Background must be ON.** Without it, the game pauses when the window loses focus, which breaks networking for the host.

### 1.2 Build the Executable

1. Click **Build** (or **Build and Run**).
2. Choose an output folder (e.g., `Builds/Windows/`).
3. Wait for the build to complete.

The output folder will contain:
```
Builds/
└── Windows/
    ├── SpaceFighter.exe
    ├── SpaceFighter_Data/
    ├── MonoBleedingEdge/
    └── UnityPlayer.dll
```

---

## 2. Running the Game

### 2.1 Host (Server + Client)

The host acts as both the **server** and a **playing client**.

1. Launch `SpaceFighter.exe` (or press Play in the Unity Editor).
2. On the Boot screen, click **Host**.
3. FishNet starts the server via `ServerManager.StartConnection()` and simultaneously connects the local client via `ClientManager.StartConnection()`.
4. The Lobby scene loads. The host can select a team and class.
5. Once all players are ready, the host clicks **Start** to begin the round.

```
Host machine
┌────────────────────────┐
│  FishNet Server         │
│  + Local Client (Host)  │
│  Listening on port 7770 │
└────────────────────────┘
```

### 2.2 Client (LAN Connection)

1. Launch `SpaceFighter.exe` on another machine on the **same LAN**.
2. On the Boot screen, enter the **host's LAN IP address** (e.g., `192.168.1.10`).
3. Click **Join**.
4. FishNet connects via `ClientManager.StartConnection(hostIP)`.
5. The Lobby scene loads. The client selects a team and class, then clicks **Ready**.

**Finding the host IP:**
- Windows: Open Command Prompt → `ipconfig` → look for `IPv4 Address` under the active network adapter.
- macOS/Linux: Open Terminal → `ifconfig` or `ip addr` → look for `inet` under the active interface.

---

## 3. Local Multi-Instance Testing

During development, you need multiple game instances on a single machine. There are two approaches.

### 3.1 ParrelSync (Recommended for Editor Testing)

[ParrelSync](https://github.com/VeriorPies/ParrelSync) creates linked clones of your Unity project that share the same Assets folder.

#### Setup

1. Install ParrelSync:
   - Download the latest `.unitypackage` from the [Releases page](https://github.com/VeriorPies/ParrelSync/releases).
   - Import into your project via `Assets → Import Package → Custom Package`.
2. Open `ParrelSync → Clones Manager` from the Unity menu bar.
3. Click **Create New Clone**. This creates a lightweight copy of the project.
4. Click **Open in New Editor** to launch the clone.

#### Testing Workflow

| Instance | Role | How to Launch |
|---|---|---|
| Original Editor | Host | Press Play → click **Host** |
| ParrelSync Clone | Client 1 | Press Play → enter `127.0.0.1` → click **Join** |
| (Optional) 2nd Clone | Client 2 | Same as above |

> **Tip:** Both editors share the same `Assets/` folder. Code changes in one are immediately reflected in the other — no need to reimport.

### 3.2 Multiple Builds

If you cannot use ParrelSync (e.g., testing release builds):

1. Build the project (see §1).
2. Launch the first instance → click **Host**.
3. Launch additional instances → enter `127.0.0.1` → click **Join**.

> **Important:** Ensure **Run In Background** is enabled in Player Settings, or background instances will freeze.

### 3.3 Mixed Editor + Build

You can also run the Editor as the host and a standalone build as the client (or vice versa):

1. Press Play in the Editor → click **Host**.
2. Launch `SpaceFighter.exe` → enter `127.0.0.1` → click **Join**.

This is useful for rapid iteration — you can modify code and test in the Editor while the build acts as a second player.

---

## 4. Common Pitfalls

### 4.1 FishNet Authority: `IsOwner` vs. `IsServer`

This is the most common source of bugs in FishNet projects.

| Property | True When | Use For |
|---|---|---|
| `base.IsOwner` | The local client owns this NetworkObject | Reading input, controlling the camera |
| `base.IsServer` | This code is running on the server | Applying damage, spawning projectiles, modifying SyncVars |
| `base.IsClient` | This code is running on any client | Rendering effects, playing sounds |
| `base.IsHost` | This machine is both server and client | Rarely needed — avoid using as a condition |

**Common mistake:** Applying damage or modifying HP inside an `IsOwner` block. This allows clients to cheat. **Always** modify gameplay state inside `IsServer` blocks or `[ServerRpc]` handlers.

```csharp
// ❌ WRONG — client-authoritative damage
private void OnTriggerEnter2D(Collider2D other)
{
    if (!base.IsOwner) return;
    health -= 20; // Client can modify this value
}

// ✅ CORRECT — server-authoritative damage
[ServerRpc(RequireOwnership = false)]
private void TakeDamageServerRpc(int amount)
{
    if (!base.IsServer) return;
    _health.Value -= amount; // SyncVar, server-only
}
```

### 4.2 Physics Jitter: Use `FixedUpdate`

**Problem:** Moving Rigidbody2D objects in `Update()` causes jitter because `Update` runs at a variable frame rate while physics runs at a fixed timestep.

**Solution:**
- Apply forces and velocity changes in `FixedUpdate()`.
- Read input in `Update()` and store it in variables; consume those variables in `FixedUpdate()`.
- Enable **Interpolation** on Rigidbody2D components to smooth visual positions between physics steps.

```csharp
private Vector2 _moveInput;

private void Update()
{
    if (!base.IsOwner) return;
    _moveInput = _inputActions.Player.Move.ReadValue<Vector2>();
}

private void FixedUpdate()
{
    if (!base.IsOwner) return;
    _rb.AddForce(_moveInput * _moveSpeed);
}
```

> **FishNet Note:** For server-reconciled movement, consider using FishNet's `TimeManager.TickDelta` in your fixed-timestep logic to stay in sync with the network tick rate.

### 4.3 Spawning Order

**Problem:** A `NetworkBehaviour`'s `OnStartClient` or `OnStartNetwork` fires before all SyncVars have been synchronized, causing null references or stale data.

**Solution:**
- Use `public override void OnStartClient()` to initialize client-side visuals **after** the object is fully spawned.
- For SyncVar callbacks, use `[SyncVar(OnChange = nameof(OnHealthChanged))]` style hooks to react to specific value changes rather than polling in `Update`.
- Do **not** access other NetworkObjects in `Awake()` — they may not be spawned yet.

### 4.4 Prefab Registration

**Problem:** Calling `ServerManager.Spawn(prefab)` results in an error or the object simply not appearing on clients.

**Solution:**
1. Ensure the prefab has a `NetworkObject` component.
2. Ensure the prefab is listed in the NetworkManager's **Prefab Objects → Default Collection**.
3. If you add new prefabs at runtime via addressables or asset bundles, register them with `NetworkManager.PrefabObjects.AddObject()`.

### 4.5 Scene Loading with FishNet

**Problem:** Using Unity's `SceneManager.LoadScene()` directly causes desynchronization between server and clients.

**Solution:** Always use FishNet's scene management API:

```csharp
// Load a scene for all connected clients
SceneLoadData sld = new SceneLoadData("Game");
sld.ReplaceScenes = ReplaceOption.All;
base.SceneManager.LoadGlobalScenes(sld);
```

This ensures the server and all clients load scenes in sync and that NetworkObjects are properly managed during transitions.

---

## 5. Troubleshooting

### 5.1 Connection Issues

| Symptom | Possible Cause | Fix |
|---|---|---|
| Client cannot connect | Wrong IP address | Verify host IP with `ipconfig` / `ifconfig` |
| Client cannot connect | Firewall blocking port 7770 | Add inbound rule for UDP port 7770 |
| Client cannot connect | Different subnets | Ensure both machines are on the same LAN/VLAN |
| Connection drops immediately | FishNet version mismatch | Ensure all builds use the same FishNet version |
| "Transport not set" error | Tugboat not assigned | Assign Tugboat on NetworkManager's Transport field |

### 5.2 Gameplay Issues

| Symptom | Possible Cause | Fix |
|---|---|---|
| Ship doesn't move | Input read outside `IsOwner` guard | Wrap input reading with `if (!base.IsOwner) return;` |
| Ship moves on one client only | Movement not using NetworkTransform or server-sync | Add `NetworkTransform` or implement server-reconciled movement |
| Projectiles don't appear on clients | Prefab not registered | Add to NetworkManager Prefab Objects |
| Damage not applied | Damage logic in `IsOwner` instead of `IsServer` | Move damage to `[ServerRpc]` or `IsServer` block |
| Physics jitter | Rigidbody modified in `Update()` | Move physics calls to `FixedUpdate()`, enable interpolation |
| SyncVar not updating visually | No OnChange callback | Add `[SyncVar(OnChange = nameof(...))]` and update UI in callback |
| Overtime Core not spawning | Core prefab not registered or spawn logic missing | Register prefab; verify `RoundManager` spawns core on overtime |

### 5.3 Build Issues

| Symptom | Possible Cause | Fix |
|---|---|---|
| Build crashes on launch | Scenes not in Build Settings | Add all 3 scenes to `File → Build Settings` |
| Game freezes in background | Run In Background disabled | Enable in `Player Settings → Resolution and Presentation` |
| "Type not found" errors | Assembly definition conflicts | Ensure FishNet asmdef references are correct |
| Input not working in build | Old Input Manager selected | Set `Active Input Handling` to **Both** or **Input System Package (New)** in Player Settings |

---

## 6. Quick Reference: Commands & Shortcuts

| Action | How |
|---|---|
| Build | `File → Build Settings → Build` |
| Host a game | Launch → click **Host** |
| Join a game | Launch → enter host IP → click **Join** |
| Test locally (Editor) | Install ParrelSync → create clone → run both editors |
| Test locally (Builds) | Build once → launch multiple `.exe` instances |
| Find host IP (Windows) | `ipconfig` in Command Prompt |
| Find host IP (Linux/Mac) | `ifconfig` or `ip addr` in Terminal |
| Check FishNet logs | Open Unity Console → filter by "FishNet" |
