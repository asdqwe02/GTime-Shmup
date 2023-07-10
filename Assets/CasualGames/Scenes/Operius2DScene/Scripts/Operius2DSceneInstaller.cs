
using CasualGames.Operius2D;
using CasualGames.Operius2D.Configs;
using CasualGames.Operius2D.Models;
using CasualGames.Operius2D.Signals;
using Zenject;

namespace CasualGames.Scenes.Operius2DScene
{
    public class Operius2DSceneInstaller : MonoInstaller
    {
        [Inject] private SpawnerConfig _enemyConfig;
        [Inject] private PlayerConfig _playerConfig;
        [Inject] private GameConfig _gameConfig;

        public override void InstallBindings()
        {
            Container.Bind<Operius2DSceneLogic>().AsSingle();
            Container.Bind<PoolCache>().AsSingle();
            // Memory Pool Bind
            Container.BindMemoryPool<BulletController, BulletController.Pool>()
                .WithInitialSize(0)
                .FromComponentInNewPrefab(_gameConfig.BulletPrefab)
                .AsCached();
            Container.BindMemoryPool<EnemyController, EnemyController.Pool>()
                .WithInitialSize(0)
                .FromComponentInNewPrefab(_enemyConfig.EnemyPrefab)
                .AsCached();
            Container.BindMemoryPool<PowerupController, PowerupController.Pool>()
                .WithInitialSize(0)
                .FromComponentInNewPrefab(_gameConfig.PowerUpPrefab)
                .AsCached();
            Container.Bind<PlayerController>() // need to change this to allow multiple player for interactive game
                .FromComponentInHierarchy(false)
                .AsSingle();
            Container.BindMemoryPool<LaserBeam, LaserBeam.Pool>()
                .WithInitialSize(0)
                .FromComponentInNewPrefab(_gameConfig.LaserBeam)
                .AsCached();

            // Signal Declare
            Container.DeclareSignal<PlayerFireBullet>();
            Container.DeclareSignal<SpawnEnemySignal>();
            Container.DeclareSignal<PlayerHitSignal>();
            Container.DeclareSignal<UpdatePlayerLife>();
            Container.DeclareSignal<GameResetSignal>();
            // Container.DeclareSignal<EnemyHitSignal>();
            Container.DeclareSignal<EnemyDestroySignal>();
            Container.DeclareSignal<GameEndSignal>();
            Container.DeclareSignal<UpdateScoreSignal>();
            Container.DeclareSignal<TogglePlayerShield>();
            Container.DeclareSignal<PowerUpSignal>();
            Container.DeclareSignal<BossFireBullet>();
            Container.DeclareSignal<NextWaveCountDownSignal>();
            Container.DeclareSignal<CameraEffectSignal>();
            Container.DeclareSignal<StartGameSignal>();
            Container.DeclareSignal<ChangeDirectionSignal>();
            Container.DeclareSignal<UpdateCoinCountSignal>();
            // Factory Bind
            Container.BindFactory<EnemyType, BasicType.Factory>().To<BasicType>();
            Container.BindFactory<EnemyType, SpreadShotType.Factory>().To<SpreadShotType>();
            Container.BindFactory<EnemyType, LaserBeamerType.Factory>().To<LaserBeamerType>();
            Container.BindFactory<EnemyType, SpikeExplodeType.Factory>().To<SpikeExplodeType>();
        }
    }
}