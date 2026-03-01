# SpaceFighter — Game Design Document

## 1. Overview

**SpaceFighter** is a 2D top-down competitive tactical arena shooter built in Unity with **FishNet Pro** networking. Two teams of five players battle in short, high-stakes rounds where positioning, resource management, and teamwork determine the winner.

| Parameter | Value |
|---|---|
| Genre | 2D Top-Down Tactical Arena Shooter |
| Players | 5 v 5 LAN Multiplayer |
| Round Duration | 75 seconds |
| Respawns | None — single life per round |
| Win Condition | Eliminate all enemy ships **or** capture the Overtime Core |
| Perspective | Top-down 2D |
| Networking | FishNet Pro (Server Authoritative) |

### 1.1 Overtime

When the 75-second timer expires and both teams still have surviving ships, **Overtime** begins:

1. A **Core** object spawns at the center of the arena.
2. A ship must remain within capture range for **2 seconds uninterrupted** to win.
3. Taking **any damage** resets the capture timer.
4. Overtime applies the following **global modifiers**:

| Modifier | Effect |
|---|---|
| Stabilize | **Disabled** — cannot be used during overtime |
| Energy Regeneration | **−30%** (reduced from 15/s to 10.5/s) |
| Vanguard Barrier Cost | **+20%** (increased from 25 energy/s to 30 energy/s) |

---

## 2. Design Philosophy

### 2.1 Core Pillars

| Pillar | Description |
|---|---|
| Skill-Based | Outcomes depend on aim, positioning, and decision-making — never luck. |
| Positioning > Ability Spam | Strong positioning provides more value than burning abilities off cooldown. |
| Heat Prevents Sustained Fire | Every weapon generates heat; unchecked firing results in a lockout. |
| Friendly Fire ON | Players must be aware of teammates' positions at all times. |
| Ship Collision ON | Physical collisions with mass-based knockback add a tactical layer. |

### 2.2 Anti-Patterns

- **No RNG crits** — damage is always deterministic.
- **No ammo / no reload** — resource control is expressed through heat and energy, not magazine counts.
- **No passive healing over time** — the only in-round heal is the active Stabilize ability.

---

## 3. Movement

SpaceFighter uses Unity's **Rigidbody2D** for all ship movement.

| Property | Detail |
|---|---|
| Physics Mode | Rigidbody2D (Dynamic) |
| Momentum / Inertia | Ships accelerate and decelerate; instant direction changes are not possible. |
| Wall Sliding | Ships slide along wall surfaces rather than stopping dead on contact. |
| Ship-to-Ship Collision | **Soft collision** — ships push each other based on relative mass; no hard stop. |

### 3.1 Dash

| Parameter | Value |
|---|---|
| Charges | 2 |
| Energy Cost | 25 per dash |
| Recharge Time | 1.5 s per charge |
| Burst Velocity | 12 units/s |
| Direction | Movement direction (or facing direction if stationary) |

Dashing grants a brief burst of velocity. It does **not** provide invincibility frames.

---

## 4. Resources

### 4.1 Energy

| Parameter | Value |
|---|---|
| Maximum | 100 |
| Base Regeneration | 15 / s |
| Regen During Damage | **0** (paused while taking damage) |
| Overtime Regen Penalty | −30% → 10.5 / s |

Energy powers abilities and dashes. Managing energy is central to tactical play — spamming abilities leaves a ship unable to dash or heal.

### 4.2 Heat

| Parameter | Value |
|---|---|
| Maximum | 100 |
| Overheat Lockout | 1.2 s (weapon disabled) |
| Decay Delay | ~0.4 s after last shot |
| Full Decay Time | ~1.5 s from 100 to 0 |

Heat is generated per shot. Different weapons generate heat at different rates. Exceeding 100 heat triggers a **lockout** during which the weapon cannot fire.

### 4.3 Stabilize

