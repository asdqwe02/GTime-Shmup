using System.IO.Ports;
using UnityEngine;

namespace PKFramework.SerialPort.Scripts
{
    [System.Serializable]
    public class SerialPortData
    {
        [SerializeField]
        private string _port;
        [SerializeField]
        private int _baudRate;
        [SerializeField]
        private Parity _parity;
        [SerializeField]
        private StopBits _stopBits;
        [SerializeField]
        private int _dataBits;
        [SerializeField]
        private bool _dtrEnable = true;
        [SerializeField]
        private bool _rtsEnable = true;
        [SerializeField]
        private int _readTimeout = 10;
        [SerializeField]
        private int _writeTimeout = 10;
        [SerializeField]
        private int _delayBeforeReconnecting = 1000;
        [SerializeField]
        private int _maxUnreadMessages = 10;

        public string Port => _port;

        public int BaudRate => _baudRate;

        public Parity Parity => _parity;

        public StopBits StopBits => _stopBits;

        public int DataBits => _dataBits;

        public bool DtrEnable => _dtrEnable;

        public bool RtsEnable => _rtsEnable;

        public int ReadTimeout => _readTimeout;

        public int WriteTimeout => _writeTimeout;

        public int DelayBeforeReconnecting => _delayBeforeReconnecting;
        public int MaxUnreadMessages => _maxUnreadMessages;
    }
}