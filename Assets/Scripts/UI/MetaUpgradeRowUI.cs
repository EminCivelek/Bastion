using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MetaUpgradeRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text levelLabel;
    [SerializeField] private TMP_Text costLabel;
    [SerializeField] private Button buyButton;

    private MetaUpgradeConfig config;
    private MetaShopUI shop;

    public void Bind(MetaUpgradeConfig upgradeConfig, MetaShopUI shopUI)
    {
        config = upgradeConfig;
        shop = shopUI;
        buyButton.onClick.AddListener(() => shop.Purchase(config));
    }

    public void Refresh()
    {
        MetaProgressionManager meta = MetaProgressionManager.Instance;
        if (meta == null || config == null) return;

        int level = meta.GetUpgradeLevel(config.upgradeId);
        bool maxed = level >= config.maxLevel;

        if (nameLabel != null) nameLabel.text = config.displayName;
        if (levelLabel != null) levelLabel.text = maxed ? "MAX" : $"Lv {level}/{config.maxLevel}";
        if (costLabel != null) costLabel.text = maxed ? "-" : config.CostForLevel(level).ToString();

        buyButton.interactable = !maxed && meta.MetaCurrency >= config.CostForLevel(level);
    }
}