| Parameter | Value |
|---|---|
| Availability | All classes **except** Vanguard |
| Uses per Round | 1 |
| Channel Time | 1.5 s |
| HP Restored | 60% of max HP |
| Energy Cost | 50 |
| Interrupted By | Any damage received |
| Overtime | **Disabled** |

Stabilize is a high-risk, high-reward self-heal. The 1.5-second channel leaves the ship vulnerable, and any incoming damage (including friendly fire) cancels the heal and still consumes the single-use charge.

---

## 5. Ship Classes

SpaceFighter features four distinct ship classes. Each team selects their composition before the round starts, subject to the restrictions in §5.5.

### 5.1 Vanguard — Frontline Tank

> *Hold the line. Absorb pressure. Create space for your team.*

#### Base Stats

| Stat | Value |
|---|---|
| HP | 220 |
| Damage Reduction | 20% passive |
| Knockback Resistance | 25% |
| Stabilize | **Not available** |

#### Primary Weapon — Pulse Cannon

| Parameter | Value |
|---|---|
| Damage per Shot | 16 |
| Shots to Overheat | 9 |
| Heat per Shot | ~11.1 |
| Projectile Speed | Medium |

#### Ability 1 — Active Barrier

| Parameter | Value |
|---|---|
| Arc | 180° frontal cone |
| Damage Reduction | 99% to incoming projectiles |
| Energy Cost | 25 / s while active |
| Maximum Duration | 4 s |
| Cooldown | 6 s (starts after deactivation) |
| Rear Module HP | 70 (destroying it disables the barrier) |

The Active Barrier blocks nearly all frontal damage but drains energy rapidly. Enemies can destroy the rear-mounted barrier module (70 HP) to disable the barrier entirely for the remainder of the round.

#### Ability 2 — Shield Bash

| Parameter | Value |
|---|---|
| Energy Cost | 35 |
| Cooldown | 8 s |
| Damage | 20 |
| Effect | Strong knockback in facing direction |

Shield Bash is a close-range displacement tool used to peel enemies or push them into unfavorable positions.

---

### 5.2 Striker — Versatile DPS

> *Sustained pressure with burst potential. The backbone of any team composition.*

#### Base Stats

| Stat | Value |
|---|---|
| HP | 100 |
| Damage Reduction | None |
| Knockback Resistance | None |
| Stabilize | 1× per round |

#### Primary Weapon — Burst Rifle

| Parameter | Value |
|---|---|
| Fire Mode | 3-shot burst |
| Damage per Hit | 12 |
| Damage per Burst | 36 (if all 3 connect) |
| Heat per Burst | Moderate |

#### Ability 1 — Overdrive

| Parameter | Value |
|---|---|
| Duration | 4 s |
| Fire Rate Bonus | +25% |
| Heat Generation Penalty | +20% |
| Energy Cost | 40 |
| Cooldown | 10 s |

Overdrive trades heat efficiency for burst damage output. Using it recklessly can lead to an overheat lockout mid-fight.

#### Ability 2 — Ricochet Shot

| Parameter | Value |
|---|---|
| Wall Bounces | 1 |
| Damage | 35% of target's max HP |
| Energy Cost | 45 |
| Cooldown | 12 s |

Ricochet Shot fires a single projectile that bounces off one wall. It deals percentage-based damage, making it equally effective against all ship classes.

---

### 5.3 Disruptor — Anti-Tank Specialist

> *Pierce defenses. Disable barriers. Punish overextension.*

#### Base Stats

| Stat | Value |
|---|---|
| HP | 100 |
| Damage Reduction | None |
| Knockback Resistance | None |
| Stabilize | 1× per round |
| **Team Limit** | **Max 2 per team** |

#### Primary Weapon — Precision Bolt

| Parameter | Value |
|---|---|
| Damage per Shot | 18 |
| Shots to Overheat | 8 |
| Heat per Shot | ~12.5 |
| Projectile Speed | Fast |

#### Ability 1 — Pierce Cannon

