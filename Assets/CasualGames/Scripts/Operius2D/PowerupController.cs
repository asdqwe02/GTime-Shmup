using CasualGames.Operius2D.Configs;
using CasualGames.Operius2D.Signals;
using UnityEngine;
using Zenject;

namespace CasualGames.Operius2D
{
    public class PowerupController : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;
        [Inject] private GameConfig _gameConfig;
        [Inject] private PowerupController.Pool _pool;

        private PowerUpType _powerUpType;
        private bool _destroyed;

        private void Awake()
        {
            _powerUpType = PowerUpType.SHIELD;
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.CompareTag("Player"))
            {
                _signalBus.Fire<PowerUpSignal>(new PowerUpSignal
                {
                    PowerUpType = _powerUpType
                });
                if (!_destroyed)
                {
                    _pool.Despawn(this);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Bound"))
            {
                if (!_destroyed)
                {
                    _pool.Despawn(this);
                }
            }
        }

        private void FixedUpdate()
        {
            transform.position += new Vector3(0, -1 * _gameConfig.PowerUpMoveSpeed, 0f) * Time.fixedDeltaTime;
        }

        public class Pool : MonoMemoryPool<PowerUpType, PowerupController>
        {
            protected override void Reinitialize(PowerUpType powerUpType, PowerupController item)
            {
                base.Reinitialize(powerUpType, item);
                item._powerUpType = powerUpType;
            }

            protected override void OnSpawned(PowerupController item)
            {
                item._destroyed = false;
                base.OnSpawned(item);
            }

            protected override void OnDespawned(PowerupController item)
            {
                item._destroyed = true;
                base.OnDespawned(item);
            }
        }
    }

    public enum PowerUpType
    {
        SHIELD,
    }
}