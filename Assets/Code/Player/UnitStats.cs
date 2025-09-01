using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitStats : MonoBehaviour, IUnitStatsProvider
{
    public const float STAMINA_UNLOCK_THRESHOLD = 0.25f;

    public event Action OnDeath = null;
    public event Action OnStatsChanged = null;

    [SerializeField] private UnitStat health = new UnitStat(100f);
    [SerializeField] private UnitStat stamina = new UnitStat(20f);
    [SerializeField] private UnitStat mana = new UnitStat(50f);

    [SerializeField, Min(0f)] private float hpRegenSpeed = 0.1f;
    [SerializeField, Min(0f)] private float staminaRegenSpeed = 0.1f;
    [SerializeField, Min(0f)] private float staminaRunCost = 0.5f;
    [SerializeField, Min(0f)] private float manaRegenSpeed = 0.2f;

    public PlayerInput Player { get; set; } = null;
    public IPlayerColorProvider ColorProvider { get; set; } = null;

    public UnitStat Health => health;
    public UnitStat Stamina => stamina;
    public UnitStat Mana => mana;
    public int PlayerIndex => Player != null ? Player.playerIndex : -1;

    public bool IsStaminaAboveThreshold => stamina.Percent01 > STAMINA_UNLOCK_THRESHOLD;
    public float StaminaRunCost => staminaRunCost;

    private float damageReceivedThisFrame = 0f;
    private float staminaUsedThisFrame = 0f;
    private float manaUsedThisFrame = 0f;

    private void Update()
    {
        if (health.Current <= 0f)
        {
            return; //Dead
        }

        bool _anyStatUpdated = false;
        float _dt = Time.deltaTime;

        if (hpRegenSpeed > 0f
            && health.IsMax == false
            && health.ChangeCurrent(hpRegenSpeed * _dt))
        {
            _anyStatUpdated = true;
        }

        if (staminaRegenSpeed > 0f
            && stamina.IsMax == false
            && stamina.ChangeCurrent(staminaRegenSpeed * _dt))
        {
            _anyStatUpdated = true;
        }

        if (manaRegenSpeed > 0f
            && mana.IsMax == false
            && mana.ChangeCurrent(manaRegenSpeed * _dt))
        {
            _anyStatUpdated = true;
        }

        if (manaUsedThisFrame != 0f || staminaUsedThisFrame != 0f || damageReceivedThisFrame != 0f)
        {
            manaUsedThisFrame = 0f;
            staminaUsedThisFrame = 0f;
            damageReceivedThisFrame = 0f;
            _anyStatUpdated = true;
        }

        if (_anyStatUpdated)
        {
            OnStatsChanged?.Invoke();
        }
    }

    public bool CanUseMana(float _mana, bool _subtract = true)
    {
        if (mana.Current >= _mana)
        {
            if (_subtract)
            {
                manaUsedThisFrame += _mana;
                mana.Current -= _mana;
            }

            return true;
        }

        return false;
    }

    public bool CanUseStamina(float _stamina, bool _subtract = true)
    {
        if (stamina.Current >= _stamina)
        {
            if (_subtract)
            {
                staminaUsedThisFrame += _stamina;
                stamina.Current -= _stamina;
            }

            return true;
        }

        return false;
    }

    public bool CanReceiveDamage(float _damage, bool _subtract = true)
    {
        if (health.Current > 0f)
        {
            if (_subtract)
            {
                damageReceivedThisFrame += _damage;
                health.Current -= _damage;

                if (health.Current <= 0f)
                {
                    OnDeath?.Invoke();
                }
            }

            return true;
        }

        return false;
    }
}
