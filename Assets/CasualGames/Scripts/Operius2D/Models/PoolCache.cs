using System;
using System.Collections.Generic;

namespace CasualGames.Operius2D.Models
{
    [Serializable]
    public class PoolCache
    {
        public List<EnemyController> ActivePoolEnemyRefs;
        public List<BulletController> ActivePoolBulletRefs;
        public List<LaserBeam> ActiveLaserBeamRefs;
        public List<PowerupController> ActivePowerUpRefs;

        PoolCache()
        {
            ActivePoolEnemyRefs = new List<EnemyController>();
            ActivePoolBulletRefs = new List<BulletController>();
            ActiveLaserBeamRefs = new List<LaserBeam>();
            ActivePowerUpRefs = new List<PowerupController>();
        }
    }
}