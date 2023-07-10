using System.Collections;
using CasualGames.Operius2D.Configs;
using CasualGames.Operius2D.Signals;
using CasualGames.Scenes.Operius2DScene;
using UnityEngine;
using Zenject;

namespace CasualGames.Operius2D
{
    public class PlayerController : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;
        [Inject] private PlayerConfig _playerConfig;
        [Inject] private BulletController.Pool _bulletPool;
        [Inject] private GameConfig _gameConfig;
        [Inject] private Operius2DSceneLogic _logic;
        private int _direction = 1;
        private Vector3 _ogPosition;
        private Coroutine _shieldTransitionCoroutine;

        [SerializeField] private Transform _firePosition;
        [SerializeField] private Animator _shieldAnimator;
        private Animator _animator;

        private static readonly int ShieldExpire = Animator.StringToHash("ShieldExpire");

        private void Awake()
        {
            _ogPosition = transform.position;
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            // _fireTime = _playerConfig.FireInterval;
            _signalBus.Subscribe<PlayerFireBullet>(OnPlayerFireBullet);
            _signalBus.Subscribe<GameResetSignal>(OnGameResetSignal);
            _signalBus.Subscribe<TogglePlayerShield>(OnPlayerShieldSignal);
            _signalBus.Subscribe<PlayerHitSignal>(OnPlayerHitSignal);
            _signalBus.Subscribe<ChangeDirectionSignal>(OnChangeDirection);
        }

        private void OnChangeDirection()
        {
            _direction = -_direction;
        }

        private void OnPlayerHitSignal(PlayerHitSignal obj)
        {
            if (!_logic.Invincible) // dumb
            {
                StartCoroutine(TriggerInvicibleAnimation());
            }
        }

        IEnumerator TriggerInvicibleAnimation()
        {
            _animator.SetTrigger("Invincible");
            yield return new WaitForSeconds(_playerConfig.InvincibleTime);
            _animator.SetTrigger("Normal");
        }

        private void OnPlayerShieldSignal(TogglePlayerShield obj)
        {
            if (obj.Activate)
            {
                _shieldAnimator.gameObject.SetActive(true);
                _shieldTransitionCoroutine = StartCoroutine(ShieldTransition());
            }
            else
            {
                _shieldAnimator.gameObject.SetActive(false);
                StopCoroutine(_shieldTransitionCoroutine);
            }
        }

        IEnumerator ShieldTransition()
        {
            yield return new WaitForSeconds(_playerConfig.ShieldDuration * .75f);
            _shieldAnimator.SetTrigger(ShieldExpire);
        }

        private void OnDestroy()
        {
            _signalBus.Unsubscribe<PlayerFireBullet>(OnPlayerFireBullet);
            _signalBus.Unsubscribe<GameResetSignal>(OnGameResetSignal);
            _signalBus.Unsubscribe<TogglePlayerShield>(OnPlayerShieldSignal);
            _signalBus.Unsubscribe<PlayerHitSignal>(OnPlayerHitSignal);
            _signalBus.Unsubscribe<ChangeDirectionSignal>(OnChangeDirection);
        }

        private void OnGameResetSignal(GameResetSignal obj)
        {
            transform.position = _ogPosition;
        }

        private void OnPlayerFireBullet(PlayerFireBullet obj)
        {
            var bullet = _bulletPool.Spawn(new BulletData
            {
                FirePosition = _firePosition.position,
                FromPlayer = true,
                Speed = _gameConfig.PlayerBulletSpeed,
                Direction = Vector2.up,
                Color = Color.white,
                BulletSprite = _gameConfig.PlayerBulletSprite
            });
        }


        private void OnTriggerExit2D(Collider2D other)
        {
            // if (other.CompareTag("Bound"))
            // {
            //     if (transform.position.x < other.transform.position.x - other.bounds.size.x / 2)
            //         _direction = 1;
            //     else if (transform.position.x > other.transform.position.x + other.bounds.size.x / 2)
            //     {
            //         _direction = -1;
            //     }
            // }
        }

        private void FixedUpdate()
        {
            if (transform.position.x <= _logic.HorizontalBound.x)
            {
                _direction = 1;
            }
            else if (transform.position.x >= _logic.HorizontalBound.y)
            {
                _direction = -1;
            }

            transform.position += new Vector3(_playerConfig.SideWayMoveSpeed * _direction * Time.fixedDeltaTime, 0, 0);
        }
    }
}