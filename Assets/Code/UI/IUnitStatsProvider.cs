public interface IUnitStatsProvider
{
    public event System.Action OnStatsChanged;

    public UnitStat Health { get; }
    public UnitStat Stamina { get; }
    public UnitStat Mana { get; }
    public int PlayerIndex { get; }
    public IPlayerColorProvider ColorProvider { get; }

    public bool CanUseMana(float _mana, bool _subtract = true);
    public bool CanUseStamina(float _stamina, bool _subtract = true);
    public bool CanReceiveDamage(float _damage, bool _subtract = true);
}
