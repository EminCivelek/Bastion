using UnityEngine;

public class Tower : MonoBehaviour
{
    [SerializeField] private TowerConfig config;
    [SerializeField] private Transform firePoint;

    private float fireCooldown;
    private float damageMultiplier = 1f;

    public void Initialize(TowerConfig towerConfig, float metaDamageMultiplier)
    {
        config = towerConfig;
        damageMultiplier = metaDamageMultiplier;
    }

    private void Update()
    {
        fireCooldown -= Time.deltaTime;
        if (fireCooldown > 0f) return;

        Enemy target = FindTarget();
        if (target == null) return;

        Fire(target);
        fireCooldown = 1f / Mathf.Max(0.01f, config.fireRate);
    }

    private Enemy FindTarget()
    {
        Enemy best = null;
        float bestScore = float.NegativeInfinity;
        Vector3 origin = transform.position;

        foreach (Enemy enemy in Enemy.Active)
        {
            if (enemy == null) continue;
            float dist = Vector3.Distance(origin, enemy.transform.position);
            if (dist > config.range) continue;

            float score = config.targeting switch
            {
                TargetingType.Closest => -dist,
                TargetingType.Strongest => enemy.CurrentHP,
                TargetingType.Weakest => -enemy.CurrentHP,
                _ => 0f,
            };

            if (best == null || score > bestScore)
            {
                best = enemy;
                bestScore = score;
            }
        }

        return best;
    }

    private void Fire(Enemy target)
    {
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        if (config.projectilePrefab != null)
        {
            GameObject go = Instantiate(config.projectilePrefab, spawnPos, Quaternion.identity);
            go.GetComponent<Projectile>().Initialize(target, config.damage * damageMultiplier, config.projectileSpeed, config.splashRadius);
        }
        else
        {
            target.TakeDamage(config.damage * damageMultiplier);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (config == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, config.range);
    }
}
