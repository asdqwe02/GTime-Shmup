using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using Zenject;

namespace PKFramework.Lidar
{
    public class PKLidarManager: MonoBehaviour, ILidarManager
    {
        private const byte RPLIDAR_ANS_SYNC_BYTE1 = 0xA5;
        private const byte RPLIDAR_ANS_SYNC_BYTE2 = 0x5A;
        private const byte RPLIDAR_CMD_GET_INFO = 0x50;
        private const byte RPLIDAR_CMD_GET_HEALTH = 0x52;
        private const byte RPLIDAR_CMD_STOP = 0x25;
        private const byte RPLIDAR_CMD_RESET = 0x40;
        private const byte RPLIDAR_CMD_SCAN = 0x20;
        private const byte RPLIDAR_CMD_FORCE_SCAN = 0x21;
        private const byte RPLIDAR_CMD_SET_MOTOR_PWM = 0xF0;

        private const int STOP_MOTOR_PWM = 0;
        private const int DEFAULT_MOTOR_PWM = 660;
        private const int MAX_MOTOR_PWM = 1023;
        private const int RPLIDAR_RESP_MEASUREMENT_QUALITY_SHIFT = 2;
        private const int RPLIDAR_RESP_MEASUREMENT_ANGLE_SHIFT = 1;

        private const int DESCRIPTOR_LEN = 7;
        private const int INFO_LEN = 20;
        private const int HEALTH_LEN = 3;

        private const int INFO_TYPE = 4;
        private const int HEALTH_TYPE = 6;
        private const int SCAN_TYPE = 129;

        [Inject] private LidarConfig _config;

        private readonly Dictionary<LidarModel, Socket> _commandSockets = new();
        private readonly Dictionary<LidarModel, Socket> _responseSockets = new();
        private readonly Dictionary<LidarModel, Thread> _commandThread = new();
        private readonly Dictionary<LidarModel, Thread> _responseThread = new();
        
        private readonly Dictionary<LidarModel, IPEndPoint> _endpoints = new();
        private readonly Dictionary<LidarModel, byte[]> _commands = new();
        
        private readonly Dictionary<LidarModel, LidarDescriptor> _lastDescriptor = new();
        private readonly Dictionary<LidarModel, LidarInfo> _lastInfo = new();
        private readonly Dictionary<LidarModel, LidarHealth> _lastHealth = new();
        
        private readonly Dictionary<LidarModel, bool> _quitting = new ();

        private void OnDestroy()
        {
            foreach (var lidarModel in _config.Lidars)
            {
                _quitting[lidarModel] = true;
            }
        }

        private void Start()
        {
            foreach (var lidar in _config.Lidars)
            {
                _endpoints[lidar] = new IPEndPoint(IPAddress.Parse(lidar.Host), lidar.Port);
                _commands[lidar] = null;

                _lastHealth[lidar] = new LidarHealth();
                _lastInfo[lidar] = new LidarInfo();
                _lastDescriptor[lidar] = new LidarDescriptor();
                _quitting[lidar] = false;
                _commandSockets[lidar] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _responseSockets[lidar] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                _commandThread[lidar] = new Thread(() =>
                {
                    SendCommand(lidar);
                })
                {
                    IsBackground = true,
                };
                _commandThread[lidar].Start();
                
                _responseThread[lidar] = new Thread(() =>
                {
                    ReceiveData(lidar);
                })
                {
                    IsBackground = true,
                };
                _responseThread[lidar].Start();
            }

            StartCoroutine(StartScan());
        }
        
