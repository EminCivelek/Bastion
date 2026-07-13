using System;
using UnityEngine;

public class TowerSelectionPopupUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private UnityEngine.UI.Button backdropButton;
    [SerializeField] private Transform rowContainer;
    [SerializeField] private TowerOptionRowUI rowPrefab;

    public bool IsOpen { get; private set; }

    private Action<TowerConfig> onChosen;
    private Action onCancelled;

    private void Awake()
    {
        if (backdropButton != null) backdropButton.onClick.AddListener(Cancel);
        if (root != null) root.SetActive(false);
    }

    public void Open(TowerConfig[] towers, Action<TowerConfig> chosen, Action cancelled)
    {
        onChosen = chosen;
        onCancelled = cancelled;
        IsOpen = true;
        root.SetActive(true);

        for (int i = rowContainer.childCount - 1; i >= 0; i--)
            Destroy(rowContainer.GetChild(i).gameObject);

        int currency = RunManager.Instance != null ? RunManager.Instance.RunCurrency : 0;
        MetaProgressionManager meta = MetaProgressionManager.Instance;

        foreach (TowerConfig config in towers)
        {
            if (meta != null && !meta.IsTowerUnlocked(config)) continue;

            TowerOptionRowUI row = Instantiate(rowPrefab, rowContainer);
            row.Bind(config, currency >= config.cost, this);
        }
    }

    public void ChooseTower(TowerConfig config)
    {
        IsOpen = false;
        root.SetActive(false);
        onChosen?.Invoke(config);
    }

    public void Cancel()
    {
        IsOpen = false;
        root.SetActive(false);
        onCancelled?.Invoke();
    }
}
