using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MetaShopUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text currencyLabel;
    [SerializeField] private Transform rowContainer;
    [SerializeField] private MetaUpgradeRowUI rowPrefab;
    [SerializeField] private List<MetaUpgradeConfig> upgrades = new List<MetaUpgradeConfig>();

    private readonly List<MetaUpgradeRowUI> spawnedRows = new List<MetaUpgradeRowUI>();

    public void Open()
    {
        panel.SetActive(true);
        Refresh();
    }

    public void Close() => panel.SetActive(false);

    private void OnEnable() => MetaProgressionManager.OnMetaChanged += Refresh;
    private void OnDisable() => MetaProgressionManager.OnMetaChanged -= Refresh;

    private void Refresh()
    {
        MetaProgressionManager meta = MetaProgressionManager.Instance;
        if (meta == null) return;

        if (currencyLabel != null) currencyLabel.text = meta.MetaCurrency.ToString();

        if (spawnedRows.Count == 0)
        {
            foreach (MetaUpgradeConfig upgrade in upgrades)
            {
                MetaUpgradeRowUI row = Instantiate(rowPrefab, rowContainer);
                row.Bind(upgrade, this);
                spawnedRows.Add(row);
            }
        }

        foreach (MetaUpgradeRowUI row in spawnedRows)
            row.Refresh();
    }

    public void Purchase(MetaUpgradeConfig upgrade)
    {
        MetaProgressionManager.Instance?.TryPurchase(upgrade);
    }
}
