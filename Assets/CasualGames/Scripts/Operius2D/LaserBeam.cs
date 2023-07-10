using System.Collections;
using CasualGames.Operius2D.Configs;
using CasualGames.Operius2D.Signals;
using UnityEngine;
using UnityEngine.Events;
using Zenject;
using ILogger = PKFramework.Logger.ILogger;

namespace CasualGames.Operius2D
{
    public class LaserBeam : MonoBehaviour
    {
        [Inject] private ILogger _logger;
        [Inject] private EnemyConfig _enemyConfig;
        [Inject] private SignalBus _signalBus;
        [Inject] private LaserBeam.Pool _pool;
        [SerializeField] private SpriteRenderer _beamChargeSpriteRenderer;
        [SerializeField] private SpriteRenderer _beamSpriteRenderer;
        [SerializeField] private Animator _beamChargeAnimator;
        public UnityAction OnLaserComplete;
        private bool _lasering;
        private Enemy _fireSource;
        private bool _destroyed;

        public IEnumerator FireLaser(float chargeDuration, float laserDuration)
        {
            _lasering = true;
            yield return ChargeLaser(chargeDuration);
            yield return FireLaserBeam(laserDuration);
        }

        IEnumerator ChargeLaser(float chargeDuration)
        {
            _beamChargeSpriteRenderer.gameObject.SetActive(true);
            yield return new WaitForSeconds(chargeDuration - 0.5f);
            _beamChargeAnimator.SetTrigger("NextCharge");
            yield return new WaitForSeconds(0.5f);
            _beamChargeSpriteRenderer.gameObject.SetActive(false);
        }

        private void FixedUpdate()
        {
            if (_fireSource != null && !_fireSource.Destroyed)
            {
                transform.position = _fireSource.transform.position + (Vector3)_enemyConfig.LaserOffset;
            }
            // else
            // {
            //     _pool.Despawn(this);
            // }
        }

        IEnumerator FireLaserBeam(float laserDuration)
        {
            float elapsed = 0;
            _beamSpriteRenderer.gameObject.SetActive(true);
            _beamChargeSpriteRenderer.gameObject.SetActive(false);
            while (elapsed <= laserDuration)
            {
                var boxCastPos = transform.position + new Vector3(0f, -_enemyConfig.LaserSize.y / 2f, 0f);
                RaycastHit2D hit = Physics2D.BoxCast(
                    boxCastPos,
                    _enemyConfig.LaserSize,
                    0f,
                    Vector2.down,
                    0.1f,
                    _enemyConfig.EnemyHitLayerMask
                );
                if (hit)
                {
                    _logger.Debug("hit player enemy laser beam");
                    _signalBus.Fire<PlayerHitSignal>(new PlayerHitSignal
                    {
                        Damage = 1,
                    });
                }

                yield return new WaitForFixedUpdate();
                elapsed += Time.fixedDeltaTime;
            }

            if (!_destroyed)
            {
                _pool.Despawn(this);
            }
        }

        public void SourceDie()
        {
            if (!_destroyed)
            {
                _fireSource = null;
                _pool.Despawn(this);
            }
        }

        private void OnDrawGizmos()
        {
            if (_lasering)
            {
                Gizmos.DrawWireCube(transform.position + new Vector3(0f, -_enemyConfig.LaserSize.y / 2f, 0f), _enemyConfig.LaserSize);
            }
        }

        private void OnEnable()
        {
            StartCoroutine(FireLaser(_enemyConfig.LaserChargeTime, _enemyConfig.LaserFireTime));
        }

        public class Pool : MonoMemoryPool<Enemy, Color32, LaserBeam>
        {
            protected override void Reinitialize(Enemy source, Color32 color, LaserBeam item)
            {
                base.Reinitialize(source, color, item);
                item._fireSource = source;
                item.transform.position = source.transform.position + (Vector3)item._enemyConfig.LaserOffset;
                item._beamSpriteRenderer.color = new Color32(color.r, color.g, color.b, 150);
                item._destroyed = false;
            }

            protected override void OnDespawned(LaserBeam item)
            {
                item.StopAllCoroutines();
                item._beamSpriteRenderer.gameObject.SetActive(false);
                item._beamChargeSpriteRenderer.gameObject.SetActive(false);
                item.OnLaserComplete?.Invoke();
                item.OnLaserComplete = null;
                // if (item._fireSource != null)
                // {
                //     item._fireSource.GetComponent<EnemyController>().Lasering = false;
                // }

                item._fireSource = null;
                item._lasering = false;
                item._destroyed = true;
                base.OnDespawned(item);
            }
        }
    }
}