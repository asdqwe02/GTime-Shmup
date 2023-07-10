using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CasualGames.Common.Signals;
using CasualGames.Operius2D;
using CasualGames.Operius2D.Configs;
using CasualGames.Operius2D.Models;
using CasualGames.Operius2D.Signals;
using JetBrains.Annotations;
using PKFramework.Runner;
using SRF;
using UnityEngine;
using Zenject;
using ILogger = PKFramework.Logger.ILogger;
using Random = UnityEngine.Random;

namespace CasualGames.Scenes.Operius2DScene
{
    [UsedImplicitly] [Serializable]
    public class Operius2DSceneLogic
    {
        [Inject] private ILogger _logger;
        [Inject] private IRunner _runner;
        [Inject] private SignalBus _signalBus;
        [Inject] private PlayerConfig _playerConfig;
        [Inject] private EnemyConfig _enemyConfig;
        [Inject] private GameConfig _gameConfig;
        [Inject] private EnemyController.Pool _enemyPool;
        [Inject] private PoolCache _poolCache;
        [Inject] private SpawnerConfig _spawnerConfig;
        [Inject] private PlayerController _playerController;
        [Inject] private BulletController.Pool _bulletPool;
        [Inject] private PowerupController.Pool _powerUpPool;
        // [Inject] private ISerialPortManager _serialPortManager;
        private int _playerLife;
        private int _scores;
        private int _prize;
        private int _waveCount;
        private int _enemyLeft;
        private bool _invincible;
        private int _bossWaveCountDown;
        private float _nextWaveCountDown; // in seconds
        private int _difficulty;
        private float _enemyAttackInterval;
        private Vector2 _horizontalBound;
        public Vector2 HorizontalBound => _horizontalBound;
        private Vector2 _verticalBound;
        public Vector2 VerticalBound => _verticalBound;
        private Coroutine _shieldCoroutine;
        private Coroutine _spawnNextWaveCoroutine;
        private Coroutine _checkCoinCoroutine;
        private List<EnemyWaveMatrix> _customWaveMatrix = new();

        private int _coinsReturned = 0;

        public bool Invincible
        {
            get => _invincible;
            set => _invincible = value;
        }

        public bool GameEnd = true;

        public void Init()
        {
            _signalBus.Subscribe<PlayerHitSignal>(OnPlayerHit);
            _signalBus.Subscribe<EnemyDestroySignal>(OnEnemyDestroy);
            _signalBus.Subscribe<PowerUpSignal>(OnPowerUpSignal);
            _signalBus.Subscribe<GameResetSignal>(OnGameResetSignal);
            _signalBus.Subscribe<KeyDownSignal>(OnKeyDownSignal);
            _signalBus.Subscribe<CoinInsertedSignal>(OnCoinInserted);
            _signalBus.Subscribe<CoinReturnedSignal>(OnCoinReturned);
            _runner.StartCoroutine(PlayerFire());
            _runner.StartCoroutine(EnemyAttack());
            _runner.StartCoroutine(EnemyMeleeAttack());
        }

        private void OnCoinInserted(CoinInsertedSignal obj)
        {
            if (GameEnd && _coinsReturned >= _prize)
            {
                if (_checkCoinCoroutine != null)
                {
                    _runner.StopCoroutine(_checkCoinCoroutine);
                }

                _signalBus.Fire(new GameResetSignal());
            }
        }

        private void OnCoinReturned()
        {
            _coinsReturned++;
        }

        private void OnKeyDownSignal(KeyDownSignal obj)
        {
#if !ARCADE
            if (GameEnd)
            {
                _signalBus.Fire(new GameResetSignal());
            }
#endif
            _signalBus.Fire<ChangeDirectionSignal>();
        }


        public float GetEnemyWeightByType(Enemy.EnemyTypeEnum enemyType)
        {
            var weight = _enemyConfig.EnemyWeights.Find(ew => ew.EnemyTypeEnum == enemyType).Weight;
            return weight;
        }

        private void OnPowerUpSignal(PowerUpSignal obj)
        {
            if (!obj.Spawn)
            {
                switch (obj.PowerUpType)
                {
                    case PowerUpType.SHIELD:
                        if (!_invincible)
                            _shieldCoroutine = _runner.StartCoroutine(ShieldPlayer());
                        break;
                }
            }
        }


