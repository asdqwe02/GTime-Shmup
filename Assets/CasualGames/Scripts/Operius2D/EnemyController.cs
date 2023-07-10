using System.Collections;
using CasualGames.Operius2D.Configs;
using CasualGames.Operius2D.Models;
using CasualGames.Operius2D.Signals;
using DG.Tweening;
using SRF;
using UnityEngine;
using Zenject;

namespace CasualGames.Operius2D
{
    public class EnemyController : Enemy
    {
        [Inject] private EnemyConfig _enemyConfig;
        [Inject] private EnemyController.Pool _enemyPool;
        [Inject] private PoolCache _poolCache;

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Animator _animator;
        [SerializeField] private AudioSource _dieSFX;
        private int _vertDirection;
        private EnemyType _enemyType;
        public EnemyType EnemyType => _enemyType;
        public SpriteRenderer SpriteRenderer => _spriteRenderer;
        public bool Lasering;

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            _formationPosition += new Vector3(
                _enemyConfig.SideWaySpeed * _horizontalDirection * Time.fixedDeltaTime,
                0f,
                0f
            );
            if (!Diving)
            {
                transform.position = _formationPosition;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Bound"))
            {
                if (transform.position.y < other.transform.position.y - other.bounds.size.y / 2)
                {
                    // _logger.Information("destroy enemy out of bound");
                    if (gameObject.activeSelf)
                        StartCoroutine(DespawnOutOfBound());
                    // Dispose();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.CompareTag("Player"))
            {
                _explodeParticle.Play();
                Dispose();
                _signalBus.Fire<PlayerHitSignal>(new PlayerHitSignal
                {
                    Damage = _enemyConfig.Damage,
                    // Source = this
                });
            }
        }

        IEnumerator DespawnOutOfBound()
        {
            // _logger.Information("enemy out of bound coroutine start");
            yield return new WaitForSeconds(_enemyConfig.OutOfBoundDespawnDelay);
            Dispose(true);
        }

        public void Dispose(bool outOfBound = false, float delay = 0f)
        {
            if (!Destroyed)
            {
                _destroyed = true;
                StartCoroutine(Despawn(delay));
                // _enemyPool.Despawn(this);
                _signalBus.Fire<EnemyDestroySignal>(new EnemyDestroySignal
                {
                    OutOfBound = outOfBound
                });
            }
        }

        public override void Hit()
        {
            _enemyType.OnDeathEffect(this);
            _dieSFX.Play();
            _collider.enabled = false;
            _animator.gameObject.SetActive(false);
            _explodeParticle.Play();
            Dispose(false, _explodeParticle.main.duration);
            // Dispose();
        }

        IEnumerator Despawn(float delay = 0f)
        {
            yield return new WaitForSeconds(delay);
            _enemyPool.Despawn(this);
            _poolCache.ActivePoolEnemyRefs.Remove(this);
        }

        public override void Attack()
        {
            if (!Diving && !_destroyed)
            {
                _enemyType.Attack(this);
            }
        }

        public override void MeleeAttack(Vector3 target)
        {
            if (!Diving)
            {
                Diving = true;
                ((IMeleeAttack)_enemyType).MeleeAttack(this, target);
            }
        }

        void SetAnimation()
        {
            switch (_enemyType)
            {
                default:
                    _animator.SetInteger("Type", 0);
                    break;
                case SpreadShotType:
                    _animator.SetInteger("Type", 1);
                    break;
                case LaserBeamerType:
                    _animator.SetInteger("Type", 2);
                    break;
                case SpikeExplodeType:
                    _animator.SetInteger("Type", 3);

                    break;
            }
        }

        public class Pool : MonoMemoryPool<Vector3, EnemyType, EnemyController>
        {
            protected override void Reinitialize(Vector3 formationPos, EnemyType enemyType, EnemyController item)
            {
                // TODO: REALLY NEED TO REFACTOR THIS
                base.Reinitialize(formationPos, enemyType, item);
                var color = item._enemyConfig.Colors.Random();
                item._spriteRenderer.color = color;
                var explodeParticle = item._explodeParticle.main;
                explodeParticle.startColor = new ParticleSystem.MinMaxGradient(color);
                item._collider.enabled = true;
                item._animator.gameObject.SetActive(true);
                item._formationPosition = formationPos;
                item.transform.position = formationPos;
                item._enemyType = enemyType;
                item.SetAnimation();
            }

            protected override void OnSpawned(EnemyController item)
            {
                base.OnSpawned(item);
                // item.enabled = true;
                item._horizontalDirection = 1;
                item.Diving = false;
                item._destroyed = false;
                if (!item._poolCache.ActivePoolEnemyRefs.Contains(item))
                    item._poolCache.ActivePoolEnemyRefs.Add(item);
            }

            protected override void OnDespawned(EnemyController item)
            {
                base.OnDespawned(item);
                item.Lasering = false;
                item.transform.DOKill();
                // item.gameObject.SetActive(false);
            }

            protected override void OnDestroyed(EnemyController item)
            {
                base.OnDestroyed(item);
                item.transform.DOKill();
            }
        }
    }
}