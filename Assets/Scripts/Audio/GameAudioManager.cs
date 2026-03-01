using UnityEngine;

namespace SpaceFighter
{
    public class GameAudioManager : MonoBehaviour
    {
        public static GameAudioManager Instance { get; private set; }

        private AudioSource _sfxSource;

        private AudioClip _shoot;
        private AudioClip _hit;
        private AudioClip _dash;
        private AudioClip _pierceCharge;
        private AudioClip _pierceFire;
        private AudioClip _overheat;
        private AudioClip _barrierOn;
        private AudioClip _barrierOff;
        private AudioClip _overtimeStart;
        private AudioClip _coreActive;
        private AudioClip _blink;
        private AudioClip _lowEnergy;
        private AudioClip _bash;
        private AudioClip _cloak;

        public float Volume
        {
            get => _sfxSource != null ? _sfxSource.volume : 1f;
            set { if (_sfxSource != null) _sfxSource.volume = Mathf.Clamp01(value); }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;

            CacheClips();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void CacheClips()
        {
            _shoot = AudioSynthesizer.ShootSound;
            _hit = AudioSynthesizer.HitSound;
            _dash = AudioSynthesizer.DashSound;
            _pierceCharge = AudioSynthesizer.PierceChargeLoop;
            _pierceFire = AudioSynthesizer.PierceFireSound;
            _overheat = AudioSynthesizer.OverheatSound;
            _barrierOn = AudioSynthesizer.BarrierOnSound;
            _barrierOff = AudioSynthesizer.BarrierOffSound;
            _overtimeStart = AudioSynthesizer.OvertimeStartSound;
            _coreActive = AudioSynthesizer.CoreActiveSound;
            _blink = AudioSynthesizer.BlinkSound;
            _lowEnergy = AudioSynthesizer.LowEnergyWarning;
            _bash = AudioSynthesizer.BashSound;
            _cloak = AudioSynthesizer.CloakSound;
        }

        public void PlayShoot() => _sfxSource.PlayOneShot(_shoot);
        public void PlayHit() => _sfxSource.PlayOneShot(_hit);
        public void PlayDash() => _sfxSource.PlayOneShot(_dash);
        public void PlayPierceCharge() => _sfxSource.PlayOneShot(_pierceCharge);
        public void PlayPierceFire() => _sfxSource.PlayOneShot(_pierceFire);
        public void PlayOverheat() => _sfxSource.PlayOneShot(_overheat);
        public void PlayBarrierOn() => _sfxSource.PlayOneShot(_barrierOn);
        public void PlayBarrierOff() => _sfxSource.PlayOneShot(_barrierOff);
        public void PlayOvertimeStart() => _sfxSource.PlayOneShot(_overtimeStart);
        public void PlayCoreActive() => _sfxSource.PlayOneShot(_coreActive);
        public void PlayBlink() => _sfxSource.PlayOneShot(_blink);
        public void PlayLowEnergy() => _sfxSource.PlayOneShot(_lowEnergy);
        public void PlayBash() => _sfxSource.PlayOneShot(_bash);
        public void PlayCloak() => _sfxSource.PlayOneShot(_cloak);
    }
}
