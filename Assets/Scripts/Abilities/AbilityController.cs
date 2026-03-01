using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace SpaceFighter
{
    public class AbilityController : NetworkBehaviour
    {
        [SyncVar] public float Ability1Cooldown;
        [SyncVar] public float Ability2Cooldown;

        private PlayerShip _ship;

        // Vanguard
        private VanguardBarrier _barrier;
        private ShieldBash _shieldBash;

        // Striker
        private StrikerOverdrive _overdrive;
        private RicochetAbility _ricochet;

        // Disruptor
        private PierceCannonAbility _pierceCannon;
        private EMPMineAbility _empMine;

        // Flanker
        private FlankerBlink _blink;
        private CloakAbility _cloak;

        private void Awake()
        {
            _ship = GetComponent<PlayerShip>();
            _barrier = GetComponent<VanguardBarrier>();
            _shieldBash = GetComponent<ShieldBash>();
            _overdrive = GetComponent<StrikerOverdrive>();
            _ricochet = GetComponent<RicochetAbility>();
            _pierceCannon = GetComponent<PierceCannonAbility>();
            _empMine = GetComponent<EMPMineAbility>();
            _blink = GetComponent<FlankerBlink>();
            _cloak = GetComponent<CloakAbility>();
        }

        private void Update()
        {
            if (IsServerInitialized)
            {
                if (Ability1Cooldown > 0f)
                    Ability1Cooldown -= Time.deltaTime;
                if (Ability2Cooldown > 0f)
                    Ability2Cooldown -= Time.deltaTime;
            }

            if (!IsOwner) return;
            if (_ship == null || !_ship.IsAlive) return;

            if (Input.GetMouseButtonDown(1))
                CmdUseAbility1();

            if (Input.GetKeyDown(KeyCode.Q))
                CmdUseAbility2();
        }

        [ServerRpc]
        private void CmdUseAbility1()
        {
            if (_ship == null || !_ship.IsAlive) return;

            switch (_ship.ShipClassType)
            {
                case ShipClass.Vanguard:
                    if (_barrier != null)
                    {
                        if (_barrier.IsBarrierActive)
                            _barrier.DeactivateBarrier();
                        else if (Ability1Cooldown <= 0f)
                            _barrier.ActivateBarrier();
                    }
                    break;
                case ShipClass.Striker:
                    if (_overdrive != null && Ability1Cooldown <= 0f)
                        _overdrive.Execute();
                    break;
                case ShipClass.Disruptor:
                    if (_pierceCannon != null && Ability1Cooldown <= 0f)
                        _pierceCannon.StartCharge();
                    break;
                case ShipClass.Flanker:
                    if (_blink != null)
                    {
                        Vector2 dir = (Vector2)transform.up;
                        _blink.Execute(dir);
                    }
                    break;
            }
        }

        [ServerRpc]
        private void CmdUseAbility2()
        {
            if (_ship == null || !_ship.IsAlive) return;

            switch (_ship.ShipClassType)
            {
                case ShipClass.Vanguard:
                    if (_shieldBash != null && Ability2Cooldown <= 0f)
                        _shieldBash.Execute();
                    break;
                case ShipClass.Striker:
                    if (_ricochet != null && Ability2Cooldown <= 0f)
                    {
                        Vector2 aimDir = (Vector2)transform.up;
                        _ricochet.Execute(aimDir);
                    }
                    break;
                case ShipClass.Disruptor:
                    if (_empMine != null && Ability2Cooldown <= 0f)
                        _empMine.Execute();
                    break;
                case ShipClass.Flanker:
                    if (_cloak != null && Ability2Cooldown <= 0f)
                        _cloak.Execute();
                    break;
            }
        }

        public bool IsAbility1Active
        {
            get
            {
                if (_ship == null) return false;
                switch (_ship.ShipClassType)
                {
                    case ShipClass.Vanguard: return _barrier != null && _barrier.IsBarrierActive;
                    case ShipClass.Striker: return _overdrive != null && _overdrive.IsOverdriveActive;
                    case ShipClass.Disruptor: return _pierceCannon != null && _pierceCannon.IsCharging;
                    default: return false;
                }
            }
        }

        public bool IsAbility2Active
        {
            get
            {
                if (_ship == null) return false;
                switch (_ship.ShipClassType)
                {
                    case ShipClass.Flanker: return _cloak != null && _cloak.IsCloaked;
                    default: return false;
                }
            }
        }
    }
}
