using TMPro;
using UnityEngine;

public class PlayerStatsDisplay : MonoBehaviour
{
    [SerializeField] private ProgressCircleController healthBar = null;
    [SerializeField] private ProgressCircleController staminaBar = null;
    [SerializeField] private ProgressCircleController manaBar = null;
    [SerializeField] private TMP_Text playerID = null;

    private IUnitStatsProvider statsProvider = null;

    private void OnDestroy()
    {
        if (statsProvider != null)
        {
            statsProvider.OnStatsChanged -= UpdateStats;
        }
    }

    public void Initialize(IUnitStatsProvider _statsProvider)
    {
        if (statsProvider != null)
        {
            statsProvider.OnStatsChanged -= UpdateStats;
        }

        statsProvider = _statsProvider;

        if (statsProvider != null)
        {
            if (playerID != null)
            {
                playerID.text = "P" + _statsProvider.PlayerIndex;
                playerID.color = _statsProvider.ColorProvider.PlayerColor;
            }

            statsProvider.OnStatsChanged += UpdateStats;
        }

        UpdateStats();
    }

    public void UpdateStats()
    {
        if (statsProvider == null)
        {
            return;
        }

        updateStat(healthBar, statsProvider.Health);

        if (healthBar != null && healthBar.Fill <= 0f)
        {
            // Player is dead, hide other stats
            if (staminaBar != null)
            {
                staminaBar.Fill = 0f;
            }

            if (manaBar != null)
            {
                manaBar.Fill = 0f;
            }

            return;
        }

        updateStat(staminaBar, statsProvider.Stamina);
        updateStat(manaBar, statsProvider.Mana);
    }

    private static void updateStat(ProgressCircleController _controller, UnitStat _stat)
    {
        if (_controller == null)
        {
            return;
        }

        if (_stat.Max <= 0f)
        {
            _controller.Fill = 0f;
            return;
        }

        _controller.Fill = _stat.Current / _stat.Max;
    }
}
