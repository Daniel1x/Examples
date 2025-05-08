using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerSpawner : MonoBehaviour
{
    public static readonly string LEFT_ARROW_SLIM = char.ConvertFromUtf32(0x2190);
    public static readonly string RIGHT_ARROW_SLIM = char.ConvertFromUtf32(0x2192);

    ///<summary>(PreviouslyActive, NewActive)</summary>
    public static event UnityAction<PlayerController, PlayerController> OnActivePlayerChanged = null;

    private static PlayerSpawner instance = null;

    public static PlayerViewModeSettingsBase Settings => instance != null ? instance.settings : null;

    [SerializeField] private PlayerController playerPrefab = null;
    [SerializeField] private PlayerViewModeSettings viewModeSettings = null;
    [SerializeField] private PlayerViewModeAutomaticSettings automaticViewModeSettings = null;
    [SerializeField] private bool useAutomatic = false;
    [SerializeField, Range(1, 9)] private int playersToSpawnAtStart = 1;
    [SerializeField] private bool disableCustomInputs = true;

    private PlayerViewModeSettingsBase settings => useAutomatic ? automaticViewModeSettings : viewModeSettings;

    private List<PlayerController> players = new List<PlayerController>();
    private PlayerController activePlayer = null;

    private Texture2D overlayTexture = null;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < playersToSpawnAtStart; i++)
        {
            spawnPlayer();
        }
    }

    private void Update()
    {
        if (disableCustomInputs)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            spawnPlayer();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            killPlayer();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            setActivePlayer(true);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            setActivePlayer(false);
        }
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (disableCustomInputs)
        {
            return;
        }

        string _rightSideInfo = $"Spawn: N" +
            $"\nKill: K" +
            $"\nNext: {RIGHT_ARROW_SLIM}" +
            $"\nPrevious: {LEFT_ARROW_SLIM}";

        if (overlayTexture == null)
        {
            overlayTexture = RenderTextureExtensions.CreateTexture2D(1, 1, Color.black.WithAlpha(0.5f));
        }

        float _fontSizeMultiplier = Mathf.Min(Screen.width, Screen.height) / 1080f;

        GUIStyle _overlayStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = (40 * _fontSizeMultiplier).RoundToInt(),
            alignment = TextAnchor.MiddleRight,
        };

        _overlayStyle.normal.background = overlayTexture;

        Color _defaultColor = GUI.color;
        GUI.color = Color.white.WithAlpha(0.75f);

        Vector2 _rightSideRectSize = _overlayStyle.CalcSize(new GUIContent(_rightSideInfo));
        const float _cornerOffset = 5f;

        GUI.Label(new Rect(new Vector2(Screen.width - _rightSideRectSize.x - _cornerOffset, _cornerOffset), _rightSideRectSize), _rightSideInfo, _overlayStyle);

        GUI.color = _defaultColor;
    }
#endif

    private void spawnPlayer()
    {
        if (playerPrefab == null)
        {
            return;
        }

        if (settings is PlayerViewModeSettingsBase _settings && players.Count < _settings.GetNumberOfSupportedPlayers())
        {
            players.Add(Instantiate(playerPrefab, transform, false));

            if (activePlayer == null)
            {
                setActivePlayer();
            }
        }
    }

    private void killPlayer()
    {
        if (players.Count <= 1)
        {
            return;
        }

        PlayerController _playerToKill = activePlayer;
        int _id = players.IndexOf(_playerToKill);

        setActivePlayer();

        players.RemoveAt(_id);
        Destroy(_playerToKill.gameObject);
    }

    private void setActivePlayer(bool _next = true)
    {
        if (players.Count <= 0)
        {
            return;
        }

        if (activePlayer == null)
        {
            activePlayer = players[0];
            OnActivePlayerChanged?.Invoke(null, activePlayer);
            return;
        }

        if (players.Count == 1)
        {
            return; //Already active
        }

        int _activePlayerID = players.IndexOf(activePlayer) + (_next ? 1 : -1);
        _activePlayerID = players.GetClampedIndex(_activePlayerID, true);

        PlayerController _newActivePlayer = players[_activePlayerID];

        if (_newActivePlayer != activePlayer)
        {
            PlayerController _previouslyActive = activePlayer;

            activePlayer = _newActivePlayer;

            OnActivePlayerChanged?.Invoke(_previouslyActive, _newActivePlayer);
        }
    }
}
