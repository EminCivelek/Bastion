using System;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public static readonly HashSet<Enemy> Active = new HashSet<Enemy>();
    public static event Action<Enemy> OnEnemyKilled; // died to tower damage - grants reward
    public static event Action<Enemy> OnEnemyRemoved; // removed from play for any reason (killed or leaked)

    [SerializeField] private float waypointArriveThreshold = 0.1f;

    public EnemyConfig Config { get; private set; }
    public float CurrentHP { get; private set; }

    private Transform[] waypoints;
    private int waypointIndex;

    public void Initialize(EnemyConfig config, Transform[] path)
    {
        Config = config;
        waypoints = path;
        waypointIndex = 0;
        CurrentHP = config.maxHP;
    }

    private void OnEnable() => Active.Add(this);
    private void OnDisable() => Active.Remove(this);

    private void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[waypointIndex];
        Vector3 pos = transform.position;
        Vector3 toTarget = target.position - pos;

        if (toTarget.sqrMagnitude <= waypointArriveThreshold * waypointArriveThreshold)
        {
            waypointIndex++;
            if (waypointIndex >= waypoints.Length)
            {
                ReachEnd();
                return;
            }
            target = waypoints[waypointIndex];
            toTarget = target.position - pos;
        }

        Vector3 dir = toTarget.normalized;
        transform.position = pos + dir * Config.moveSpeed * Time.deltaTime;
    }

    public void TakeDamage(float amount)
    {
        float reduced = amount * (100f / (100f + Config.armor));
        CurrentHP -= reduced;
        if (CurrentHP <= 0f)
            Die();
    }

    private void Die()
    {
        OnEnemyKilled?.Invoke(this);
        OnEnemyRemoved?.Invoke(this);
        Destroy(gameObject);
    }

    private void ReachEnd()
    {
        PlayerBase.Instance?.TakeDamage(Config.baseHitDamageToBase);
        OnEnemyRemoved?.Invoke(this);
        Destroy(gameObject);
    }
}
