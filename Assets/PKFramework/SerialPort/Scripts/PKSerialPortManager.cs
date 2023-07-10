using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Zenject;

namespace PKFramework.SerialPort.Scripts
{
    public class PKSerialPortManager: MonoBehaviour, ISerialPortManager
    {
        public event PortOpened OnPortOpened;
        public event PortClosed OnPortClosed;
        public event MessageReceived OnMessageReceived;

        private readonly Dictionary<SerialPortData, SerialThread> _ports = new();
        private readonly Dictionary<SerialPortData, Thread> _threads = new();
        
        [Inject] private SerialPortConfig _config;
        [Inject] private PKFramework.Logger.ILogger _logger;

        private void Awake()
        {
            foreach (var portData in _config.SerialPorts)
            {
                var portName = portData.Port;
                var serialThread = new SerialThread(portData);
                
                serialThread.OnConnected += () =>
                {
                    OnPortOpened?.Invoke(portName);
                };
                serialThread.OnDisconnected += () =>
                {
                    OnPortClosed?.Invoke(portName);
                };
                var thread = new Thread(serialThread.RunForever);
                thread.Start();
                _ports.Add(portData, serialThread);
                _threads.Add(portData, thread);
            }

            StartCoroutine(SerialPortLoop());
        }

        private void OnDestroy()
        {
            foreach (var serialThread in _ports)
            {
                serialThread.Value.RequestStop();
            }
            foreach (var thread in _threads)
            {
                thread.Value.Join();
            }
        }

        private IEnumerator SerialPortLoop()
        {
            while (true)
            {
                foreach (var port in _ports)
                {
                    var message = port.Value.ReadSerialMessage();
                    if (message != null)
                    {
                        _logger.Verbose($"Received message: {message.Trim()} from {port.Key.Port}");
                        OnMessageReceived?.Invoke(port.Key.Port, message.Trim());
                    }
                }
                yield return null;

            }

            // ReSharper disable once IteratorNeverReturns
        }
        
        public void SendMessage(string message, string portName = default)
        {
            _logger.Verbose($"Send {message} to {portName}");
            var portData = portName == default ? _ports.First().Key : _ports.Keys.FirstOrDefault(p => p.Port.Equals(portName));

            if (portData != null)
            {
                _logger.Information($"Found port {portName}");
                _ports[portData].SendSerialMessage(message);
            }
            else
            {
                _logger.Warning($"Port {portName} not found");
                _ports.First().Value.SendSerialMessage(message);
            }
        }
    }
}
