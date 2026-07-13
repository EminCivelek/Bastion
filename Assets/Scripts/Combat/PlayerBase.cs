using System;
using UnityEngine;

public class PlayerBase : MonoBehaviour
{
    public static PlayerBase Instance { get; private set; }
    public static event Action OnBaseDestroyed;
    public static event Action<float, float> OnHPChanged; // current, max

    [SerializeField] private float maxHP = 20f;
    public float CurrentHP { get; private set; }
    public float MaxHP => maxHP;

    private void Awake()
    {
        Instance = this;
    }

    public void Initialize(float bonusMaxHP)
    {
        maxHP += bonusMaxHP;
        CurrentHP = maxHP;
        OnHPChanged?.Invoke(CurrentHP, maxHP);
    }

    public void TakeDamage(float amount)
    {
        if (CurrentHP <= 0f) return;

        CurrentHP = Mathf.Max(0f, CurrentHP - amount);
        OnHPChanged?.Invoke(CurrentHP, maxHP);

        if (CurrentHP <= 0f)
            OnBaseDestroyed?.Invoke();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
