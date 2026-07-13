using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerOptionRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text costLabel;
    [SerializeField] private Button selectButton;

    public void Bind(TowerConfig config, bool affordable, TowerSelectionPopupUI popup)
    {
        if (nameLabel != null) nameLabel.text = config.displayName;
        if (costLabel != null) costLabel.text = $"{config.cost}g";

        selectButton.interactable = affordable;
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => popup.ChooseTower(config));
    }
}
