using System.Collections.Generic;
using UnityEngine;

public class PlayerCanvasManager : MonoBehaviour
{
    [System.Serializable]
    public class PlayerData
    {
        public PlayerController Controller = null;
        public PlayerView View = null;

        public PlayerData(PlayerController _controller, RenderTexture _texture, PlayerView _view)
        {
            Controller = _controller;
            View = _view;
            View.TextureTarget.texture = _texture;
        }
    }

    [SerializeField] private Transform parent = null;
    [SerializeField] private PlayerView playerViewPrefab = null;
    [SerializeField] private List<PlayerData> playerDatas = new List<PlayerData>();

    private ScreenResolutionChecker resolutionChecker = new ScreenResolutionChecker();
    private RectTransform rectTransform = null;

    private void Awake()
    {
        rectTransform = (RectTransform)transform;

        PlayerController.OnNewPlayerSpawned += onNewSpawned;
        PlayerController.OnPlayerDestroyed += onDestroyed;
        PlayerSpawner.OnActivePlayerChanged += onActivePlayerChanged;
    }

    private void OnDestroy()
    {
        PlayerController.OnNewPlayerSpawned -= onNewSpawned;
        PlayerController.OnPlayerDestroyed -= onDestroyed;
        PlayerSpawner.OnActivePlayerChanged -= onActivePlayerChanged;
    }

    private void Start()
    {
        checkResolutionAndAdjust();
    }

    private void Update()
    {
        checkResolutionAndAdjust();
    }

    private void checkResolutionAndAdjust()
    {
        if (resolutionChecker.CheckForRectChange(rectTransform))
        {
            adjustPlayerScreens();
        }
    }

    private void adjustPlayerScreens()
    {
        int _count = playerDatas.Count;

        if (_count == 0)
        {
            return;
        }

        if (PlayerSpawner.Settings is not PlayerViewModeSettingsBase _viewModeSettings)
        {
            return;
        }

        bool _valid = _viewModeSettings.GetViewModeSettings(_count, out PlayerViewModeSettings.ViewModeSettings _settings);

        if (_valid == false)
        {
            return;
        }

        Vector2Int _availablePixels = resolutionChecker.Resolution.Size;

        for (int i = 0; i < playerDatas.Count; i++)
        {
            PlayerViewModeSettings.ViewModeSettings.ViewAnchors _anchorSettings = _settings.GetPlayerAnchors(i);

            Vector2Int _rtSize = new Vector2Int
            {
                x = (_availablePixels.x * _anchorSettings.HorizontalAnchor.Range).RoundToInt(),
                y = (_availablePixels.y * _anchorSettings.VerticalAnchor.Range).RoundToInt()
            };

            playerDatas[i].View.TextureTarget.texture = playerDatas[i].Controller.UpdateRenderTextureSize(_rtSize);
            playerDatas[i].View.RectTransform.SetAnchorsXY(_anchorSettings.HorizontalAnchor, _anchorSettings.VerticalAnchor);
        }
    }

    private void onNewSpawned(PlayerController _controller, RenderTexture _texture)
    {
        playerDatas.Add(new PlayerData(_controller, _texture, Instantiate(playerViewPrefab, parent, false)));
        adjustPlayerScreens();
    }

    private void onDestroyed(PlayerController _controller, RenderTexture _texture)
    {
        for (int i = 0; i < playerDatas.Count; i++)
        {
            PlayerData _data = playerDatas[i];

            if (_data.Controller == null
                || _data.Controller == _controller)
            {
                Destroy(_data.View.gameObject);
                playerDatas.RemoveAt(i);
                i--;
            }
        }

        adjustPlayerScreens();
    }

    private void onActivePlayerChanged(PlayerController _previousActive, PlayerController _newActive)
    {
        if (getPlayerData(_previousActive) is PlayerData _previousActiveData)
        {
            _previousActiveData.View.SetAsSelected(false);
        }

        if (getPlayerData(_newActive) is PlayerData _newActiveData)
        {
            _newActiveData.View.SetAsSelected(true);
        }
    }

    private PlayerData getPlayerData(PlayerController _controller)
    {
        if (_controller == null)
        {
            return null;
        }

        for (int i = 0; i < playerDatas.Count; i++)
        {
            if (playerDatas[i].Controller == _controller)
            {
                return playerDatas[i];
            }
        }

        return null;
    }
}
