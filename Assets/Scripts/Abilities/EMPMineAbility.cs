using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace SpaceFighter
{
    public class EMPMineAbility : NetworkBehaviour
    {
        [SyncVar] public float Cooldown;

        [SerializeField] private GameObject _empMinePrefab;

        private PlayerShip _ship;
        private EnergySystem _energy;
        private AbilityController _abilityController;

        private void Awake()
        {
            _ship = GetComponent<PlayerShip>();
            _energy = GetComponent<EnergySystem>();
            _abilityController = GetComponent<AbilityController>();
        }

        private void Update()
        {
            if (!IsServerInitialized) return;

            if (Cooldown > 0f)
                Cooldown -= Time.deltaTime;
        }

        [Server]
        public void Execute()
        {
            if (Cooldown > 0f) return;
            if (_ship == null || !_ship.IsAlive) return;
            if (_energy == null || !_energy.UseEnergy(GameConstants.DisruptorEMPMineEnergyCost)) return;

            Cooldown = GameConstants.DisruptorEMPMineCooldown;
            if (_abilityController != null)
                _abilityController.Ability2Cooldown = GameConstants.DisruptorEMPMineCooldown;

            if (_empMinePrefab == null) return;

            GameObject go = Instantiate(_empMinePrefab, transform.position, Quaternion.identity);
            EMPMine mine = go.GetComponent<EMPMine>();
            if (mine != null)
                mine.MineTeam = _ship.ShipTeam;
            ServerManager.Spawn(go);
        }
    }
}
