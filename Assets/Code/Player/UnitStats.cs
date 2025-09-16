using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitStats : MonoBehaviour, IUnitStatsProvider
{
    public event Action OnDeath = null;
    public event Action OnDamaged = null;
    public event Action OnHealed = null;
    public event Action OnStatsChanged = null;

    [SerializeField] private bool destroyOnDeath = false;
    [SerializeField] private UnitStat health = new UnitStat(100f);
    [SerializeField] private UnitStat stamina = new UnitStat(20f);
    [SerializeField] private UnitStat mana = new UnitStat(50f);

    [SerializeField, Min(0f)] private float hpRegenSpeed = 0.1f;
    [SerializeField, Min(0f)] private float staminaRegenSpeed = 0.1f;
    [SerializeField, Min(0f)] private float staminaRunCost = 0.5f;
    [SerializeField, Range(0f, 1f)] private float staminaUnlockThreshold = 0.25f;
    [SerializeField, Min(0f)] private float manaRegenSpeed = 0.2f;

    public PlayerInput Player { get; set; } = null;
    public IPlayerColorProvider PlayerColor { get; set; } = null;

    public UnitStat Health => health;
    public UnitStat Stamina => stamina;
    public UnitStat Mana => mana;
    public int PlayerIndex => Player != null ? Player.playerIndex : -1;

    public bool IsAlive => health.Current > 0f;
    public bool IsStaminaAboveThreshold => stamina.Percent01 >= staminaUnlockThreshold;
    public float StaminaRunCost => staminaRunCost;

    private float damageReceivedThisFrame = 0f;
    private float staminaUsedThisFrame = 0f;
    private float manaUsedThisFrame = 0f;

    private void Update()
    {
        if (IsAlive == false)
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

    public bool CanReceiveDamage(float _damage, bool _apply = true)
    {
        if (IsAlive == false || _damage == 0f)
        {
            return false; // Dead or no damage
        }

        if (_damage < 0f)
        {
            return CanHeal(-_damage, _apply);
        }

        if (_apply == false)
        {
            return true; // Have any health, so can receive damage
        }

        // Apply damage
        health.ChangeCurrent(-_damage);
        damageReceivedThisFrame += _damage;

        if (DamageIndicatorsManager.Instance != null)
        {
            DamageIndicatorsManager.Instance?.ShowDamageIndicator(transform.position, -_damage, DamageIndicator.DamageType.Normal);
        }

        if (health.Current <= 0f)
        {
            OnDeath?.Invoke();

            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            OnDamaged?.Invoke();
        }

        return true;
    }

    public bool CanHeal(float _heal, bool _apply = true)
    {
        if (IsAlive == false || _heal == 0f)
        {
            return false; // Dead or no heal
        }

        if (_heal < 0f)
        {
            return CanReceiveDamage(-_heal, _apply);
        }

        if (health.IsMax)
        {
            return false; // Already at max health
        }

        if (_apply == false)
        {
            return true; // Can heal
        }

        // Apply heal
        health.ChangeCurrent(_heal);
        damageReceivedThisFrame -= _heal;

        if (DamageIndicatorsManager.Instance != null)
        {
            DamageIndicatorsManager.Instance?.ShowDamageIndicator(transform.position, _heal, DamageIndicator.DamageType.Heal);
        }

        OnHealed?.Invoke();
        return true;
    }

    public bool CanUseMana(float _mana, bool _apply = true)
    {
        if (IsAlive == false || _mana == 0f)
        {
            return false; // Dead or no mana change
        }

        if (_mana < 0f)
        {
            return CanRestoreMana(-_mana, _apply);
        }

        if (mana.Current >= _mana)
        {
            if (_apply)
            {
                mana.ChangeCurrent(-_mana);
                manaUsedThisFrame += _mana;
            }

            return true;
        }

        return false;
    }

    public bool CanRestoreMana(float _mana, bool _apply = true)
    {
        if (IsAlive == false || _mana == 0f)
        {
            return false; // Dead or no mana change
        }

        if (_mana < 0f)
        {
            return CanUseMana(-_mana, _apply);
        }

        if (mana.IsMax)
        {
            return false; // Already at max mana
        }

        if (_apply == false)
        {
            return true; // Can restore mana
        }

        // Apply mana restore
        mana.ChangeCurrent(_mana);
        manaUsedThisFrame -= _mana;
        return true;
    }

    public bool CanUseStamina(float _stamina, bool _apply = true)
    {
        if (IsAlive == false || _stamina == 0f)
        {
            return false; // Dead or no stamina change
        }

        if (_stamina < 0f)
        {
            return CanRestoreStamina(-_stamina, _apply);
        }

        if (stamina.Current >= _stamina)
        {
            if (_apply)
            {
                stamina.ChangeCurrent(-_stamina);
                staminaUsedThisFrame += _stamina;
            }

            return true;
        }

        return false;
    }

    public bool CanRestoreStamina(float _stamina, bool _apply = true)
    {
        if (IsAlive == false || _stamina == 0f)
        {
            return false; // Dead or no stamina change
        }

        if (_stamina < 0f)
        {
            return CanUseStamina(-_stamina, _apply);
        }

        if (stamina.IsMax)
        {
            return false; // Already at max stamina
        }

        if (_apply == false)
        {
            return true; // Can restore stamina
        }

        // Apply stamina restore
        stamina.ChangeCurrent(_stamina);
        staminaUsedThisFrame -= _stamina;
        return true;
    }
}