        // update score 
        private void OnEnemyDestroy(EnemyDestroySignal obj)
        {
            _enemyLeft--;
            // calculate point when enemy killed
            if (!obj.OutOfBound)
            {
                _signalBus.Fire(new UpdateScoreSignal
                {
                    Score = _scores++
                });
            }

            if (_enemyLeft <= 0 && !GameEnd)
            {
                SpawnNextWave(WaveType.BASIC);
            }
        }


        private void SpawnNextWave(WaveType waveType)
        {
            if (_spawnNextWaveCoroutine != null)
            {
                _runner.StopCoroutine(_spawnNextWaveCoroutine);
            }

            if (_waveCount != 1)
            {
                SpawnPowerUp();
                _scores += 50;
                if (_waveCount <= _gameConfig.TargetWave)
                {
                    _enemyAttackInterval -= (_enemyConfig.MaxAttackInterval - _enemyConfig.MinAttackInterval) / _gameConfig.TargetWave;
                }

                _signalBus.Fire(new UpdateScoreSignal
                {
                    Score = _scores
                });
            }

            switch (waveType)
            {
                case WaveType.BASIC:
                    // var basicWaveIndex = Random.Range(0, _spawnerConfig.BasicEnemyWaves.Count);
                    var numberOfEnemy = 0;
                    var customWaveChance = Random.Range(0f, 1f);
                    EnemyWaveMatrix customWave = null;
                    if
                    (
                        (customWaveChance <= _spawnerConfig.CustomWaveChance || _waveCount > _gameConfig.TargetWave)
                        && _customWaveMatrix.Any()
                    )
                    {
                        customWave = _customWaveMatrix.Random();
                        _customWaveMatrix.Remove(customWave);
                        _enemyLeft += customWave.GetEnemyCount();
                    }
                    else
                    {
#if ARCADE
                        if (_waveCount >= _gameConfig.MinWaveForCoins)
                        {
                            _prize++;
                            _signalBus.Fire(new UpdateCoinCountSignal
                            {
                                Coin = _prize
                            });
                        }
#endif

                        _difficulty += _gameConfig.DifficultyIncreaseValue;
                        _waveCount++;
                        numberOfEnemy = Mathf.Clamp(
                            (int)Mathf.Pow(_spawnerConfig.EnemyExponentialVal, _waveCount),
                            (int)Mathf.Pow(_spawnerConfig.EnemyExponentialVal, _waveCount),
                            _spawnerConfig.BaseWaveRow * _spawnerConfig.BaseWaveColumn
                        );
                        _enemyLeft += numberOfEnemy;
                    }

                    _signalBus.Fire<SpawnEnemySignal>(new SpawnEnemySignal
                    {
                        WaveCount = _waveCount,
                        NumberOfEnemy = numberOfEnemy,
                        EnemyWaveMatrix = customWave
                    });
                    _nextWaveCountDown = _enemyLeft * 1f; // 1s for each enemy in the formation
                    _spawnNextWaveCoroutine = _runner.StartCoroutine(NextWaveCountDown());
                    break;
                case WaveType.BOSS:
                    var bossWaveIndex = Random.Range(0, _spawnerConfig.BossEnemyWaves.Count);
                    _signalBus.Fire<SpawnEnemySignal>(new SpawnEnemySignal
                    {
                        IsBoss = true // change this later
                    });
                    _enemyLeft = _spawnerConfig.BossEnemyWaves[bossWaveIndex].NumberOfEnemy;
                    break;
                case WaveType.OBSTACLE:
                    break;
                case WaveType.BONUS:
                    break;
            }
        }

        void SpawnPowerUp()
        {
            if (Random.Range(0f, 1f) < _gameConfig.PowerUpSpawnChance)
            {
                _signalBus.Fire(new PowerUpSignal
                {
                    Spawn = true,
                    PowerUpType = PowerUpType.SHIELD
                });
            }
        }

        private void OnGameResetSignal(GameResetSignal obj)
        {
            // _runner.StartCoroutine(ResetGame());
            ResetGame();
        }

        private void OnPlayerHit(PlayerHitSignal obj)
        {
            if (_invincible)
            {
                return;
            }

            _runner.StartCoroutine(InvincibleFrame());
            _signalBus.Fire(new CameraEffectSignal
            {
                CameraEffectType = CameraEffectType.SHAKE
            });
            if (!GameEnd)
            {
                _playerLife -= obj.Damage;
                // _logger.Information($"Player Hit \n Life left: {_playerLife}");
                _signalBus.Fire(new UpdatePlayerLife
                {
                    Life = _playerLife
                });
                if (_playerLife <= 0)
                {
                    GameOver();
                }
            }
        }

