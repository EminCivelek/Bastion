using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WaveEntry
{
    public EnemyConfig enemy;
    public int count = 5;
    public float spawnInterval = 0.8f;
    public float delayBeforeWave = 3f;
}

[CreateAssetMenu(fileName = "WaveConfig", menuName = "Bastion/Wave Config (Difficulty Tier)")]
public class WaveConfig : ScriptableObject
{
    public string tierId;
    public int tierIndex;
    public List<WaveEntry> waves = new List<WaveEntry>();
    public int baseStartingCurrency = 50;
}
