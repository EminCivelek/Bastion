using System;
using System.Collections.Generic;
using UnityEngine;

public class MetaProgressionManager : MonoBehaviour
{
    public static MetaProgressionManager Instance { get; private set; }
    public static event Action OnMetaChanged;

    [SerializeField] private List<MetaUpgradeConfig> allUpgrades = new List<MetaUpgradeConfig>();

    private const string CurrencyKey = "Bastion_MetaCurrency";
    private const string UpgradeKeyPrefix = "Bastion_Upgrade_";
    private const string TierKey = "Bastion_HighestTier";

    public int MetaCurrency { get; private set; }
    public int HighestUnlockedTier { get; private set; }

    public float DamageMultiplier { get; private set; } = 1f;
    public int StartingCurrencyBonus { get; private set; }
    public float BaseMaxHPBonus { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public int GetUpgradeLevel(string upgradeId)
    {
        return PlayerPrefs.GetInt(UpgradeKeyPrefix + upgradeId, 0);
    }

    public bool TryPurchase(MetaUpgradeConfig upgrade)
    {
        int level = GetUpgradeLevel(upgrade.upgradeId);
        if (level >= upgrade.maxLevel) return false;

        int cost = upgrade.CostForLevel(level);
        if (MetaCurrency < cost) return false;

        MetaCurrency -= cost;
        PlayerPrefs.SetInt(UpgradeKeyPrefix + upgrade.upgradeId, level + 1);
        PlayerPrefs.SetInt(CurrencyKey, MetaCurrency);
        PlayerPrefs.Save();

        RecalculateBonuses();
        OnMetaChanged?.Invoke();
        return true;
    }

    public void AddCurrency(int amount)
    {
        MetaCurrency += amount;
        PlayerPrefs.SetInt(CurrencyKey, MetaCurrency);
        PlayerPrefs.Save();
        OnMetaChanged?.Invoke();
    }

    public void UnlockTier(int tierIndex)
    {
        if (tierIndex <= HighestUnlockedTier) return;
        HighestUnlockedTier = tierIndex;
        PlayerPrefs.SetInt(TierKey, HighestUnlockedTier);
        PlayerPrefs.Save();
        OnMetaChanged?.Invoke();
    }

    private void RecalculateBonuses()
    {
        DamageMultiplier = 1f;
        StartingCurrencyBonus = 0;
        BaseMaxHPBonus = 0f;

        foreach (MetaUpgradeConfig upgrade in allUpgrades)
        {
            int level = GetUpgradeLevel(upgrade.upgradeId);
            if (level <= 0) continue;

            switch (upgrade.effect)
            {
                case MetaUpgradeEffect.DamageMultiplier:
                    DamageMultiplier += upgrade.effectPerLevel * level;
                    break;
                case MetaUpgradeEffect.StartingCurrency:
                    StartingCurrencyBonus += Mathf.RoundToInt(upgrade.effectPerLevel * level);
                    break;
                case MetaUpgradeEffect.BaseMaxHP:
                    BaseMaxHPBonus += upgrade.effectPerLevel * level;
                    break;
            }
        }
    }

    public bool IsTowerUnlocked(TowerConfig tower)
    {
        if (tower.metaUnlockTier <= 0) return true;

        foreach (MetaUpgradeConfig upgrade in allUpgrades)
        {
            if (upgrade.effect == MetaUpgradeEffect.UnlockTower && upgrade.towerToUnlock == tower)
                return GetUpgradeLevel(upgrade.upgradeId) > 0;
        }
        return false;
    }

    private void Load()
    {
        MetaCurrency = PlayerPrefs.GetInt(CurrencyKey, 0);
        HighestUnlockedTier = PlayerPrefs.GetInt(TierKey, 0);
        RecalculateBonuses();
    }
}
