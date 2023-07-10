using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace CasualGames.Operius2D.Configs
{
    [Serializable]
    [CreateAssetMenu(menuName = "Operius2D/Enemy Config", fileName = "EnemyConfig.asset")]
    public class EnemyConfig : ScriptableObject
    {
        public float SideWaySpeed;
        public float VerticalSpeed;
        public GameObject EnemyPrefab;
        public int Hp;
        public int Damage;
        public float OutOfBoundDespawnDelay;
        public List<Color32> Colors;
        public LayerMask EnemyHitLayerMask;
        public float MaxAttackInterval;
        public float MinAttackInterval;
        public int MinEnemyToResetAttInterval;
        [Header("Basic Enemy Stats")]
        
        public float DiveCoolDown;


        [Header("Spread Shot Enemy Stats")]
        public int SpreadBullet;

        public float SpreadConeAngle;

        [Header("Laser Enemy Stats")]
        public float LaserChargeTime;

        public float LaserFireTime;
        public float LaserCoolDown;
        public Vector2 LaserOffset;
        public Vector2 LaserSize;

        [Header("Spike Explode Enemy Stats")]
        public int NumberOfSpike;

        [Header("Enemy Weighting")]
        public List<EnemyWeight> EnemyWeights;
    }

    [Serializable]
    public class EnemyWeight
    {
        public Enemy.EnemyTypeEnum EnemyTypeEnum;
        public float Weight;
    }
}