using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CasualGames.Operius2D.Configs;
using CasualGames.Operius2D.Signals;
using CasualGames.Scenes.Operius2DScene;
using SRF;
using UnityEngine;
using Zenject;
using Random = System.Random;

namespace CasualGames.Operius2D
{
    public class EnemySpawner : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;
        [Inject] private SpawnerConfig _spawnerConfig;
        [Inject] private BossConfig _bossConfig;
        [Inject] private EnemyController.Pool _enemyPool;
        [Inject] private DiContainer _container;
        [Inject] private Operius2DSceneLogic _logic;

        [Inject] private BasicType.Factory _basicTypeFactory;
        [Inject] private SpreadShotType.Factory _spreadShotFactory;
        [Inject] private LaserBeamerType.Factory _laserBeamerFactory;
        [Inject] private SpikeExplodeType.Factory _spikeExplodeFactory;
        private int waveIndex;

        private void Awake()
        {
            _signalBus.Subscribe<SpawnEnemySignal>(OnSpawnEnemySignal);
        }

        private void OnSpawnEnemySignal(SpawnEnemySignal obj)
        {
            StartCoroutine(SpawnEnemyWave(obj));
            // else
            // {
            //     // NOTE: DO NOT USE THIS FOR NOW 
            //     var boss = _container.InstantiatePrefab(_bossConfig.BossEnemies[obj.WaveIndex].BossEnemyController);
            //     boss.transform.position = transform.position;
            //     boss.GetComponent<BossEnemyController>().SetUpStat(_bossConfig.BossEnemies[obj.WaveIndex].BossStat);
            // }
        }

        IEnumerator SpawnEnemyWave(SpawnEnemySignal obj)
        {
            if (obj.EnemyWaveMatrix == null) // spawn normal wave 
            {
                // List<bool> enemyPos = new List<bool>(new bool[_spawnerConfig.BaseWaveRow * _spawnerConfig.BaseWaveColumn]);
                // var rnd = new Random();
                // for (int i = 0; i < obj.NumberOfEnemy; i++)
                // {
                //     // Debug.Log(i);
                //     enemyPos[i] = true;
                // }

                // enemyPos = enemyPos.OrderBy(item => rnd.Next()).ToList();
                // int index = 0;

                int enemyCount = obj.NumberOfEnemy;
                for (int i = 0; i < _spawnerConfig.BaseWaveRow; i++)
                {
                    for (int j = 0; j < _spawnerConfig.BaseWaveColumn; j++)
                    {
                        // if (enemyPos[index])
                        // {
                        float xPos = _spawnerConfig.BaseWaveSpacing.x * j;
                        float yPos = -_spawnerConfig.BaseWaveSpacing.y * i;
                        Vector3 formationPos = transform.position + new Vector3(xPos, yPos, 0);
                        var enemyTypeEnum = _logic.GetRandomEnemyType();
                        EnemyType enemyType;
                        switch (enemyTypeEnum)
                        {
                            case Enemy.EnemyTypeEnum.SPREAD_SHOT:
                                enemyType = _spreadShotFactory.Create();
                                break;
                            case Enemy.EnemyTypeEnum.SPIKE_EXPLODE:
                                enemyType = _spikeExplodeFactory.Create();
                                break;
                            case Enemy.EnemyTypeEnum.LASER_SHOT:
                                enemyType = _laserBeamerFactory.Create();
                                break;
                            default:
                                enemyType = _basicTypeFactory.Create();
                                break;
                        }

                        _enemyPool.Spawn(formationPos, enemyType);
                        enemyCount--;
                        if (enemyCount <= 0)
                        {
                            yield break;
                        }

                        yield return new WaitForFixedUpdate();
                        // }
                        //
                        // index++;
                    }
                }
            }
            else // spawn custom wave
            {
                // var waveMatrix = _spawnerConfig.BasicEnemyWaves[obj.WaveIndex].EnemyWaveMatrix;
                var waveMatrix = obj.EnemyWaveMatrix;
                for (int i = 0; i < waveMatrix.Row; i++)
                {
                    for (int j = 0; j < waveMatrix.Column; j++)
                    {
                        if (waveMatrix.FormationMatrix[i, j])
                        {
                            float xPos = waveMatrix.Spacing.x * j;
                            float yPos = -waveMatrix.Spacing.y * i;
                            Vector3 formationPos = transform.position + new Vector3(xPos, yPos, 0);
                            var enemyTypeEnum = _logic.GetRandomEnemyType();
                            EnemyType enemyType;
                            switch (enemyTypeEnum)
                            {
                                case Enemy.EnemyTypeEnum.SPREAD_SHOT:
                                    enemyType = _spreadShotFactory.Create();
                                    break;
                                case Enemy.EnemyTypeEnum.SPIKE_EXPLODE:
                                    enemyType = _spikeExplodeFactory.Create();
                                    break;
                                case Enemy.EnemyTypeEnum.LASER_SHOT:
                                    enemyType = _laserBeamerFactory.Create();
                                    break;
                                default:
                                    enemyType = _basicTypeFactory.Create();
                                    break;
                            }

                            _enemyPool.Spawn(formationPos, enemyType);
                        }
                    }
                }
            }

            yield return null;
        }
    }
}