using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Enemy target;
    private float damage;
    private float speed;
    private float splashRadius;

    public void Initialize(Enemy targetEnemy, float dmg, float projectileSpeed, float splash)
    {
        target = targetEnemy;
        damage = dmg;
        speed = projectileSpeed;
        splashRadius = splash;
    }

    private void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 pos = transform.position;
        Vector3 toTarget = target.transform.position - pos;
        float step = speed * Time.deltaTime;

        if (toTarget.magnitude <= step)
        {
            Hit(target.transform.position);
            return;
        }

        transform.position = pos + toTarget.normalized * step;
    }

    private void Hit(Vector3 point)
    {
        if (splashRadius > 0f)
        {
            foreach (Enemy enemy in Enemy.Active)
            {
                if (enemy == null) continue;
                if (Vector3.Distance(enemy.transform.position, point) <= splashRadius)
                    enemy.TakeDamage(damage);
            }
        }
        else
        {
            target.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
