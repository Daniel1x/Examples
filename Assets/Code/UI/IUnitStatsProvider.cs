public interface IUnitStatsProvider : IDamageable, IPlayerDataProvider, IStaminaUser, IManaUser
{
    public event System.Action OnStatsChanged;
}

public interface IDamageable
{
    public event System.Action OnDamaged;
    public event System.Action OnHealed;
    public event System.Action OnDeath;

    public UnitStat Health { get; }

    public bool CanReceiveDamage(float _damage, bool _apply = true);
    public bool CanHeal(float _healing, bool _apply = true);
}

public interface IPlayerDataProvider
{
    public int PlayerIndex { get; }
    public IPlayerColorProvider PlayerColor { get; }
}

public interface IStaminaUser
{
    public UnitStat Stamina { get; }
    public bool CanUseStamina(float _stamina, bool _apply = true);
    public bool CanRestoreStamina(float _stamina, bool _apply = true);
}

public interface IManaUser
{
    public UnitStat Mana { get; }
    public bool CanUseMana(float _mana, bool _apply = true);
    public bool CanRestoreMana(float _mana, bool _apply = true);
}

public interface ICanApplyDamage
{
    public event System.Action OnCanApplyDamageStateUpdated;
    public AttackSide CanApplyDamage { get; }
}
