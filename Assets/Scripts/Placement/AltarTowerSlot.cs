using UnityEngine;

public class AltarTowerSlot : MonoBehaviour
{
    [SerializeField] private Transform towerAnchor;
    [SerializeField] private SpriteRenderer altarSprite;

    public bool IsOccupied { get; private set; }

    // The altar's transform sits at its ground-contact point, well below its visual
    // center (the sprite's own bounds), so spawning a centered tower sprite there
    // would visually sit too low. Target the sprite's actual visual center instead.
    public Vector3 TowerPosition
    {
        get
        {
            if (towerAnchor != null) return towerAnchor.position;
            if (altarSprite != null) return altarSprite.bounds.center;
            return transform.position;
        }
    }

    private void Awake()
    {
        if (altarSprite == null) altarSprite = GetComponent<SpriteRenderer>();
    }

    public void MarkOccupied()
    {
        IsOccupied = true;
        gameObject.SetActive(false);
    }
}
