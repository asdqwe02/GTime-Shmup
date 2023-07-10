using CasualGames.Operius2D.Models;
using CasualGames.Operius2D.Signals;
using CasualGames.Scenes.Operius2DScene;
using UnityEngine;
using Zenject;
using ILogger = PKFramework.Logger.ILogger;

namespace CasualGames.Operius2D
{
    public class BulletController : MonoBehaviour
    {
        [Inject] private ILogger _logger;
        [Inject] private BulletController.Pool _bulletPool;
        [Inject] private SignalBus _signalBus;
        [Inject] private PoolCache _poolCache;
        [Inject] private Operius2DSceneLogic _logic;
        private SpriteRenderer _spriteRenderer;
        private BulletData _bulletData;
        private bool _destroyed;
        public bool Destroyed => _destroyed;

        // private bool _fromPlayer;
        //
        // private int _direction;
        // private float _speed;
        // private Vector2 _direction;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void FixedUpdate()
        {
            // transform.position += new Vector3(0f, _direction * _speed, 0f) * Time.fixedDeltaTime;
            transform.position += (Vector3)_bulletData.Direction * (_bulletData.Speed * Time.fixedDeltaTime);
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!_destroyed)
            {
                if (col.CompareTag("Enemy") && _bulletData.FromPlayer)
                {
                    Destroy();
                    col.GetComponent<Enemy>().Hit();
                    // _signalBus.Fire<EnemyHitSignal>(new EnemyHitSignal
                    // {
                    //     EnemyController = col.GetComponent<EnemyController>()
                    // });
                }

                if (col.CompareTag("Player") && !_bulletData.FromPlayer)
                {
                    Destroy();
                    _signalBus.Fire<PlayerHitSignal>(new PlayerHitSignal
                    {
                        Damage = 1
                    });
                }
            }
        }

        void Destroy()
        {
            _destroyed = true;
            _bulletPool.Despawn(this);
            _poolCache.ActivePoolBulletRefs.Remove(this);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!_destroyed)
            {
                if (other.CompareTag("Bound"))
                {
                    Destroy();
                }
            }
        }

        public class Pool : MonoMemoryPool<BulletData, BulletController>
        {
            protected override void Reinitialize(BulletData data, BulletController controller)
            {
                var transform = controller.transform;
                transform.position = data.FirePosition;
                controller._bulletData = data;
                controller._spriteRenderer.color = data.Color;
                controller._spriteRenderer.sprite = data.BulletSprite;
                transform.rotation = Quaternion.LookRotation(Vector3.forward, data.Direction);
                // controller._fromPlayer = data.FromPlayer;
                // controller._speed = data.Speed;
                // controller._direction = data.Direction;
                // if (!data.FromPlayer)
                // {
                //     controller._spriteRenderer.color = Color.red;
                //     // controller._direction = -1;
                // }
                // else
                // {
                //     controller._spriteRenderer.color = Color.white;
                //     // controller._direction = 1;
                // }
            }

            protected override void OnSpawned(BulletController item)
            {
                base.OnSpawned(item);
                // item.gameObject.SetActive(true);
                item._destroyed = false;
                if (!item._poolCache.ActivePoolBulletRefs.Contains(item))
                {
                    item._poolCache.ActivePoolBulletRefs.Add(item);
                }
            }

            protected override void OnDespawned(BulletController item)
            {
                item._destroyed = true;
                item.transform.rotation = Quaternion.identity;
                base.OnDespawned(item);
                // item.gameObject.SetActive(false);
            }
        }
    }

    public class BulletData
    {
        public bool FromPlayer;
        public float Speed;
        public Vector2 Direction;
        public Vector2 FirePosition;
        public Color32 Color;
        public Sprite BulletSprite;
    }
}