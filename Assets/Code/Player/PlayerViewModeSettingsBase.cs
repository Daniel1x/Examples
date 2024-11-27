using DL.Structs;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerViewModeSettingsBase : ScriptableObject
{
    [System.Serializable]
    public class ViewModeSettings
    {
        [System.Serializable]
        public struct ViewAnchors
        {
            [MinMaxSlider(0f, 1f, 32)] public MinMax HorizontalAnchor;
            [MinMaxSlider(0f, 1f, 32)] public MinMax VerticalAnchor;

            public ViewAnchors(MinMax _horizontalAnchor, MinMax _verticalAnchor)
            {
                HorizontalAnchor = _horizontalAnchor;
                VerticalAnchor = _verticalAnchor;
            }
        }

        [SerializeField] private int requiredPlayerCount = 1;
        [SerializeField] private List<ViewAnchors> viewAnchors = new List<ViewAnchors>();

        public ViewModeSettings(List<ViewAnchors> _viewAnchors)
        {
            viewAnchors = _viewAnchors;
            requiredPlayerCount = _viewAnchors.Count;
        }

        public int RequiredPlayerCount => requiredPlayerCount;
        public bool IsSetupValid => requiredPlayerCount == viewAnchors.Count;

        public ViewAnchors GetPlayerAnchors(int _playerID)
        {
            return viewAnchors[_playerID];
        }
    }

    public abstract int GetNumberOfSupportedPlayers();
    public abstract bool GetViewModeSettings(int _playerCount, out ViewModeSettings _settings);
}
