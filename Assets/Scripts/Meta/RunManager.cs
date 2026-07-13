using System;
using UnityEngine;

public enum RunResult { InProgress, Won, Lost }

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }
    public static event Action<RunResult, int> OnRunEnded; // result, metaCurrencyEarned
    public static event Action<int> OnRunCurrencyChanged;

    [SerializeField] private WaveConfig waveConfig;
    [SerializeField] private WaveSpawner waveSpawner;
    [SerializeField] private PlayerBase playerBase;

    public int RunCurrency { get; private set; }
    public RunResult Result { get; private set; } = RunResult.InProgress;

    private int enemiesKilled;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        MetaProgressionManager meta = MetaProgressionManager.Instance;
        RunCurrency = waveConfig.baseStartingCurrency + (meta != null ? meta.StartingCurrencyBonus : 0);
        playerBase.Initialize(meta != null ? meta.BaseMaxHPBonus : 0f);

        PlayerBase.OnBaseDestroyed += HandleBaseDestroyed;
        Enemy.OnEnemyKilled += HandleEnemyKilled;
        Enemy.OnEnemyRemoved += HandleEnemyRemoved;
        WaveSpawner.OnAllWavesSpawned += CheckWinCondition;

        waveSpawner.BeginRun(waveConfig);
        OnRunCurrencyChanged?.Invoke(RunCurrency);
    }

    public bool TrySpendCurrency(int amount)
    {
        if (RunCurrency < amount) return false;
        RunCurrency -= amount;
        OnRunCurrencyChanged?.Invoke(RunCurrency);
        return true;
    }

    private void HandleEnemyKilled(Enemy enemy)
    {
        enemiesKilled++;
        RunCurrency += enemy.Config.baseGoldReward;
        OnRunCurrencyChanged?.Invoke(RunCurrency);
    }

    private void HandleEnemyRemoved(Enemy enemy)
    {
        // Fires after WaveSpawner's own OnEnemyRemoved handler (subscribed in OnEnable, which
        // always runs before this Start()), so the alive-count is already decremented here.
        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (Result != RunResult.InProgress) return;
        if (waveSpawner.AllEnemiesCleared)
            EndRun(RunResult.Won);
    }

    private void HandleBaseDestroyed()
    {
        EndRun(RunResult.Lost);
    }

    private void EndRun(RunResult result)
    {
        if (Result != RunResult.InProgress) return;
        Result = result;

        int reward = enemiesKilled * 2 + (result == RunResult.Won ? waveConfig.waves.Count * 10 : 0);
        MetaProgressionManager.Instance?.AddCurrency(reward);
        if (result == RunResult.Won)
            MetaProgressionManager.Instance?.UnlockTier(waveConfig.tierIndex + 1);

        OnRunEnded?.Invoke(result, reward);
    }

    private void OnDestroy()
    {
        PlayerBase.OnBaseDestroyed -= HandleBaseDestroyed;
        Enemy.OnEnemyKilled -= HandleEnemyKilled;
        Enemy.OnEnemyRemoved -= HandleEnemyRemoved;
        WaveSpawner.OnAllWavesSpawned -= CheckWinCondition;
    }
}
