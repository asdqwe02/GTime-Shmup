using System;
using System.Collections.Generic;
using UnityEngine;

namespace CasualGames.Operius2D.Configs
{
    [Serializable]
    [CreateAssetMenu(menuName = "Operius2D/Spawner Config", fileName = "SpawnerConfig.asset")]
    public class SpawnerConfig : ScriptableObject
    {
        public Enemy EnemyPrefab;
        public int MinWaveUntilBoss;
        public int MaxWaveUntilBoss;
        public Vector2 BaseWaveSpacing = Vector2.one;
        public int BaseWaveColumn;
        public int BaseWaveRow;
        public int EnemyExponentialVal;
        
        [Header("Custom Waves")]
        public int NumberOfCacheCustomWaves;
        public float CustomWaveChance;
        public List<BasicEnemyWave> BasicEnemyWaves;
        public List<BossEnemyWave> BossEnemyWaves;

        // should make a dictionary to group wave difficulty easier
    }

    [Serializable]
    public class Wave
    {
        // public WaveDifficulty WaveDifficulty;
        // public float Weighting;
    }

    public enum WaveDifficulty
    {
        EASY,
        NORMAL,
        MEDIUM,
        HARD,
    }

    public enum WaveType
    {
        BASIC,
        BOSS,
        OBSTACLE,
        BONUS
    }

    [Serializable]
    public class BasicEnemyWave : Wave
    {
        // public int NumberOfEnemy;
        // public Vector2 Spacing;
        // public float FireInterval;
        // public float DiveCoolDown;
        public EnemyWaveMatrix EnemyWaveMatrix;
    }

    [Serializable]
    public class BossEnemyWave : Wave
    {
        public BossEnemyController BossEnemyPrefasb;
        public BossStat BossStat;
        public int NumberOfEnemy;
    }
}