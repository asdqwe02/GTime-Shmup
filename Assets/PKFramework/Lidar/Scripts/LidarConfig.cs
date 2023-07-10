using System.Collections.Generic;
using UnityEngine;

namespace PKFramework.Lidar
{
    [CreateAssetMenu(fileName = "LidarConfig.asset", menuName = "PK/Settings/Lidar Config")]

    public class LidarConfig: ScriptableObject
    {
        [SerializeField]
        private List<LidarModel> _lidars;
        public List<LidarModel> Lidars => _lidars;
    }
}