        private IEnumerator StartScan()
        {
            foreach (var lidar in _config.Lidars)
            {
                var commandSocket = _commandSockets[lidar];
                while (!commandSocket.Connected)
                {
                    yield return null;
                }
                SendCommand(lidar, RPLIDAR_CMD_GET_INFO);
            }
            
            yield return new WaitForSeconds(0.5f);

            foreach (var lidar in _config.Lidars)
            {
                SendCommand(lidar, RPLIDAR_CMD_GET_HEALTH);
            }

            yield return new WaitForSeconds(0.5f);

            foreach (var lidar in _config.Lidars)
            {
                if (_lastHealth[lidar].Status == LidarHealthStatus.ERROR)
                {
                    SendCommand(lidar, RPLIDAR_CMD_RESET);
                    yield return new WaitForSeconds(0.5f);
                    SendCommand(lidar, RPLIDAR_CMD_GET_HEALTH);
                }
                SendCommand(lidar, RPLIDAR_CMD_SET_MOTOR_PWM, BitConverter.GetBytes(lidar.MotorPwm));
            }
            yield return new WaitForSeconds(0.5f);
            foreach (var lidar in _config.Lidars)
            {
                SendCommand(lidar, RPLIDAR_CMD_SCAN, null);
            }
        }

        private void SendCommand(LidarModel lidarModel)
        {
            while (true)
            {
                var commandSocket = _commandSockets[lidarModel];
                var endpoint = _endpoints[lidarModel];
                var command = _commands[lidarModel];
                
                if (_quitting[lidarModel])
                {
                    commandSocket.Send(ProcessCommand(RPLIDAR_CMD_SET_MOTOR_PWM, BitConverter.GetBytes(STOP_MOTOR_PWM)));
                    commandSocket.Send(ProcessCommand(RPLIDAR_CMD_STOP, null));
                    Debug.Log("Stopping...");
                    commandSocket.Close();
                    break;
                }
                
                if (!commandSocket.Connected)
                {
                    commandSocket.Connect(endpoint);
                }
                try
                {
                    if (command != null)
                    {
                        commandSocket.Send(command);
                        _commands[lidarModel] = null;
                    }
                }
                catch (Exception err)
                {
                    Debug.LogError(err.Message);
                }
            }
        }
        
        private void SendCommand(LidarModel lidarModel, byte cmd, byte[] payload = null)
        {
            _commands[lidarModel] = ProcessCommand(cmd, payload);
        }

        private static byte[] ProcessCommand(byte cmd, IReadOnlyCollection<byte> payload)
        {
            var cmdBytes = new List<byte>
            {
                RPLIDAR_ANS_SYNC_BYTE1,
                cmd,
            };
            if (payload == null) return cmdBytes.ToArray();
            cmdBytes.Add((byte)payload.Count);
            cmdBytes.AddRange(payload);
            var checksum = cmdBytes.Aggregate(new byte(), (current, item) => (byte)(current ^ item));
            cmdBytes.Add(checksum);
            return cmdBytes.ToArray();
        }
        
        private void ReceiveData(LidarModel lidarModel)
        {
            while (true)
            {
                var responseSocket = _responseSockets[lidarModel];
                var endpoint = _endpoints[lidarModel];
                
                if (_quitting[lidarModel])
                {
                    responseSocket.Close();
                    break;
                }

                if (!responseSocket.Connected)
                {
                    responseSocket.Connect(endpoint);
                }
                try
                {
                    var raw = new byte[4096];
                    var bytes = responseSocket.Receive(raw);
                    UpdateDescriptorAndInfo(lidarModel, raw, bytes);
                    UpdateDescriptorAndHeath(lidarModel, raw, bytes);

                    var lidarData = new LidarData
                    {
                        Points = new List<LidarPoint>(),
                        Index = _config.Lidars.IndexOf(lidarModel),
                    };
                    if (GetDescriptor(lidarModel, raw, bytes) == null && _lastDescriptor[lidarModel].Length == 5)
                    {
                        for (var i = 0; i < bytes / 5; i++)
                        {
                            var index = 5 * i;
                            var new_scan = ((raw[index] >> 0 & 0x1) == 1) ? true : false;
                            var inversed_new_scan = ((raw[index] >> 1 & 0x1) == 1) ? true : false;
                            var quality = raw[index] >> 2;
                            if (new_scan == inversed_new_scan)
                            {
                                //New scan flags mismatch
                                break;
                            }
                            var check_bit = raw[index + 1] >> 0 & 0x1;
                            if (check_bit != 1)
                            {
                                //Check bit not equal to 1
                                break;
                            }
                            var angle = ((raw[index + 1] >> 1) + (raw[index + 2] << 7)) / 64f;
                            var distance = (raw[index + 3] + (raw[index + 4] << 8)) / 4f;
                            lidarData.Points.Add(new LidarPoint()
                            {
                                Angle = angle,
                                Distance = distance,
                                Quality = quality,
                            });
                        }
                    }
                    
                    
                    OnDataReceived?.Invoke(lidarData);
                }
                catch (Exception err)
                {
                    Debug.LogError(err.Message);
                    break;
                }
            }
        }
        
