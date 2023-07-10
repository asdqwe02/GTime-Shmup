using PKFramework.GraphQL;
using PKFramework.Lidar;
using PKFramework.Logger;
using PKFramework.Scene;
using PKFramework.SerialPort.Scripts;
using PKFramework.Utils;
using UnityEngine;
using Zenject;

namespace PKFramework.Core.Installers
{
    [CreateAssetMenu(fileName = "PKSettingsInstaller", menuName = "PK/Installers/PKSettingsInstaller")]
    public class PKSettingsInstaller  : ScriptableObjectInstaller<PKSettingsInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInstance(ConfigHelper.GetConfig<LogConfig>());
            Container.BindInstance(ConfigHelper.GetConfig<GraphQLConfig>());
            Container.BindInstance(ConfigHelper.GetConfig<SceneConfig>());
#if PK_USE_LIDAR_MODULE
            Container.BindInstance(ConfigHelper.GetConfig<LidarConfig>());
#endif
#if PK_USE_SERIAL_PORT_MODULE
            Container.BindInstance(ConfigHelper.GetConfig<SerialPortConfig>());
#endif
        }
    }

}