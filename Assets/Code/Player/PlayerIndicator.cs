using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerIndicator : MonoBehaviour, IPlayerColorProvider
{
    [Header("References")]
    [SerializeField] private TMP_Text textField = null;
    [SerializeField] private MaskableGraphic[] graphics = new MaskableGraphic[] { };

    [Header("Settings")]
    [SerializeField] private string playerName = "Player";
    [SerializeField, Range(0f, 1f)] private float colorAlpha = 1f;
    [SerializeField] private Color[] colors = new[] { Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta };

    private PlayerInput playerInput = null;

    public PlayerInput Player
    {
        get => playerInput;
        set
        {
            playerInput = value;

            if (playerInput != null)
            {
                PlayerColor = colors.IsIndexOutOfRange(playerInput.playerIndex)
                    ? ColorExtensions.RandomColor(colorAlpha)
                    : colors[playerInput.playerIndex].WithAlpha(colorAlpha);
            }
        }
    }

    private CameraTargetProvider cameraTargetProvider = null;

    private Color playerColor = Color.white;
    public Color PlayerColor
    {
        get => playerColor;
        set
        {
            playerColor = value;
            UpdateIndicator();
        }
    }

    private void Awake()
    {
        playerInput = this.GetComponentHereOrInParent<PlayerInput>();
        cameraTargetProvider = this.GetComponentHereOrInParent<CameraTargetProvider>();
    }

    private void LateUpdate()
    {
        if (cameraTargetProvider != null && cameraTargetProvider.VirtualCamera != null)
        {
            Vector3 _camForward = cameraTargetProvider.VirtualCamera.transform.forward;
            _camForward.y = 0f;

            transform.rotation = Quaternion.LookRotation(Vector3.down, _camForward);
        }
    }

    public void UpdateIndicator()
    {
        if (playerInput == null)
        {
            return;
        }

        int _playerIndex = playerInput != null
            ? playerInput.playerIndex
            : -1;

        if (textField != null)
        {
            textField.text = _playerIndex >= 0
                ? $"{playerName} {_playerIndex}"
                : $"{playerName}";
        }

        if (graphics != null)
        {
            foreach (MaskableGraphic _graphic in graphics)
            {
                if (_graphic != null)
                {
                    _graphic.color = PlayerColor;
                }
            }
        }
    }
}
