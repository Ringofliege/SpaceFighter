namespace SpaceFighter
{
    public static class GameConstants
    {
        // Ship HP
        public const int VanguardHP = 220;
        public const int DefaultHP = 100;

        // Damage Reduction
        public const float VanguardDR = 0.2f;
        public const float KnockbackResist = 0.25f;

        // Movement
        public const float BaseSpeed = 6f;
        public const float VanguardSpeedMult = 1.05f;
        public const float VanguardBarrierSpeedMult = 0.7f;
        public const float FlankerAccelMult = 1.5f;
        public const float BaseAcceleration = 25f;

        // Energy
        public const int EnergyMax = 100;
        public const float EnergyRegen = 15f;
        public const float OvertimeRegenMult = 0.7f;
        public const float DamageLockoutDuration = 0.5f;

        // Heat
        public const float HeatMax = 100f;
        public const float HeatDecayRate = 66.67f;
        public const float OverheatLockout = 1.2f;

        // Dash
        public const int DashMaxCharges = 2;
        public const int DashEnergyCost = 25;
        public const float DashRechargeTime = 1.5f;
        public const float DashBurstSpeed = 12f;

        // Stabilize
        public const float StabilizeHealPercent = 0.6f;
        public const float StabilizeChannelTime = 1.5f;
        public const int StabilizeEnergyCost = 50;

        // Round
        public const float RoundDuration = 75f;
        public const float OvertimeDelay = 3f;
        public const float CoreCaptureTime = 2f;

        // Weapons – Shared
        public const float ProjectileSpeed = 15f;
        public const float PierceSpeed = 30f;

        // Weapons – Vanguard
        public const int VanguardDamage = 16;
        public const float VanguardFireRate = 0.25f;
        public const float VanguardHeatPerShot = 11.11f;
        public const float VanguardKnockbackMult = 1.5f;

        // Weapons – Striker
        public const int StrikerBurstDamage = 12;
        public const float StrikerBurstDelay = 0.08f;
        public const float StrikerBurstInterval = 0.4f;
        public const float StrikerHeatPerBurst = 14.29f;
        public const int StrikerShotsPerBurst = 3;

        // Weapons – Disruptor
        public const int DisruptorDamage = 18;
        public const float DisruptorFireRate = 0.35f;
        public const float DisruptorHeatPerShot = 12.5f;

        // Weapons – Flanker
        public const int FlankerPelletDamage = 8;
        public const int FlankerPellets = 5;
        public const float FlankerSpreadAngle = 30f;
        public const float FlankerFireRate = 0.3f;
        public const float FlankerHeatPerShot = 14.29f;

        // Abilities – Vanguard
        public const float VanguardBarrierDuration = 4f;
        public const float VanguardBarrierCooldown = 6f;
        public const int VanguardBarrierEnergyCostPerSec = 25;
        public const int VanguardBarrierRearModuleHP = 70;
        public const float VanguardShieldBashCooldown = 8f;
        public const int VanguardShieldBashEnergyCost = 35;
        public const int VanguardShieldBashDamage = 20;

        // Abilities – Striker
        public const float StrikerOverdriveDuration = 4f;
        public const float StrikerOverdriveCooldown = 10f;
        public const int StrikerOverdriveEnergyCost = 40;
        public const float StrikerOverdriveFireRateBonus = 0.25f;
        public const float StrikerOverdriveHeatPenalty = 0.2f;
        public const float StrikerRicochetShotCooldown = 12f;
        public const int StrikerRicochetShotEnergyCost = 45;
        public const float StrikerRicochetShotDamagePercent = 0.35f;
        public const int StrikerRicochetShotBounces = 1;

        // Abilities – Disruptor
        public const float DisruptorPierceCannonChargeTime = 2.5f;
        public const float DisruptorPierceCannonMoveSpeedMult = 0.3f;
        public const int DisruptorPierceCannonEnergyCost = 60;
        public const float DisruptorPierceCannonCooldown = 14f;
        public const float DisruptorPierceCannonStandardDmgPercent = 0.6f;
        public const float DisruptorPierceCannonVanguardDmgPercent = 0.3f;
        public const float DisruptorPierceCannonStunDuration = 0.4f;
        public const float DisruptorEMPMineArmTime = 1f;
        public const float DisruptorEMPMineBarrierDisableDuration = 1.5f;
        public const int DisruptorEMPMineEnergyCost = 45;
        public const float DisruptorEMPMineCooldown = 14f;

        // Abilities – Flanker
        public const int FlankerBlinkEnergyCost = 35;
        public const int FlankerBlinkCharges = 2;
        public const float FlankerBlinkRechargeTime = 8f;
        public const float FlankerCloakDuration = 1.5f;
        public const float FlankerCloakCooldown = 12f;
        public const int FlankerCloakEnergyCost = 50;

        // Composition
        public const int MaxVanguards = 2;
        public const int MaxDisruptors = 2;

        // Layers
        public const int ProjectileLayer = 8;

        // Physics
        public const float MicroKnockback = 2f;
        public const float ShieldBashForce = 15f;
        public const float CollisionPushFalloff = 0.8f;
    }
}
