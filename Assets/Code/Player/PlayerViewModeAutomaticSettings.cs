using DL.Structs;
using System.Collections.Generic;
using UnityEngine;
using static PlayerViewModeSettingsBase.ViewModeSettings;

[CreateAssetMenu(fileName = "viewSettings_Automatic", menuName = "DL Examples/Player View Mode Automatic Settings")]
public class PlayerViewModeAutomaticSettings : PlayerViewModeSettingsBase
{
    [SerializeField, Range(1, 64)] private int maxNumberOfSupportedPlayers = 16;

    private Dictionary<int, ViewModeSettings> generatedSettings = new();

    public override int GetNumberOfSupportedPlayers() => maxNumberOfSupportedPlayers;

    public override bool GetViewModeSettings(int _playerCount, out ViewModeSettings _settings)
    {
        if (_playerCount <= 0)
        {
            _settings = null;
            return false;
        }

        if (generatedSettings.ContainsKey(_playerCount) == false)
        {
            generatedSettings.Add(_playerCount, generateView(_playerCount));
        }

        _settings = generatedSettings[_playerCount];
        return true;
    }

    private ViewModeSettings generateView(int _playerCount)
    {
        if (_playerCount <= 1)
        {
            return new ViewModeSettings(new List<ViewAnchors>()
            {
                new ViewAnchors(new MinMax(0f, 1f), new MinMax(0f, 1f))
            });
        }

        int _playersHorizontal = Mathf.CeilToInt(Mathf.Sqrt(_playerCount));
        int _playersVertical = Mathf.CeilToInt(_playerCount / (float)_playersHorizontal);

        float _currentMinVertical = 0f;
        float _verticalDelta = 1f / _playersVertical;
        List<ViewAnchors> _viewAnchors = new List<ViewAnchors>();

        while (_viewAnchors.Count < _playerCount)
        {
            int _playersToAdd = _playerCount - _viewAnchors.Count;

            if (_playersToAdd >= _playersHorizontal)
            {
                _addViewsInLine(_playersHorizontal);
            }
            else if (_playersToAdd > 0)
            {
                _addViewsInLine(_playersToAdd);
            }
        }

        return new ViewModeSettings(_viewAnchors);

        void _addViewsInLine(int _playersInLine)
        {
            if (_playersInLine <= 0)
            {
                return;
            }

            float _horizontalDelta = 1f / _playersInLine;

            for (int i = 0; i < _playersInLine; i++)
            {
                MinMax _verticalRange = new MinMax(_currentMinVertical, _currentMinVertical + _verticalDelta);

                float _minHorizontal = i * _horizontalDelta;
                MinMax _horizontalRange = new MinMax(_minHorizontal, _minHorizontal + _horizontalDelta);

                _viewAnchors.Add(new ViewAnchors(_horizontalRange, _verticalRange));
            }

            _currentMinVertical += _verticalDelta;
        }
    }
}