        public void GameOver()
        {
            GameEnd = true;
            // _logger.Debug("game end");
            Time.timeScale = 0f;
            _signalBus.Fire(new GameEndSignal
            {
                Win = false
            });
#if ARCADE
            _coinsReturned = 0;
            if (_prize > 0)
            {
                _serialPortManager.SendMessage(_prize.ToString(), "/dev/ttyACM0");
            }

            _checkCoinCoroutine = _runner.StartCoroutine(DoCheckCoinsAndGoBackToHome(_prize));
            // _runner.StartCoroutine(ResetGame());
#endif
        }

        private IEnumerator DoCheckCoinsAndGoBackToHome(int numberOfCoins)
        {
            yield return new WaitForSecondsRealtime(10.0f);
            _logger.Information("finish 10s delay to spit out coins");
            while (_coinsReturned < numberOfCoins)
            {
                yield return null;
            }

            _signalBus.Fire(new GameEndSignal
            {
                Win = false,
                ShowStartPanel = true
            });
        }

        public void SetUpBoundary(Collider2D boundary)
        {
            var position = boundary.transform.position;
            var bounds = boundary.bounds;
            _horizontalBound = new Vector2(
                position.x - bounds.size.x / 2,
                position.x + bounds.size.x / 2
            );
            _verticalBound = new Vector2(
                position.y - bounds.size.y / 2,
                position.y + bounds.size.y / 2
            );
        }

        public void StartGame()
        {
            _signalBus.Fire(new StartGameSignal());
            GameEnd = false;
            _difficulty = 0;
            _waveCount = 0;
            _scores = 0;
            _enemyLeft = 0;
            _enemyAttackInterval = _enemyConfig.MaxAttackInterval;
            _playerLife = _playerConfig.Life;
            Time.timeScale = 1f;
            // _bossWaveCountDown = Random.Range(_spawnerConfig.MinWaveUntilBoss, _spawnerConfig.MaxWaveUntilBoss);
            CacheRandomCustomEnemyWaves();
            _signalBus.Fire(new UpdateScoreSignal
            {
                Score = _scores
            });
#if ARCADE
            _prize = 0;
            _signalBus.Fire(new UpdateCoinCountSignal
            {
                Coin = _prize
            });
#endif
            SpawnNextWave(WaveType.BASIC);
        }

        void ResetGame()
        {
            // yield return new WaitForSecondsRealtime(_gameConfig.ResetWaitTime);
            // clear stuffs from last game
            _logger.Debug("Reset Game");
            DespawnAllEnemy();
            foreach (var activeBullet in _poolCache.ActivePoolBulletRefs)
            {
                if (!activeBullet.Destroyed)
                {
                    _bulletPool.Despawn(activeBullet);
                }
            }

            foreach (var powerUp in _poolCache.ActivePowerUpRefs)
            {
                if (powerUp.gameObject.activeSelf)
                {
                    _powerUpPool.Despawn(powerUp);
                }
            }

            _poolCache.ActivePowerUpRefs.Clear();
            _poolCache.ActivePoolEnemyRefs.Clear();
            _poolCache.ActivePoolBulletRefs.Clear();
            _powerUpPool.Clear();
            _enemyPool.Clear();
            _bulletPool.Clear();
            // _signalBus.Fire<GameResetSignal>(new GameResetSignal { });
            // start game
            StartGame();
        }

        IEnumerator PlayerFire()
        {
            while (true)
            {
                yield return new WaitForSeconds(_playerConfig.FireInterval);
                _signalBus.Fire<PlayerFireBullet>(new PlayerFireBullet { });
            }
        }

        public IEnumerator ShieldPlayer()
        {
            _invincible = true;
            _signalBus.Fire<TogglePlayerShield>(new TogglePlayerShield
            {
                Activate = _invincible
            });
            yield return new WaitForSeconds(_playerConfig.ShieldDuration);
            if (_invincible)
            {
                _invincible = false;
                _signalBus.Fire<TogglePlayerShield>(new TogglePlayerShield
                {
                    Activate = _invincible
                });
            }
        }

