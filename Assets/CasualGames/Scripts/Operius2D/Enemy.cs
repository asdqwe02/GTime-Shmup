using System;
using System.Collections;
using CasualGames.Operius2D.Configs;
using CasualGames.Scenes.Operius2DScene;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace CasualGames.Operius2D
{
    public class Enemy : MonoBehaviour
    {
        [Inject] protected GameConfig _gameConfig;
        [Inject] protected SignalBus _signalBus;
        [Inject] protected BulletController.Pool _bulletPool;
        [Inject] protected Operius2DSceneLogic _logic;
        [SerializeField] protected ParticleSystem _explodeParticle;
        [SerializeField] protected Vector3 _formationPosition;

        [HideInInspector]
        public bool Diving;

        protected int _horizontalDirection = 1;
        protected Collider2D _collider;
        protected bool _destroyed;
        public bool Destroyed => _destroyed;
        protected UnityAction OnEnemyDestroy;

        private void Awake()
        {
            var mainExplodeParticle = _explodeParticle.main;
            _collider = GetComponent<Collider2D>();
            mainExplodeParticle.duration = mainExplodeParticle.startLifetime.constantMax;
            mainExplodeParticle.stopAction = ParticleSystemStopAction.Callback;
        }

        protected virtual void FixedUpdate()
        {
            if (_formationPosition.x <= _logic.HorizontalBound.x)
            {
                _horizontalDirection = 1;
            }
            else if (_formationPosition.x >= _logic.HorizontalBound.y)
            {
                _horizontalDirection = -1;
            }
        }

        public virtual void Attack()
        {
        }

        public virtual void Hit()
        {
        }

        public virtual void MeleeAttack(Vector3 target)
        {
            // if (!_diving)
            // {
            //     _diving = true;
            //     transform.DOMove(target, 3f).SetEase(Ease.Linear).OnComplete(() => { StartCoroutine(GetBackToFormation()); });
            // }
        }

        private void OnDestroy()
        {
            transform.DOKill();
        }

        public IEnumerator GetBackToFormation(float duration = 1f)
        {
            float elapsedTime = 0f;
            while (elapsedTime <= duration)
            {
                transform.position = Vector3.Lerp(transform.position, _formationPosition, elapsedTime / duration);
                elapsedTime += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            transform.position = _formationPosition;
            Diving = false;
        }

        public virtual void Die()
        {
        }

        public enum EnemyTypeEnum
        {
            UNDEFINED,
            BASIC,
            SPREAD_SHOT,
            SPIKE_EXPLODE,
            LASER_SHOT,
        }
    }

    public interface IMeleeAttack
    {
        public void MeleeAttack(EnemyController enemyController, Vector3 target);
    }

    [Serializable]
    public abstract class EnemyType
    {
        [Inject] protected EnemyConfig _enemyConfig;
        [Inject] protected BulletController.Pool _bulletPool;
        [Inject] protected Operius2DSceneLogic _logic;
        [Inject] protected GameConfig _gameConfig;
        [Inject] protected LaserBeam.Pool _laserBeamPool;
        public abstract void Attack(EnemyController enemyController);
        public abstract void OnDeathEffect(EnemyController enemyController);
    }

    [Serializable]
    public class SpreadShotType : EnemyType
    {
        public override void Attack(EnemyController enemyController)
        {
            float startAngle = -_enemyConfig.SpreadConeAngle / 2;
            float stepAngle = _enemyConfig.SpreadConeAngle / (_enemyConfig.SpreadBullet - 1);

            for (int i = 0; i < _enemyConfig.SpreadBullet; i++)
            {
                var bulletDir = _logic.RotateVector2(Vector2.down, startAngle + stepAngle * i);
                _bulletPool.Spawn(new BulletData
                {
                    FirePosition = enemyController.transform.position,
                    FromPlayer = false,
                    Speed = _gameConfig.EnemyBulletSpeed,
                    Direction = bulletDir,
                    Color = enemyController.SpriteRenderer.color,
                    BulletSprite = _gameConfig.EnemyBulletSprite
                });
            }
        }

        public override void OnDeathEffect(EnemyController enemyController)
        {
        }

        public class Factory : PlaceholderFactory<EnemyType>
        {
        }
    }

    [Serializable]
    public class BasicType : EnemyType, IMeleeAttack
    {
        public override void Attack(EnemyController enemyController)
        {
            _bulletPool.Spawn(new BulletData
            {
                FirePosition = enemyController.transform.position,
                FromPlayer = false,
                Speed = _gameConfig.EnemyBulletSpeed,
                Direction = Vector2.down,
                Color = enemyController.SpriteRenderer.color,
                BulletSprite = _gameConfig.EnemyBulletSprite
            });
        }


        public override void OnDeathEffect(EnemyController enemyController)
        {
        }

        public class Factory : PlaceholderFactory<EnemyType>
        {
        }

        public void MeleeAttack(EnemyController enemyController, Vector3 target)
        {
            enemyController.transform.DOMove(target, 3f).SetEase(Ease.Linear).OnComplete(() => { enemyController.StartCoroutine(enemyController.GetBackToFormation()); });
        }
    }

    [Serializable]
    public class SpikeExplodeType : EnemyType, IMeleeAttack
    {
        public override void Attack(EnemyController enemyController)
        {
        }


        public override void OnDeathEffect(EnemyController enemyController)
        {
            float startAngle = 0;
            float stepAngle = 360f / _enemyConfig.NumberOfSpike;
            for (int i = 0; i < _enemyConfig.NumberOfSpike; i++)
            {
                var bulletDir = _logic.RotateVector2(Vector2.down, startAngle + stepAngle * i);
                _bulletPool.Spawn(new BulletData
                    {
                        FirePosition = enemyController.transform.position,
                        FromPlayer = false,
                        Speed = _gameConfig.EnemyBulletSpeed,
                        Direction = bulletDir,
                        Color = enemyController.SpriteRenderer.color,
                        BulletSprite = _gameConfig.EnemyBulletSprite
                    }
                );
            }
        }

        public void MeleeAttack(EnemyController enemyController, Vector3 target)
        {
            enemyController.transform.DOMove(target, 3f).SetEase(Ease.Linear).OnComplete(() => { enemyController.StartCoroutine(enemyController.GetBackToFormation()); });
        }

        public class Factory : PlaceholderFactory<EnemyType>
        {
        }
    }

    [Serializable]
    public class LaserBeamerType : EnemyType
    {
        private LaserBeam _laserBeam;

        public override void Attack(EnemyController enemyController)
        {
            if (!enemyController.Lasering)
            {
                enemyController.Lasering = true;
                _laserBeam = _laserBeamPool.Spawn(enemyController, enemyController.SpriteRenderer.color);
                _laserBeam.OnLaserComplete = () =>
                {
                    if (enemyController != null)
                    {
                        enemyController.Lasering = false;
                    }
                };
            }
        }


        public override void OnDeathEffect(EnemyController enemyController)
        {
            _laserBeam?.SourceDie();
            _laserBeam = null;
        }

        public class Factory : PlaceholderFactory<EnemyType>
        {
        }
    }
}