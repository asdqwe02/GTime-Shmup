using JetBrains.Annotations;
using PKFramework.Core.Editor;

namespace PKFramework.SerialPort.Scripts.Editor
{
    [UsedImplicitly]
    public class SerialPortMenu: Menu<SerialPortConfig>
    {
        public override string MenuName => "Settings/Serial Port";
    }
}