using UnityEngine;

namespace SpaceFighter
{
    public static class DamageSystem
    {
        public static void ApplyDamage(PlayerShip target, int baseDamage, bool ignoresBarrier,
            bool ignoresDR, Vector2 fromDirection,
            float knockbackForce = GameConstants.MicroKnockback)
        {
            if (target == null || !target.IsAlive) return;
            target.TakeDamage(baseDamage, ignoresBarrier, ignoresDR, fromDirection, knockbackForce);
        }

        public static void ApplyRearModuleDamage(VanguardBarrier barrier, int damage)
        {
            if (barrier == null) return;
            barrier.DamageRearModule(damage);
        }

        public static bool IsHitFromBehind(Transform target, Vector2 hitDirection)
        {
            Vector2 forward = target.up;
            float dot = Vector2.Dot(forward.normalized, hitDirection.normalized);
            return dot > 0.3f;
        }
    }
}
