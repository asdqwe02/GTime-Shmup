using System;
using UnityEngine;
using Zenject;

namespace CasualGames.Operius2D.Configs
{
    [Serializable]
    [CreateAssetMenu(menuName = "Operius2D/Config Installer", fileName = "Operius2DConfigInstaller.asset")]
    public class Operius2DConfigInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private SpawnerConfig _spawnerConfig;
        [SerializeField] private EnemyConfig _enemyConfig;
        [SerializeField] private GameConfig _gameConfig;
        [SerializeField] private BossConfig _bossConfig;
        public override void InstallBindings()
        {
            Container.BindInstance(_playerConfig);
            Container.BindInstance(_spawnerConfig);
            Container.BindInstance(_enemyConfig);
            Container.BindInstance(_gameConfig);
            Container.BindInstance(_bossConfig);
        }
    }
}