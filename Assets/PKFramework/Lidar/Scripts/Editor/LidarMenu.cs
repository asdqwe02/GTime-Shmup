using JetBrains.Annotations;
using PKFramework.Core.Editor;

namespace PKFramework.Lidar.Editor
{
    [UsedImplicitly]
    public class LidarMenu: Menu<LidarConfig>
    {
        public override string MenuName => "Settings/Lidar";
    }
}