using JetBrains.Annotations;
using PKFramework.Core.Editor;

namespace PKFramework.Logger.Editor
{
    [UsedImplicitly]
    public class LogMenu: Menu<LogConfig>
    {
        public override string MenuName => "Settings/Logger";
    }
}