        private void UpdateDescriptorAndInfo(LidarModel lidarModel, byte[] raw, int bytes)
        {
            if (GetDescriptor(lidarModel, raw, DESCRIPTOR_LEN) != null)
            {
                UpdateInfo(lidarModel, raw, bytes, DESCRIPTOR_LEN);
            }
            else
            {
                UpdateInfo(lidarModel, raw, bytes);
            }
        }
        
        private void UpdateDescriptorAndHeath(LidarModel lidarModel, byte[] raw, int bytes)
        {
            if (GetDescriptor(lidarModel, raw, DESCRIPTOR_LEN) != null)
            {
                GetHealth(lidarModel, raw, bytes, DESCRIPTOR_LEN);
            }
            else
            {
                GetHealth(lidarModel, raw, bytes);
            }
        }
        
        private LidarDescriptor GetDescriptor(LidarModel lidarModel, byte[] raw, int bytes)
        {
            if (bytes != DESCRIPTOR_LEN)
            {
                //Descriptor length mismatch
                return null;
            }
            if (raw[0] != RPLIDAR_ANS_SYNC_BYTE1 || raw[1] != RPLIDAR_ANS_SYNC_BYTE2)
            {
                //Incorrect descriptor starting bytes
                return null;
            }

            _lastDescriptor[lidarModel].Length = raw[2] + (raw[3] << 8) + (raw[4] << 16) + ((raw[5] & 0b00111111) << 24);
            _lastDescriptor[lidarModel].IsSingle = (raw[5] & 0b11000000) == 0;
            _lastDescriptor[lidarModel].Type = raw[6];
            
            return _lastDescriptor[lidarModel];
        }
        
        private void UpdateInfo(LidarModel lidarModel, byte[] raw, int bytes, int offset = 0)
        {
            if (bytes != offset + INFO_LEN)
            {
                //Wrong get_info reply length
                return;
            }
            if (!_lastDescriptor[lidarModel].IsSingle)
            {
                //Not a single response mode
                return;
            }
            if (_lastDescriptor[lidarModel].Type != INFO_TYPE)
            {
                //Wrong response data type
                return;
            }

            _lastInfo[lidarModel].MajorModel = raw[offset + 0] >> 4;
            _lastInfo[lidarModel].SubModel = raw[offset + 0] & 0b00001111;
            _lastInfo[lidarModel].Firmware = float.Parse(raw[offset + 2] + "." + raw[offset + 1]);
            _lastInfo[lidarModel].Hardware = raw[offset + 3];
            _lastInfo[lidarModel].SerialNumber = "";
            for (var i = offset + 4; i < offset + INFO_LEN; i++)
            {
                _lastInfo[lidarModel].SerialNumber += raw[i].ToString("X2");
            }
        }
        
        private LidarHealth GetHealth(LidarModel lidarModel, byte[] raw, int bytes, int offset = 0)
        {
            if (bytes != (offset + HEALTH_LEN))
            {
                //Wrong get_info reply length
                return null;
            }
            if (!_lastDescriptor[lidarModel].IsSingle)
            {
                //Not a single response mode
                return null;
            }
            if (_lastDescriptor[lidarModel].Type != HEALTH_TYPE)
            {
                //Wrong response data type
                return null;
            }

            _lastHealth[lidarModel].Status = (LidarHealthStatus)raw[offset + 0];
            _lastHealth[lidarModel].ErrorCode = (raw[offset + 1] << 8) + raw[offset + 2];


            return _lastHealth[lidarModel];

        }

        public event LidarDataReceived OnDataReceived;
    }
}