using System;
using UnityEngine;

namespace PKFramework.Lidar
{
    [Serializable]
    public class LidarModel
    {
        [SerializeField]
        private string _host;
        public string Host => Host;
        [SerializeField]
        private int _port;
        public int Port => _port;
        [Range(0, 1023)]
        [SerializeField]
        private int _motorPwm = 660;
        public int MotorPwm => _motorPwm;
    }
}