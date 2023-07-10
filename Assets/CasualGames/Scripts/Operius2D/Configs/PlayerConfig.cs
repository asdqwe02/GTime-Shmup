using System;
using UnityEngine;

namespace CasualGames.Operius2D.Configs
{
    [Serializable]
    [CreateAssetMenu(menuName = "Operius2D/Player Config", fileName = "PlayerConfig.asset")]
    public class PlayerConfig : ScriptableObject
    {
        public int Life;
        public float SideWayMoveSpeed;
        public float FireInterval;
        public float InvincibleTime;
        public float ShieldDuration;
    }
}