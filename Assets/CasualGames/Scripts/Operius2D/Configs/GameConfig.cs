using System;
using UnityEngine;

namespace CasualGames.Operius2D.Configs
{
    [Serializable]
    [CreateAssetMenu(menuName = "Operius2D/Game Config", fileName = "GameConfig.asset")]
    public class GameConfig : ScriptableObject
    {
        public Vector2 BoundaryPadding;
        public float ResetWaitTime;
        public float PlayerBulletSpeed;
        public float EnemyBulletSpeed;
        public BulletController BulletPrefab;
        public PowerupController PowerUpPrefab;
        public LaserBeam LaserBeam;
        public float PowerUpMoveSpeed;
        public float PowerUpSpawnChance;
        public Sprite PlayerBulletSprite;
        public Sprite EnemyBulletSprite;
        public int DifficultyIncreaseValue;
        public int TargetWave;
        public int MinWaveForCoins;
    }
}