using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [SerializeField] private TMP_Text currencyLabel;
    [SerializeField] private TMP_Text waveLabel;
    [SerializeField] private Image baseHPFill;
    [SerializeField] private TMP_Text baseHPLabel;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultLabel;
    [SerializeField] private TMP_Text resultRewardLabel;

    private void OnEnable()
    {
        RunManager.OnRunCurrencyChanged += HandleCurrencyChanged;
        WaveSpawner.OnWaveStarted += HandleWaveStarted;
        PlayerBase.OnHPChanged += HandleBaseHPChanged;
        RunManager.OnRunEnded += HandleRunEnded;

        if (resultPanel != null) resultPanel.SetActive(false);
    }

    private void OnDisable()
    {
        RunManager.OnRunCurrencyChanged -= HandleCurrencyChanged;
        WaveSpawner.OnWaveStarted -= HandleWaveStarted;
        PlayerBase.OnHPChanged -= HandleBaseHPChanged;
        RunManager.OnRunEnded -= HandleRunEnded;
    }

    private void HandleCurrencyChanged(int amount)
    {
        if (currencyLabel != null) currencyLabel.text = amount.ToString();
    }

    private void HandleWaveStarted(int waveIndex, int totalWaves)
    {
        if (waveLabel != null) waveLabel.text = $"Wave {waveIndex + 1}/{totalWaves}";
    }

    private void HandleBaseHPChanged(float current, float max)
    {
        if (baseHPFill != null) baseHPFill.fillAmount = max > 0f ? current / max : 0f;
        if (baseHPLabel != null) baseHPLabel.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
    }

    private void HandleRunEnded(RunResult result, int metaReward)
    {
        if (resultPanel == null) return;

        resultPanel.SetActive(true);
        if (resultLabel != null) resultLabel.text = result == RunResult.Won ? "Victory" : "Defeat";
        if (resultRewardLabel != null) resultRewardLabel.text = $"+{metaReward} Meta Currency";
    }
}
