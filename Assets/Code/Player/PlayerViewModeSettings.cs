using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "viewSettings_PlayerDefault", menuName = "DL Examples/Player View Mode Settings")]
public class PlayerViewModeSettings : PlayerViewModeSettingsBase
{
    [SerializeField] private List<ViewModeSettings> viewModes = new List<ViewModeSettings>();

    public override int GetNumberOfSupportedPlayers()
    {
        int _max = 0;

        foreach (var _mode in viewModes)
        {
            if (_mode.RequiredPlayerCount > _max)
            {
                _max = _mode.RequiredPlayerCount;
            }
        }

        return _max;
    }

    public override bool GetViewModeSettings(int _playerCount, out ViewModeSettings _settings)
    {
        for (int i = 0; i < viewModes.Count; i++)
        {
            if (viewModes[i].RequiredPlayerCount == _playerCount
                && viewModes[i].IsSetupValid)
            {
                _settings = viewModes[i];
                return true;
            }
        }

        _settings = null;
        return false;
    }
}
