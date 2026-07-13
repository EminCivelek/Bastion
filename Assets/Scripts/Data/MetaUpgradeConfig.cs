using UnityEngine;

public enum MetaUpgradeEffect { DamageMultiplier, StartingCurrency, BaseMaxHP, UnlockTower }

[CreateAssetMenu(fileName = "MetaUpgradeConfig", menuName = "Bastion/Meta Upgrade Config")]
public class MetaUpgradeConfig : ScriptableObject
{
    public string upgradeId;
    public string displayName;
    public string description;
    public Sprite icon;

    public int baseCost = 25;
    public float costGrowth = 1.35f;
    public int maxLevel = 10;

    public MetaUpgradeEffect effect;
    public float effectPerLevel = 0.05f;
    public TowerConfig towerToUnlock;

    public int CostForLevel(int currentLevel)
    {
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costGrowth, currentLevel));
    }
}
