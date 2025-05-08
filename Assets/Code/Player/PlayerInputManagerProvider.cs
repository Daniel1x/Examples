using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

[RequireComponent(typeof(PlayerInputManager))]
public class PlayerInputManagerProvider : MonoBehaviour
{
    public static event UnityAction<PlayerInput> OnAnyPlayerJoined = null;
    public static event UnityAction<PlayerInput> OnAnyPlayerLeft = null;

    public static PlayerInputManager ActiveInstance = null;

    private PlayerInputManager playerInputManager = null;

    [Header("Join Player Settings")]
    [SerializeField] private bool createFirstPlayerAtStart = true;
    [SerializeField] private InputAction joinAction = new InputAction();

    private void Awake()
    {
        playerInputManager = GetComponent<PlayerInputManager>();

        if (playerInputManager != null)
        {
            playerInputManager.onPlayerJoined += OnPlayerJoined;
            playerInputManager.onPlayerLeft += OnPlayerLeft;
        }

        joinAction.Enable();
        joinAction.performed += joinActionPerformed;
    }

    private void OnDestroy()
    {
        if (playerInputManager != null)
        {
            playerInputManager.onPlayerJoined -= OnPlayerJoined;
            playerInputManager.onPlayerLeft -= OnPlayerLeft;
        }

        joinAction.performed -= joinActionPerformed;
        joinAction.Disable();
    }

    private void Start()
    {
        if (createFirstPlayerAtStart && PlayerInput.all.Count <= 0)
        {
            JoinPlayerWithFreeController();
        }
    }

    private void OnEnable()
    {
        ActiveInstance = playerInputManager;
    }

    private void OnDisable()
    {
        if (ActiveInstance == playerInputManager)
        {
            ActiveInstance = null;
        }
    }

    [ActionButton]
    public void JoinPlayerWithFreeController() => JoinPlayerWithFreeController(null);