| Parameter | Value |
|---|---|
| Charge Time | 2.5 s |
| Move Speed While Charging | 30% of normal |
| Energy Cost | 60 |
| Cooldown | 14 s |
| Barrier Interaction | **Ignores** Active Barrier entirely |

| Target | Damage | Stun |
|---|---|---|
| Standard ship (100 HP) | 60% of max HP → **60 damage** | 0.4 s stun |
| Vanguard (220 HP) | 30% of max HP → **66 damage** | No stun |

Pierce Cannon is the primary counter to Vanguard's Active Barrier. The long charge time and severe movement penalty make positioning critical.

#### Ability 2 — EMP Mine

| Parameter | Value |
|---|---|
| Arm Time | 1 s after deployment |
| Effect | Disables Active Barrier for 1.5 s |
| Energy Cost | 45 |
| Cooldown | 14 s |

EMP Mine is a zoning tool. Placing it in chokepoints forces Vanguards to either avoid the area or risk losing their barrier at a critical moment.

---

### 5.4 Flanker — Mobile Assassin

> *Strike from the shadows. Exploit gaps. Vanish before retaliation.*

#### Base Stats

| Stat | Value |
|---|---|
| HP | 100 |
| Agility | **Highest** of all classes |
| Damage Reduction | None |
| Knockback Resistance | None |
| Stabilize | 1× per round |

#### Primary Weapon — Spread Shot

| Parameter | Value |
|---|---|
| Pellets per Shot | 5 |
| Damage per Pellet | 8 |
| Max Damage per Shot | 40 (all pellets hit) |
| Shots to Overheat | 7 |
| Spread Pattern | Fan / cone |

#### Ability 1 — Blink

| Parameter | Value |
|---|---|
| Type | Instant teleport |
| Energy Cost | 35 per use |
| Charges | 2 |
| Recharge Time | 8 s per charge |

Blink instantly teleports the Flanker a fixed distance in the movement direction. It can cross walls and obstacles.

#### Ability 2 — Cloak

| Parameter | Value |
|---|---|
| Duration | 1.5 s |
| Restrictions | Cannot fire while cloaked |
| Energy Cost | 50 |
| Cooldown | 12 s |

Cloak renders the Flanker invisible for 1.5 seconds. Any offensive action breaks the cloak immediately. Primarily used for repositioning or escaping.

---

### 5.5 Team Composition Rules

| Rule | Constraint |
|---|---|
| Max Vanguards per team | 2 |
| Max Disruptors per team | 2 |
| Max Strikers per team | No limit (up to 5) |
| Max Flankers per team | No limit (up to 5) |
| Team size | 5 players |

---

## 6. Combat System

### 6.1 General Rules

| Rule | Detail |
|---|---|
| Friendly Fire | **ON** — all projectiles damage allied ships |
| Critical Hits | **None** — no RNG-based damage modifiers |
| Ammo / Reload | **None** — weapons are heat-limited, not ammo-limited |
| Projectile Identification | Team-colored projectiles (e.g., blue vs. red) |
| Micro-Knockback | All projectile hits apply a small knockback impulse |

### 6.2 Damage Pipeline

```
Raw Damage
  → Apply target Damage Reduction (e.g., Vanguard 20% DR)
  → Apply Active Barrier (99% DR if frontal + barrier active)
  → Apply final damage to HP
  → Apply micro-knockback (reduced by Knockback Resistance)
  → If target HP ≤ 0 → Destroy ship
```

### 6.3 Heat Management Reference

| Class | Weapon | Heat/Shot | Shots to Overheat | Lockout |
|---|---|---|---|---|
| Vanguard | Pulse Cannon | ~11.1 | 9 | 1.2 s |
| Striker | Burst Rifle | per burst | varies | 1.2 s |
| Disruptor | Precision Bolt | ~12.5 | 8 | 1.2 s |
| Flanker | Spread Shot | ~14.3 | 7 | 1.2 s |

---

