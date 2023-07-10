using System;
using System.Collections;
using CasualGames.Operius2D.Configs;
using CasualGames.Operius2D.Signals;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CasualGames.Operius2D
{
    [Serializable]
    public class BossEnemyController : Enemy
    {
        private BossStat _bossStat;
        private Collider2D _collider2D;
        private int _hp; // current HP

        private void Awake()
        {
            _collider2D = GetComponent<Collider2D>();
        }

        private void OnGameResetSignal(GameResetSignal obj)
        {
            Destroy(gameObject);
        }

        private void Start()
        {
            // _signalBus.Subscribe<BossFireBullet>(OnBossFireBullet);
            _signalBus.Subscribe<GameResetSignal>(OnGameResetSignal);
            StartCoroutine(FireBullet());
        }

        private void OnDestroy()
        {
            // _signalBus.Unsubscribe<BossFireBullet>(OnBossFireBullet);
            _signalBus.Unsubscribe<GameResetSignal>(OnGameResetSignal);
        }


        // useless
        private void OnBossFireBullet(BossFireBullet obj)
        {
            var value = _collider2D.bounds.size.x / 2;
            var bulletStartPos = transform.position + new Vector3(Random.Range(-value, value), 0, 0);
            _bulletPool.Spawn(new BulletData
            {
                FirePosition = bulletStartPos,
                FromPlayer = false,
                Speed = _gameConfig.EnemyBulletSpeed,
                Direction = Vector2.down
            });
        }

        private void FixedUpdate()
        {
            transform.position += new Vector3(
                _bossStat.Speed * _horizontalDirection * Time.fixedDeltaTime,
                0f,
                0f
            );
        }

        public override void Hit()
        {
            _hp--;
            if (_hp <= 0 && !Destroyed)
            {
                Die();
                // _signalBus.Fire<EnemyDestroySignal>(new EnemyDestroySignal
                // {
                //     IsBoss = true
                // });
                // Destroy(gameObject);
            }
        }

        public void Destroy()
        {
            _signalBus.Fire<EnemyDestroySignal>(new EnemyDestroySignal
            {
                IsBoss = true
            });
            Destroy(gameObject);
        }

        public override void Die()
        {
            _destroyed = true;
            _collider2D.enabled = false;
            GetComponent<SpriteRenderer>().enabled = false; // temporary change this later
            _explodeParticle.Play();
        }

        IEnumerator FireBullet()
        {
            while (true)
            {
                yield return new WaitForSeconds(_bossStat.FireInterval);
                var value = _collider2D.bounds.size.x / 2;
                var bulletStartPos = transform.position + new Vector3(Random.Range(-value, value), 0, 0);
                _bulletPool.Spawn(new BulletData
                {
                    FirePosition = bulletStartPos,
                    FromPlayer = false,
                    Speed = _gameConfig.EnemyBulletSpeed,
                    Direction = Vector2.down,
                    Color = Color.red
                });
                // bulletStartPos, false, _gameConfig.EnemyBulletSpeed);
            }
        }

        private void OnTriggerExit2D(Collider2D col)
        {
            if (col.CompareTag("Bound"))
            {
                _horizontalDirection = -_horizontalDirection;
            }
        }

        public void SetUpStat(BossStat bossStat)
        {
            _bossStat = bossStat;
            _hp = _bossStat.HP;
        }
    }
}