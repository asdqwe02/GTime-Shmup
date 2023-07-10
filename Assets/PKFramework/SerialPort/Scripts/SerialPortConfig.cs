using System.Collections.Generic;
using UnityEngine;

namespace PKFramework.SerialPort.Scripts
{
    [CreateAssetMenu(fileName = "SerialPortConfig.asset", menuName = "PK/Settings/Serial Port Config")]
    public class SerialPortConfig: ScriptableObject
    {
        [SerializeField]
        private List<SerialPortData> _serialPorts;

        public List<SerialPortData> SerialPorts => _serialPorts;
    }
}