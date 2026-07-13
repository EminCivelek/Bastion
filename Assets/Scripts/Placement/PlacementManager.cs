using UnityEngine;
using UnityEngine.InputSystem;

public class PlacementManager : MonoBehaviour
{
    [SerializeField] private Camera arenaCamera;
    [SerializeField] private TowerConfig[] availableTowers;
    [SerializeField] private Transform towerContainer;
    [SerializeField] private TowerSelectionPopupUI popup;
    [SerializeField] private float altarClickRadius = 1f;

    private AltarTowerSlot[] altars;
    private AltarTowerSlot activeAltar;

    private void Start()
    {
        altars = Object.FindObjectsByType<AltarTowerSlot>(FindObjectsSortMode.None);
    }

    private void Update()
    {
        if (popup != null && popup.IsOpen) return;
        if (!TryGetTapScreenPosition(out Vector2 screenPoint)) return;

        Vector3 world = arenaCamera.ScreenToWorldPoint(screenPoint);
        world.z = 0f;

        AltarTowerSlot altar = FindNearestAltar(world);
        if (altar == null || altar.IsOccupied) return;

        OpenPopupFor(altar);
    }

    private bool TryGetTapScreenPosition(out Vector2 screenPoint)
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            screenPoint = Mouse.current.position.ReadValue();
            return true;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            screenPoint = Touchscreen.current.primaryTouch.position.ReadValue();
            return true;
        }

        screenPoint = default;
        return false;
    }

    private AltarTowerSlot FindNearestAltar(Vector3 point)
    {
        AltarTowerSlot best = null;
        float bestDist = altarClickRadius;

        foreach (AltarTowerSlot altar in altars)
        {
            if (altar == null) continue;
            float dist = Vector3.Distance(altar.transform.position, point);
            if (dist <= bestDist)
            {
                best = altar;
                bestDist = dist;
            }
        }

        return best;
    }

    private void OpenPopupFor(AltarTowerSlot altar)
    {
        activeAltar = altar;
        Time.timeScale = 0f;
        popup.Open(availableTowers, OnTowerChosen, OnPopupCancelled);
    }

    private void OnTowerChosen(TowerConfig config)
    {
        if (activeAltar != null && RunManager.Instance != null && RunManager.Instance.TrySpendCurrency(config.cost))
        {
            GameObject go = Instantiate(config.prefab, activeAltar.TowerPosition, Quaternion.identity, towerContainer);
            float damageMultiplier = MetaProgressionManager.Instance != null ? MetaProgressionManager.Instance.DamageMultiplier : 1f;
            go.GetComponent<Tower>().Initialize(config, damageMultiplier);
            activeAltar.MarkOccupied();
        }

        ResumeGame();
    }

    private void OnPopupCancelled()
    {
        ResumeGame();
    }

    private void ResumeGame()
    {
        activeAltar = null;
        Time.timeScale = 1f;
    }
}
