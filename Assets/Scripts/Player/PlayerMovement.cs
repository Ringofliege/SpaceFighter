using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace SpaceFighter
{
    public class PlayerMovement : NetworkBehaviour
    {
        [SyncVar]
        private Vector2 _syncPosition;

        [SyncVar]
        private float _syncRotation;

        private Rigidbody2D _rb;
        private PlayerShip _ship;
        private Vector2 _inputDirection;
        private Vector2 _aimDirection = Vector2.up;
        private Vector2 _velocity;

        private bool _isStunned;
        private float _stunTimer;

        public bool IsStunned
        {
            get { return _isStunned; }
        }

        private float _speedMultiplier = 1f;
        public float SpeedMultiplier
        {
            get { return _speedMultiplier; }
            set { _speedMultiplier = value; }
        }

        private const float DragFactor = 0.95f;
        private const float InterpolationSpeed = 15f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _ship = GetComponent<PlayerShip>();
        }

        private void Update()
        {
            if (!IsOwner) return;

            Vector2 input = Vector2.zero;
            if (Input.GetKey(KeyCode.W)) input.y += 1f;
            if (Input.GetKey(KeyCode.S)) input.y -= 1f;
            if (Input.GetKey(KeyCode.A)) input.x -= 1f;
            if (Input.GetKey(KeyCode.D)) input.x += 1f;

            if (input.sqrMagnitude > 1f)
                input.Normalize();

            CmdMove(input);

            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 aimDir = ((Vector2)mouseWorld - (Vector2)transform.position).normalized;
            if (aimDir.sqrMagnitude > 0.01f)
                CmdAim(aimDir);
        }

        [ServerRpc]
        private void CmdMove(Vector2 input)
        {
            _inputDirection = input;
        }

        [ServerRpc]
        private void CmdAim(Vector2 aimDir)
        {
            _aimDirection = aimDir.normalized;
        }

        private void FixedUpdate()
        {
            if (!IsServerInitialized) return;

            if (_isStunned)
            {
                _stunTimer -= Time.fixedDeltaTime;
                if (_stunTimer <= 0f)
                    _isStunned = false;
                _inputDirection = Vector2.zero;
            }

            float classMult = GetClassSpeedMultiplier();
            float maxSpeed = GameConstants.BaseSpeed * classMult * _speedMultiplier;
            float accel = GameConstants.BaseAcceleration * _speedMultiplier;

            if (_inputDirection.sqrMagnitude > 0.01f)
            {
                _velocity += _inputDirection * accel * Time.fixedDeltaTime;
            }
            else
            {
                _velocity *= DragFactor;
            }

            if (_velocity.magnitude > maxSpeed)
                _velocity = _velocity.normalized * maxSpeed;

            _rb.MovePosition(_rb.position + _velocity * Time.fixedDeltaTime);

            // Rotate to face aim direction
            if (_aimDirection.sqrMagnitude > 0.01f)
            {
                float angle = Mathf.Atan2(_aimDirection.y, _aimDirection.x) * Mathf.Rad2Deg - 90f;
                _rb.MoveRotation(angle);
            }

            _syncPosition = _rb.position;
            _syncRotation = _rb.rotation;
        }

        private void LateUpdate()
        {
            if (IsOwner || IsServerInitialized) return;

            // Interpolate for non-owner clients
            transform.position = Vector2.Lerp(transform.position, _syncPosition,
                Time.deltaTime * InterpolationSpeed);
            float currentAngle = transform.eulerAngles.z;
            float targetAngle = _syncRotation;
            float smoothAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * InterpolationSpeed);
            transform.rotation = Quaternion.Euler(0f, 0f, smoothAngle);
        }

        private float GetClassSpeedMultiplier()
        {
            if (_ship == null) return 1f;

            switch (_ship.ShipClassType)
            {
                case ShipClass.Vanguard:
                    return _ship.HasBarrierActive
                        ? GameConstants.VanguardBarrierSpeedMult
                        : GameConstants.VanguardSpeedMult;
                default:
                    return 1f;
            }
        }

        public float GetMass()
        {
            if (_ship != null && _ship.ShipClassType == ShipClass.Vanguard)
                return 2f;
            return 1f;
        }

        public void ApplyForce(Vector2 force)
        {
            _velocity += force;
        }

        [Server]
        public void ApplyStun(float duration)
        {
            _isStunned = true;
            _stunTimer = duration;
            _inputDirection = Vector2.zero;
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (!IsServerInitialized) return;

            if (collision.contactCount == 0) return;
            ContactPoint2D contact = collision.GetContact(0);

            // Wall sliding: project velocity along wall surface
            Vector2 wallNormal = contact.normal;
            float dot = Vector2.Dot(_velocity, wallNormal);
            if (dot < 0f)
            {
                _velocity -= wallNormal * dot;
            }

            // Speed loss on strong impact
            if (collision.relativeVelocity.magnitude > GameConstants.BaseSpeed * 0.5f)
            {
                _velocity *= 0.8f;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!IsServerInitialized) return;

            PlayerMovement otherMovement = collision.gameObject.GetComponent<PlayerMovement>();
            if (otherMovement == null) return;

            float myMass = GetMass();
            float otherMass = otherMovement.GetMass();
            float massRatio = otherMass / (myMass + otherMass);

            if (collision.contactCount == 0) return;
            Vector2 pushDir = collision.GetContact(0).normal;
            float pushForce = collision.relativeVelocity.magnitude * massRatio * GameConstants.CollisionPushFalloff;

            _velocity += pushDir * pushForce;
        }
    }
}