        private IEnumerator EnemyAttack()
        {
            while (true)
            {
                if (_poolCache.ActivePoolEnemyRefs.Count > 0)
                {
                    var enemy = _poolCache.ActivePoolEnemyRefs[Random.Range(0, _poolCache.ActivePoolEnemyRefs.Count)];
                    // while (enemy.EnemyType == Enemy.EnemyType.SPIKE_EXPLODE)
                    // {
                    //     enemy = _poolCache.ActivePoolEnemyRefs[Random.Range(0, _poolCache.ActivePoolEnemyRefs.Count)];
                    // }
                    enemy.Attack();
                }

                if (_poolCache.ActivePoolEnemyRefs.Count <= _enemyConfig.MinEnemyToResetAttInterval)
                {
                    yield return new WaitForSeconds(_enemyConfig.MaxAttackInterval);
                    // _logger.Debug($"enemy attack wait longer when there are {_enemyConfig.MinEnemyToResetAttInterval} or less enemy");
                }
                else
                {
                    yield return new WaitForSeconds(_enemyAttackInterval);
                }
            }
        }

        private IEnumerator EnemyMeleeAttack()
        {
            while (true)
            {
                yield return new WaitForSeconds(_enemyConfig.DiveCoolDown);
                var meleeEnemyList = _poolCache.ActivePoolEnemyRefs.Where(e => e.EnemyType is IMeleeAttack).ToList();
                if (meleeEnemyList.Count > 0)
                {
                    // var enemy = _poolCache.ActivePoolEnemyRefs[Random.Range(0, _poolCache.ActivePoolEnemyRefs.Count)];
                    var enemy = meleeEnemyList.Random();
                    enemy.MeleeAttack(_playerController.transform.position);
                }
            }
        }

        public IEnumerator NextWaveCountDown()
        {
            float elapsed = _nextWaveCountDown;
            while (elapsed > 0)
            {
                yield return null;
                if (GameEnd)
                    yield break;
                elapsed -= Time.deltaTime;
                _signalBus.Fire<NextWaveCountDownSignal>(new NextWaveCountDownSignal
                {
                    CountDownTime = elapsed
                });
            }

            // yield return new WaitForSeconds(_nextWaveCountDown);
            // DespawnAllEnemy();
            SpawnNextWave(WaveType.BASIC);
        }

        IEnumerator InvincibleFrame()
        {
            _invincible = true;
            yield return new WaitForSeconds(_playerConfig.InvincibleTime);
            _invincible = false;
        }

        public void DespawnAllEnemy()
        {
            foreach (var activeEnemy in _poolCache.ActivePoolEnemyRefs)
            {
                if (!activeEnemy.Destroyed)
                {
                    _enemyPool.Despawn(activeEnemy);
                }
            }
        }

        public Vector2 RotateVector2(Vector2 vec, float angle)
        {
            float newAngle = Mathf.Atan2(vec.y, vec.x) + angle * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle));
        }

        public void CacheRandomCustomEnemyWaves()
        {
            _customWaveMatrix.Clear();
            List<int> rolledNumbers = new List<int>();
            int index;
            for (int i = 0; i < _spawnerConfig.NumberOfCacheCustomWaves; i++)
            {
                do
                {
                    index = Random.Range(0, _spawnerConfig.BasicEnemyWaves.Count);
                } while (rolledNumbers.Contains(index));

                rolledNumbers.Add(index);
                _customWaveMatrix.Add(_spawnerConfig.BasicEnemyWaves[index].EnemyWaveMatrix);
            }
        }

        public Enemy.EnemyTypeEnum GetRandomEnemyType()
        {
            var maxWeight = _enemyConfig.EnemyWeights.Max(w => w.Weight);
            var spawnChances = _enemyConfig.EnemyWeights.Select(e => maxWeight - Mathf.Abs(e.Weight - _difficulty)).ToList();
            var sumChance = spawnChances.Sum();
            spawnChances = spawnChances.Select(c => c / sumChance).ToList();
            var random = Random.value;
            Enemy.EnemyTypeEnum enemyType = Enemy.EnemyTypeEnum.UNDEFINED;

            var currentChance = 0f;
            // _logger.Information($"Random : {random}");
            for (int i = 0; i < spawnChances.Count; ++i)
            {
                // _logger.Information($"Enemy {i}: {_enemyConfig.EnemyWeights[i].EnemyTypeEnum} - {spawnChances[i]} - {currentChance}");

                if (random >= currentChance && random <= currentChance + spawnChances[i])
                {
                    // _logger.Information($"Select {_enemyConfig.EnemyWeights[i].EnemyTypeEnum}");

                    return _enemyConfig.EnemyWeights[i].EnemyTypeEnum;
                }

                currentChance += spawnChances[i];
            }

            return enemyType;
        }
    }
}