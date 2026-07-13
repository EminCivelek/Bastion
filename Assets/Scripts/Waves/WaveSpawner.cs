using System;
using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    public static event Action<int, int> OnWaveStarted; // waveIndex, totalWaves
    public static event Action OnAllWavesSpawned;

    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform[] path;

    private WaveConfig waveConfig;
    private int enemiesAlive;
    public bool AllWavesSpawned { get; private set; }
    public bool AllEnemiesCleared => AllWavesSpawned && enemiesAlive <= 0;

    private void OnEnable()
    {
        Enemy.OnEnemyRemoved += HandleEnemyRemoved;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyRemoved -= HandleEnemyRemoved;
    }

    public void BeginRun(WaveConfig config)
    {
        waveConfig = config;
        AllWavesSpawned = false;
        enemiesAlive = 0;
        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        for (int i = 0; i < waveConfig.waves.Count; i++)
        {
            WaveEntry wave = waveConfig.waves[i];
            yield return new WaitForSeconds(wave.delayBeforeWave);

            OnWaveStarted?.Invoke(i, waveConfig.waves.Count);

            for (int n = 0; n < wave.count; n++)
            {
                SpawnEnemy(wave.enemy);
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }

        AllWavesSpawned = true;
        OnAllWavesSpawned?.Invoke();
    }

    private void SpawnEnemy(EnemyConfig config)
    {
        GameObject go = Instantiate(config.prefab, spawnPoint.position, Quaternion.identity);
        Enemy enemy = go.GetComponent<Enemy>();
        enemy.Initialize(config, path);
        enemiesAlive++;
    }

    private void HandleEnemyRemoved(Enemy enemy)
    {
        enemiesAlive--;
    }
}
