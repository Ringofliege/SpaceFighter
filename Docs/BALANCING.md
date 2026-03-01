# SpaceFighter – Balancing Guide

## How to Tweak Values Safely

All balance values live in `Assets/Scripts/Core/GameConstants.cs`. Change values there and they apply globally.

### Recommended Workflow
1. Change ONE value at a time
2. Test in a 1v1 or 2v2 local match
3. Note the impact before changing more

## Ship Balance Reference

| Stat | Vanguard | Striker | Disruptor | Flanker |
|------|----------|---------|-----------|---------|
| HP | 220 | 100 | 100 | 100 |
| Damage Reduction | 20% | 0% | 0% | 0% |
| Knockback Resist | 25% | 0% | 0% | 0% |
| Speed Multiplier | 105% | 100% | 100% | 100% |
| Acceleration | 100% | 100% | 100% | 150% |
| Primary DPS (approx) | 64/s | 90/s | 51/s | 133/s (close) |

## Weapon Tuning

| Weapon | Damage | Fire Rate | Heat/Shot | Overheat Shots |
|--------|--------|-----------|-----------|----------------|
| Pulse Cannon | 16 | 0.25s | 11.11 | 9 |
| Burst Rifle | 12×3 | 0.4s burst | 14.29 | 7 bursts |
| Precision Bolt | 18 | 0.35s | 12.5 | 8 |
| Spread Shot | 8×5 | 0.3s | 14.29 | 7 |

## Ability Tuning

| Ability | Energy | Cooldown | Duration | Key Value |
|---------|--------|----------|----------|-----------|
| Active Barrier | 25/s | 6s | 4s max | 99% DR, Rear 70 HP |
| Shield Bash | 35 | 8s | instant | 20 dmg + knockback |
| Overdrive | 40 | 10s | 4s | +25% fire rate |
| Ricochet Shot | 45 | 12s | instant | 35% MaxHP |
| Pierce Cannon | 60 | 14s | 2.5s charge | 60%/30% MaxHP |
| EMP Mine | 45 | 14s | until triggered | 1.5s barrier disable |
| Blink | 35 | 8s/charge | instant | 2 charges, 6u range |
| Cloak | 50 | 12s | 1.5s | No fire allowed |
| Stabilize | 50 | 1x/round | 1.5s channel | 60% HP heal |

## Common Balance Levers

- **Vanguard too tanky**: Reduce HP (200), reduce DR (15%), increase barrier energy cost
- **Striker too dominant**: Reduce Overdrive duration (3s), increase burst heat
- **Disruptor Pierce too strong**: Increase charge time (3s), reduce damage % (50/25)
- **Flanker too slippery**: Reduce Blink charges to 1, increase Cloak cooldown (15s)
- **Games too short**: Increase all HP by 10-20%
- **Games too long**: Reduce round timer (60s), increase damage across board
- **Overtime too decisive**: Increase CoreCaptureTime (3s)
- **Energy too abundant**: Reduce regen (12/s), increase ability costs