    public void JoinPlayerWithFreeController(params InputDevice[] _prioritizedDevices)
    {
        if (playerInputManager == null)
        {
            Debug.LogWarning("PlayerInputManager is not assigned.");
            return;
        }

        // Pobierz wszystkie urzπdzenia typu Gamepad
        List<InputDevice> _allDevices = InputSystem.devices.ToList();

        Debug.Log($"All devices: {_allDevices.ToStringByElements(_e => _e.displayName)}");

        // Pobierz urzπdzenia juø przypisane do graczy
        List<InputDevice> _used = new List<InputDevice>();
        ReadOnlyArray<PlayerInput> _allPlayers = PlayerInput.all;

        foreach (var _player in _allPlayers)
        {
            if (_player.devices.Count <= 0)
            {
                continue;
            }

            _validatePlayerInputDevices(_player);

            _used.AddRange(_player.devices);
        }

        Debug.Log($"Used devices: {_used.ToStringByElements(_e => _e.displayName)}");

        // Znajdü pierwszy nieuøywany gamepad
        List<InputDevice> _freeDevices = _allDevices.Where(_device => _used.Contains(_device) == false).ToList();

        InputDevice[] _devicesToAssign = _getValidDevicesToAssign();

        if (_devicesToAssign.IsNullOrEmpty() == false)
        {
            // UtwÛrz nowego gracza z nieuøywanym gamepadem
            var _newPlayer = playerInputManager.JoinPlayer(-1, -1, null, _devicesToAssign);

            if (_newPlayer != null)
            {
                Debug.Log($"New player joined with free gamepad: {_devicesToAssign.ToStringByElements(_e => _e.displayName)} :: Assigned devices: {_newPlayer.devices.ToStringByElements(_e => _e.displayName)}");

                _validatePlayerInputDevices(_newPlayer);
            }
            else
            {
                Debug.Log($"Player was not created!");
            }
        }
        else
        {
            Debug.LogWarning("No free gamepads available to join a new player.");
        }

        InputDevice[] _getValidDevicesToAssign()
        {
            if (_prioritizedDevices.IsNullOrEmpty() == false)
            {
                List<InputDevice> _validPrioritizedDevices = _prioritizedDevices.Where(_device => _freeDevices.Contains(_device)).ToList();

                if (_validPrioritizedDevices.IsNullOrEmpty() == false)
                {
                    bool _containsMouse = _validPrioritizedDevices.Any(_device => _device is Mouse);
                    bool _containsKeyboard = _validPrioritizedDevices.Any(_device => _device is Keyboard);

                    if (_containsMouse ^ _containsKeyboard)
                    {
                        //If there is only one of them, find the other
                        foreach (InputDevice _device in _freeDevices)
                        {
                            if (_device is Mouse or Keyboard)
                            {
                                _validPrioritizedDevices.AddIfNotContains(_device);
                            }
                        }

                        return _validPrioritizedDevices.ToArray();
                    }

                    return _validPrioritizedDevices.ToArray();
                }
            }

            List<InputDevice> _validDevices = new List<InputDevice>();

            foreach (InputDevice _device in _freeDevices)
            {
                if (_device is Mouse or Keyboard)
                {
                    _validDevices.Add(_device);
                }
            }

            //Use mouse and keyboard
            if (_validDevices.Count > 0)
            {
                return _validDevices.ToArray();
            }

            foreach (InputDevice _device in _freeDevices)
            {
                //Use gamepad
                if (_device is Gamepad)
                {
                    return new InputDevice[] { _device };
                }
            }

            //Use any free device
            foreach (InputDevice _device in _freeDevices)
            {
                if (_device != null)
                {
                    return new InputDevice[] { _device };
                }
            }

            return null;
        }

        void _validatePlayerInputDevices(PlayerInput _player)
        {
            if (_player.devices.Count <= 1)
            {
                return;
            }

            Debug.LogWarning($"Player {_player.playerIndex} has more than one device assigned. Only the first one will be kept.");

            List<InputDevice> _mouseOrKeyboard = new List<InputDevice>();
            List<InputDevice> _gamepads = new List<InputDevice>();

            foreach (var _device in _player.devices)
            {
                switch (_device)
                {
                    case Mouse or Keyboard:
                        _mouseOrKeyboard.Add(_device);
                        break;
                    case Gamepad:
                        _gamepads.Add(_device);
                        break;
                }
            }

            InputDevice[] _controls = _mouseOrKeyboard.Count > 0
                ? _mouseOrKeyboard.ToArray()
                : _gamepads.Count <= 0 ? _gamepads.ToArray() : new InputDevice[] { _gamepads[0] };

            bool _switched = _player.SwitchCurrentControlScheme(_controls.ToArray());

            Debug.Log($"Tried to switch current controls! Switched:{_switched} :: New:{_controls.ToStringByElements(_e => _e.displayName)} :: Assigned:{_player.devices.ToStringByElements(_e => _e.displayName)}");
        }
    }

    public void OnPlayerJoined(PlayerInput _input)
    {
        OnAnyPlayerJoined?.Invoke(_input);
    }

    public void OnPlayerLeft(PlayerInput _input)
    {
        OnAnyPlayerLeft?.Invoke(_input);
    }

    private void joinActionPerformed(InputAction.CallbackContext _context)
    {
        InputDevice _triggeringDevice = _context.control?.device;

        if (_triggeringDevice == null)
        {
            Debug.LogWarning("JoinAction was triggered, but no device was found.");
            return;
        }

        Debug.Log($"Join action performed by device: {_triggeringDevice.displayName}");

        // Sprawdü, czy urzπdzenie jest juø przypisane do gracza
        if (PlayerInput.all.Any(_player => _player.devices.Contains(_triggeringDevice)))
        {
            Debug.LogWarning($"Device {_triggeringDevice.displayName} is already assigned to a player.");
            return;
        }

        JoinPlayerWithFreeController(_triggeringDevice);
    }
}
