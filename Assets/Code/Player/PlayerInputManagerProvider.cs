using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

[RequireComponent(typeof(PlayerInputManager))]
public class PlayerInputManagerProvider : MonoBehaviour
{
    [System.Serializable]
    public class PlayerData
    {
        public event UnityAction<PlayerData> OnPlayerDestroyed = null;

        [SerializeField] private PlayerInput playerInput = null;
        [SerializeField] private PlayerInputInstance inputInstance = null;

        private MonoBehaviourEventCaller eventCaller = null;

        public PlayerInput PlayerInput => playerInput;
        public PlayerInputInstance InputInstance => inputInstance;
        public int ID => PlayerInput != null ? PlayerInput.playerIndex : -1;

        public PlayerData(PlayerInput _playerInput)
        {
            if (_playerInput == null)
            {
                return;
            }

            playerInput = _playerInput;
            inputInstance = _playerInput.gameObject.GetOrAddComponent<PlayerInputInstance>(out _);

            eventCaller = _playerInput.gameObject.GetOrAddComponent<MonoBehaviourEventCaller>(out _);
            eventCaller.OnBehaviourDestroyed += onPlayerDestroyed;
        }

        private void onPlayerDestroyed(MonoBehaviourEventCaller _caller)
        {
            if (_caller != null)
            {
                _caller.OnBehaviourDestroyed -= onPlayerDestroyed;
            }

            OnPlayerDestroyed?.Invoke(this);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Init()
    {
        OnAnyPlayerJoined = null;
        OnAnyPlayerLeft = null;

        validInputDevicesForPlayer.Clear();
        allPlayers = default;
        allDevices = default;
    }

    public static event UnityAction<PlayerInput> OnAnyPlayerJoined = null;
    public static event UnityAction<PlayerInput> OnAnyPlayerLeft = null;

    //Temporary lists to store input devices for validation
    private static List<InputDevice> validInputDevicesForPlayer = new();
    private static ReadOnlyArray<PlayerInput> allPlayers = default;
    private static ReadOnlyArray<InputDevice> allDevices = default;

    [Header("Join Player Settings")]
    [SerializeField] private bool createFirstPlayerAtStart = true;
    [SerializeField] private bool useJoinAction = true;
    [SerializeField] private InputAction joinAction = new InputAction();

    [Header("Player Data")]
    [SerializeField, ReadOnlyProperty] private List<PlayerData> playerDataList = new();

    private PlayerInputManager playerInputManager = null;

    public bool UseJoinAction
    {
        get => useJoinAction;
        set
        {
            useJoinAction = value;

            if (useJoinAction)
            {
                joinAction.Enable();
            }
            else
            {
                joinAction.Disable();
            }
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            UseJoinAction = useJoinAction; //Refresh the action if it was changed in the inspector
        }
    }

    private void Awake()
    {
        playerInputManager = GetComponent<PlayerInputManager>();

        if (playerInputManager != null)
        {
            playerInputManager.onPlayerJoined += OnPlayerJoined;
            playerInputManager.onPlayerLeft += OnPlayerLeft;
        }

        UseJoinAction = useJoinAction; //Enable or disable the join action based on the inspector value
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

    [ActionButton]
    public void JoinPlayerWithFreeController() => JoinPlayerWithFreeController(null);

    public bool JoinPlayerWithFreeController(InputDevice _prioritizedDevice)
    {
        if (playerInputManager == null)
        {
            return false;
        }

        InputDevice[] _devicesToAssign = getValidDevicesToAssign(_prioritizedDevice);

        if (_devicesToAssign.IsNullOrEmpty())
        {
            Debug.LogWarning("No free devices available to join a new player.");
            return false; //No free devices available
        }

        PlayerInput _newPlayer = playerInputManager.JoinPlayer(-1, -1, null, _devicesToAssign);

        if (_newPlayer == null)
        {
            return false; //Failed to join player
        }

        registerNewPlayer(_newPlayer);

        _newPlayer.transform.parent = transform; //Set the player as a child of this object

        validatePlayerInputDevices(_newPlayer);
        return true;
    }

    private void registerNewPlayer(PlayerInput _player)
    {
        if (_player == null)
        {
            return;
        }

        PlayerData _newPlayerData = new PlayerData(_player);
        playerDataList.Add(_newPlayerData);
        sortPlayerDataByIndex();

        MyLog.Log($"Player added! Player Index: {_player.playerIndex}");

        _newPlayerData.OnPlayerDestroyed += onPlayerDestroyed;
    }

    private void unregisterPlayer(PlayerInput _player)
    {
        for (int i = playerDataList.Count - 1; i >= 0; i--)
        {
            if (playerDataList[i].PlayerInput == null
                || playerDataList[i].PlayerInput == _player)
            {
                playerDataList.RemoveAt(i);
                MyLog.Log($"Player removed! ID: {i}");
            }
        }

        sortPlayerDataByIndex();
    }

    private void sortPlayerDataByIndex()
    {
        if (playerDataList.Count <= 1)
        {
            return;
        }

        playerDataList.Sort((_p1, _p2) => _p1.ID.CompareTo(_p2.ID));
    }

    private void onPlayerDestroyed(PlayerData _player)
    {
        unregisterPlayer(_player.PlayerInput);
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
            return; //Invalid device
        }

        if (PlayerInput.all.Any(_player => _player.devices.Contains(_triggeringDevice)))
        {
            return; //Already used
        }

        Debug.Log($"Join action performed by device: {_triggeringDevice.name}");

        JoinPlayerWithFreeController(_triggeringDevice);
    }

    public static bool SetPlayerGamepad(PlayerInput _player, Gamepad _selectedGamepad = null)
    {
        _player.DebugCurrentDevices($"Set Gamepad : {_selectedGamepad?.name}");

        if (_player == null)
        {
            return false;
        }

        if (_selectedGamepad != null)
        {
            return ReplacePlayerGamepad(_player, _selectedGamepad, true);
        }

        //Null gamepad can be set to player 0 because he can use keyboard and mouse
        if (_player.playerIndex != 0)
        {
            return false; //Invalid gamepad
        }

        ReadOnlyArray<InputDevice> _playerDevices = _player.devices;
        List<InputDevice> _mouseAndKeyboardDevices = new();

        foreach (InputDevice _device in _playerDevices)
        {
            if (isMouseOrKeyboard(_device))
            {
                _mouseAndKeyboardDevices.Add(_device);
            }
        }

        if (anyDeviceModified(in _playerDevices, _mouseAndKeyboardDevices))
        {
            _player.SwitchCurrentControlScheme(_mouseAndKeyboardDevices.ToArray());
            validatePlayerInputDevices(_player);

            return true;
        }

        return false;
    }

    public static bool ReplacePlayerGamepad(PlayerInput _player, Gamepad _gamepad, bool _findFreeGamepadForPreviousOwner = true)
    {
        if (_player == null || _gamepad == null)
        {
            return false;
        }

        _player.DebugCurrentDevices($"Replace Gamepad : {_gamepad?.name}");

        PlayerInput _previousOwner = findDeviceOwner(_gamepad);

        if (_previousOwner == _player)
        {
            return false; //This player already owns the gamepad
        }

        if (_previousOwner != null)
        {
            //Remove gamepad from the previous player
            RemoveDeviceFromPlayer(_previousOwner, _gamepad);
        }

        ReadOnlyArray<InputDevice> _playerDevices = _player.devices;
        List<InputDevice> _validDevices = _player.devices.ToList();

        foreach (InputDevice _device in _playerDevices)
        {
            if (_device != null && _device is not Gamepad)
            {
                _validDevices.Add(_device); //Add all devices that are not gamepads
            }
        }

        _validDevices.Add(_gamepad); //Add the new gamepad
        bool _devicesModified = false;

        if (anyDeviceModified(in _playerDevices, _validDevices))
        {
            _player.SwitchCurrentControlScheme(_playerDevices.ToArray());
            validatePlayerInputDevices(_player);
            _devicesModified = true;

            _player.DebugCurrentDevices("Gamepad set to new owner");
        }

        if (_findFreeGamepadForPreviousOwner && _previousOwner != null)
        {
            ReadOnlyArray<InputDevice> _previousOwnerDevices = _previousOwner.devices;
            List<InputDevice> _previousOwnerValidDevices = new List<InputDevice>();
            bool _alreadyHasGamepad = false;

            foreach (InputDevice _device in _previousOwnerDevices)
            {
                if (_device is Gamepad)
                {
                    _alreadyHasGamepad = true; //Previous owner already has a gamepad
                }
                else
                {
                    _previousOwnerValidDevices.Add(_device); //Add all devices that are not gamepads
                }
            }

            if (_alreadyHasGamepad == false)
            {
                Gamepad _freeGamepad = findFreeGamepad();

                if (_freeGamepad != null)
                {
                    _previousOwnerValidDevices.Add(_freeGamepad); //Add the new gamepad
                }
            }

            if (anyDeviceModified(in _previousOwnerDevices, _previousOwnerValidDevices))
            {
                _previousOwner.SwitchCurrentControlScheme(_previousOwnerValidDevices.ToArray());
                validatePlayerInputDevices(_previousOwner);
                _devicesModified = true;

                _previousOwner.DebugCurrentDevices("New free device for previous owner");
            }
        }

        return _devicesModified;
    }

    private static Gamepad findFreeGamepad()
    {
        allPlayers = PlayerInput.all;
        List<InputDevice> _usedDevices = new();

        for (int i = 0; i < allPlayers.Count; i++)
        {
            if (allPlayers[i] != null)
            {
                _usedDevices.AddRange(allPlayers[i].devices);
            }
        }

        allDevices = InputSystem.devices;

        foreach (InputDevice _device in allDevices)
        {
            if (_device != null
                && _device is Gamepad _freeGamepad
                && _usedDevices.Contains(_device) == false)
            {
                return _freeGamepad; //Return the first free gamepad
            }
        }

        return null; //No free gamepad found
    }

    public static bool RemoveDeviceFromPlayer(PlayerInput _player, InputDevice _device)
    {
        if (_player == null || _device == null)
        {
            return false;
        }

        _player.DebugCurrentDevices("Removing device");

        List<InputDevice> _playerDevices = _player.devices.ToList();

        if (_playerDevices.Contains(_device))
        {
            _playerDevices.Remove(_device);
            _player.SwitchCurrentControlScheme(_playerDevices.ToArray());

            validatePlayerInputDevices(_player);

            _player.DebugCurrentDevices("Device removed");

            return true;
        }

        return false;
    }

    private static PlayerInput findDeviceOwner<T>(T _device) where T : InputDevice
    {
        allPlayers = PlayerInput.all;

        foreach (PlayerInput _player in allPlayers)
        {
            if (_player != null && _player.devices.Contains(_device))
            {
                return _player; //Found the player that owns the device
            }
        }

        return null; //No player owns the device
    }

    private static bool isMouseOrKeyboard(InputDevice _device)
    {
        return _device is Mouse or Keyboard;
    }

    private static void validatePlayerInputDevices(PlayerInput _player)
    {
        if (_player == null)
        {
            return;
        }

        validInputDevicesForPlayer.Clear();

        if (_player.playerIndex == 0) //All keyboard and mouse devices for first player
        {
            validInputDevicesForPlayer.AddRange(InputSystem.devices.Where(isMouseOrKeyboard));
        }

        ReadOnlyArray<InputDevice> _playerDevices = _player.devices;

        foreach (InputDevice _device in _playerDevices)
        {
            if (_device is Gamepad)
            {
                validInputDevicesForPlayer.Add(_device);
                break; //Only one gamepad per player
            }
        }

        if (anyDeviceModified(in _playerDevices, validInputDevicesForPlayer))
        {
            _player.SwitchCurrentControlScheme(validInputDevicesForPlayer.ToArray());
        }
    }

    private static bool anyDeviceModified(in ReadOnlyArray<InputDevice> _playerDevices, List<InputDevice> _newDevices)
    {
        if (_playerDevices.Count != _newDevices.Count)
        {
            return true; //Different amount of devices
        }

        foreach (InputDevice _currentDevice in _playerDevices)
        {
            if (_newDevices.Contains(_currentDevice) == false)
            {
                return true; //Different devices
            }
        }

        return false; //Same amount and same devices
    }

    private static InputDevice[] getValidDevicesToAssign(InputDevice _prioritizedDevice = null)
    {
        allPlayers = PlayerInput.all;
        List<InputDevice> _usedDevices = new();

        foreach (PlayerInput _player in allPlayers)
        {
            if (_player != null)
            {
                validatePlayerInputDevices(_player); //Validate and release devices if needed
                _usedDevices.AddRange(_player.devices);
            }
        }

        allDevices = InputSystem.devices;
        List<InputDevice> _freeDevices = new();

        foreach (InputDevice _device in allDevices)
        {
            if (_device != null && _usedDevices.Contains(_device) == false)
            {
                _freeDevices.Add(_device);
            }
        }

        if (_prioritizedDevice != null && _freeDevices.Contains(_prioritizedDevice))
        {
            return new InputDevice[] { _prioritizedDevice }; //Only use the prioritized device if it is free
        }

        foreach (InputDevice _device in _freeDevices)
        {
            //Use first gamepad
            if (_device is Gamepad)
            {
                return new InputDevice[] { _device };
            }
        }

        foreach (InputDevice _device in _freeDevices)
        {
            //Use first free device, gamepad were already checked
            if (isMouseOrKeyboard(_device))
            {
                return new InputDevice[] { _device };
            }
        }

        return null;
    }
}

public static class PlayerInputManagerExtensions
{
    public static void DebugCurrentDevices(this PlayerInput _player, string _additionalInfo = null)
    {
        if (_player != null)
        {
            Debug.Log($"{_additionalInfo} :: Current: {_playerInfo(_player)}\n{PlayerInput.all.ToStringByElements(_playerInfo, "\n")}");
        }

        string _playerInfo(PlayerInput _player)
        {
            if (_player == null)
            {
                return "Null";
            }

            return $"Player: {_player.playerIndex} :: Devices: {_player.devices.ToStringByElements(_e => $"{_e.name}", _printIDs: true)}";
        }
    }
}