## 7. Networking Architecture

### 7.1 Authority Model

SpaceFighter uses a **server-authoritative** model for all gameplay-critical systems:

| System | Authority |
|---|---|
| Ship Movement | Server (client-side prediction with reconciliation) |
| Projectile Spawning | Server |
| Damage Calculation | Server |
| Ability Activation | Server (client sends input; server validates and executes) |
| Heat / Energy | Server |
| Round State | Server |
| Core Capture | Server |

### 7.2 LAN Lobby

| Feature | Detail |
|---|---|
| Discovery | LAN broadcast (Tugboat transport) |
| Lobby Flow | Host creates → clients join → team/class select → host starts round |
| Host Role | Acts as both **server** and **client** simultaneously |

### 7.3 FishNet Specifics

- **NetworkObject** on every networked prefab (ships, projectiles, mines, core).
- **NetworkBehaviour** scripts use `[ServerRpc]` for client→server calls and `[ObserversRpc]` / `[TargetRpc]` for server→client calls.
- **SyncVar** for replicated state (HP, energy, heat, alive/dead, team, class).
- **TimeManager.Tick** used for deterministic fixed-timestep logic.

---

## 8. Balancing Reference Tables

### 8.1 Ship Comparison

| Stat | Vanguard | Striker | Disruptor | Flanker |
|---|---|---|---|---|
| HP | 220 | 100 | 100 | 100 |
| Passive DR | 20% | 0% | 0% | 0% |
| Knockback Resist | 25% | 0% | 0% | 0% |
| Agility | Low | Medium | Medium | **High** |
| Stabilize | No | Yes (1×) | Yes (1×) | Yes (1×) |

### 8.2 Weapon Comparison

| Weapon | Class | Damage | Shots to Overheat | Notes |
|---|---|---|---|---|
| Pulse Cannon | Vanguard | 16 / shot | 9 | Steady DPS |
| Burst Rifle | Striker | 12 / hit (36 / burst) | varies | 3-shot burst |
| Precision Bolt | Disruptor | 18 / shot | 8 | Highest per-shot |
| Spread Shot | Flanker | 8 × 5 (40 max) | 7 | Close-range burst |

### 8.3 Ability Comparison

| Ability | Class | Energy | Cooldown | Key Effect |
|---|---|---|---|---|
| Active Barrier | Vanguard | 25/s | 6 s | 180° frontal, 99% DR |
| Shield Bash | Vanguard | 35 | 8 s | 20 dmg + knockback |
| Overdrive | Striker | 40 | 10 s | +25% fire rate for 4 s |
| Ricochet Shot | Striker | 45 | 12 s | 35% max HP, 1 bounce |
| Pierce Cannon | Disruptor | 60 | 14 s | Ignores barrier, % HP dmg |
| EMP Mine | Disruptor | 45 | 14 s | 1.5 s barrier disable |
| Blink | Flanker | 35 | 8 s (2 charges) | Instant teleport |
| Cloak | Flanker | 50 | 12 s | 1.5 s invisibility |

### 8.4 Resource Rates

| Resource | Base Value | Overtime Value | Notes |
|---|---|---|---|
| Energy Regen | 15 / s | 10.5 / s (−30%) | Paused while taking damage |
| Heat Decay | ~1.5 s full decay | unchanged | ~0.4 s delay before decay starts |
| Dash Recharge | 1.5 s / charge | unchanged | 2 max charges, 25 energy each |
| Stabilize | 1× per round | **Disabled** | 1.5 s channel, 60% HP heal |

### 8.5 Overtime Modifiers Summary

| Modifier | Normal | Overtime |
|---|---|---|
| Round Timer | 75 s | Unlimited (until capture) |
| Energy Regen | 15 / s | 10.5 / s |
| Vanguard Barrier Cost | 25 energy/s | 30 energy/s |
| Stabilize | Available (1×) | Disabled |
| Core Capture | N/A | 2 s uninterrupted |
