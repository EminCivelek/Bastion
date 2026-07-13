using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Bastion/Enemy Config")]
public class EnemyConfig : ScriptableObject
{
    public string enemyId;
    public string displayName;
    public GameObject prefab;

    public float maxHP = 30f;
    public float moveSpeed = 2f;
    public float armor = 0f;
    public int baseGoldReward = 5;
    public int baseHitDamageToBase = 1;
    public bool isFlying = false;
}
