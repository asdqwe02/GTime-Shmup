using PKFramework.Data;
using PKFramework.GraphQL;
using PKFramework.Lidar;
using PKFramework.Logger;
using PKFramework.Runner;
using PKFramework.Scene;
using PKFramework.SerialPort.Scripts;
using UnityEngine;
using Zenject;
using ILogger = PKFramework.Logger.ILogger;

namespace PKFramework.Core.Installers
{
    public class PKInstaller : MonoInstaller
    {
        [SerializeField]
        private PKRunner _pkRunner;
        [SerializeField]
        private PKGraphQLCaller _pkGraphQLCaller;
        [SerializeField] 
        private PKSceneManager _pkSceneManager;
        [SerializeField] 
        private PKLidarManager _pkLidarManager;
        [SerializeField] 
        private PKSerialPortManager _pkSerialPortManager;

        public override void InstallBindings()
        {
            Container.Bind<ILogger>().To<PKLogger>().AsSingle();
            Container.Bind<ISceneManager>().FromComponentInNewPrefab(_pkSceneManager).AsSingle().NonLazy();
            Container.Bind<IDataManager>().To<MemoryPackDataManager>().AsSingle();
            Container.Bind<IGraphQLCaller>().FromComponentInNewPrefab(_pkGraphQLCaller).AsSingle();
            Container.Bind<IRunner>().FromComponentInNewPrefab(_pkRunner).AsSingle();
#if PK_USE_LIDAR_MODULE
            Container.Bind<ILidarManager>().FromComponentInNewPrefab(_pkLidarManager).AsSingle();
#endif
#if PK_USE_SERIAL_PORT_MODULE
            Container.Bind<ISerialPortManager>().FromComponentInNewPrefab(_pkSerialPortManager).AsSingle();
#endif
        }
    }
}

