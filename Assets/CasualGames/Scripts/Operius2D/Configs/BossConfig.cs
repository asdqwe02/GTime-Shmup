using System;
using System.Collections.Generic;
using UnityEngine;

namespace CasualGames.Operius2D.Configs
{
    [Serializable]
    [CreateAssetMenu(menuName = "Operius2D/Boss Config", fileName = "BossConfig.asset")]
    public class BossConfig : ScriptableObject
    {
        public List<BossEnemy> BossEnemies;
    }

    [Serializable]
    public class BossEnemy
    {
        public BossEnemyController BossEnemyController;
        public BossStat BossStat;
    }

    [Serializable]
    public class BossStat
    {
        public int HP;
        public float Speed;
        public float SkillCoolDown;
        public float FireInterval;
    }
}