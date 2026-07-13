using UnityEngine;

public enum TargetingType { First, Closest, Strongest, Weakest }

[CreateAssetMenu(fileName = "TowerConfig", menuName = "Bastion/Tower Config")]
public class TowerConfig : ScriptableObject
{
    public string towerId;
    public string displayName;
    public Sprite icon;
    public GameObject prefab;
    public GameObject projectilePrefab;

    public int cost = 20;
    public float range = 3.5f;
    public float damage = 10f;
    public float fireRate = 1f;
    public float projectileSpeed = 12f;
    public float splashRadius = 0f;
    public TargetingType targeting = TargetingType.First;

    public int metaUnlockTier = 0;
}
