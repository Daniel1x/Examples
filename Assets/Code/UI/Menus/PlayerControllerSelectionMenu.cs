using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class PlayerControllerSelectionMenu : MonoBehaviour
{
    [System.Serializable]
    public abstract class DeviceOption
    {
        public string OptionName = "";

        public DeviceOption(string _optionName)
        {
            OptionName = _optionName;
        }
    }

    [System.Serializable]
    public class MultipleDevicesOption : DeviceOption
    {
        public List<InputDevice> InputDevices = new List<InputDevice>();

        public MultipleDevicesOption(string _optionName, InputDevice _device = null) : base(_optionName)
        {
            if (_device != null)
            {
                InputDevices.Add(_device);
            }
        }

        public void AddInputDevice(InputDevice _device)
        {
            InputDevices.AddIfNotContains(_device);
        }
    }

    [System.Serializable]
    public class GamepadOption : DeviceOption
    {
        public Gamepad AssignedGamepad { get; private set; } = null;

        public GamepadOption(string _optionName, Gamepad _gamepad = null) : base(_optionName)
        {
            OptionName = _optionName;
            AssignedGamepad = _gamepad;
        }

        public void SetData(string _optionName, Gamepad _inputDevice)
        {
            OptionName = _optionName;
            AssignedGamepad = _inputDevice;
        }
    }

    [Header("Menu Options")]
    [SerializeField] private CustomButton autoAssignButton = null;
    [SerializeField] private BehaviourCollection<PlayerControllerSelector> selectors = new();

    [Header("Controller Options")]
    [SerializeField] private MultipleDevicesOption mouseAndKeyboard = new MultipleDevicesOption("Mouse & Keyboard");
    [SerializeField] private List<GamepadOption> controllerOptions = new List<GamepadOption>();
    [SerializeField] private List<string> controllerDropdownOptions = new();

    private Coroutine updateCoroutine = null;
    private PlayerInputManagerProvider playerInputManagerProvider = null;

    public PlayerInputManagerProvider PlayerInputManagerProvider
    {
        get
        {
            if (playerInputManagerProvider == null)
            {
                playerInputManagerProvider = FindFirstObjectByType<PlayerInputManagerProvider>();
            }

            return playerInputManagerProvider;
        }
    }

    private void Awake()
    {
        if (autoAssignButton != null)
        {
            autoAssignButton.OnItemClicked += onAutoAssignButtonClicked;
        }

        selectors.OnNewItemCreated += onNewSelectorCreated;
        selectors.OnBeforeItemDestroyed += onBeforeSelectorDestroyed;
    }

    private void OnDestroy()
    {
        if (autoAssignButton != null)
        {
            autoAssignButton.OnItemClicked -= onAutoAssignButtonClicked;
        }

        selectors.OnNewItemCreated -= onNewSelectorCreated;
        selectors.OnBeforeItemDestroyed -= onBeforeSelectorDestroyed;
    }

    private void OnEnable()
    {
        InputSystem.onDeviceChange += onDeviceChanged;
        PlayerInputManagerProvider.OnAnyPlayerJoined += onPlayerCountChanged;
        PlayerInputManagerProvider.OnAnyPlayerLeft += onPlayerCountChanged;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= onDeviceChanged;
        PlayerInputManagerProvider.OnAnyPlayerJoined -= onPlayerCountChanged;
        PlayerInputManagerProvider.OnAnyPlayerLeft -= onPlayerCountChanged;
    }

    private void Start()
    {
        UpdateAvailableControllerOptions();
    }

    [ActionButton]
    public void UpdateAvailableControllerOptions()
    {
        ReadOnlyArray<InputDevice> _allInputDevices = InputSystem.devices;
        int _controllerID = 0;

        mouseAndKeyboard.AddInputDevice(Keyboard.current);
        mouseAndKeyboard.AddInputDevice(Mouse.current);

        foreach (InputDevice _device in _allInputDevices)
        {
            switch (_device)
            {
                case Mouse or Keyboard:
                    mouseAndKeyboard.AddInputDevice(_device);
                    break;

                case Gamepad _gamepad:
                    string _optionName = getOptionName(_device, _controllerID);
                    int _placeToSetData = _controllerID;
                    _controllerID++;

                    if (controllerOptions.Count < _placeToSetData + 1)
                    {
                        controllerOptions.Add(new GamepadOption(_optionName, _gamepad));
                    }
                    else
                    {
                        controllerOptions[_placeToSetData].SetData(_optionName, _gamepad);
                    }

                    break;
            }
        }

        if (controllerOptions.Count > _controllerID)
        {
            controllerOptions.RemoveRange(_controllerID, controllerOptions.Count - _controllerID);
        }

        updateDropdownOptions();

        ReadOnlyArray<PlayerInput> _allPlayers = PlayerInput.all;
        ReadOnlyArray<InputDevice> _playerDevices = default;
        selectors.VisibleItems = _allPlayers.Count;

        for (int i = 0; i < _allPlayers.Count; i++)
        {
            PlayerInput _player = _allPlayers[i];
            _playerDevices = _player.devices;

            bool _hasKeyboard = _playerDevices.Any(_device => _device is Keyboard or Mouse);
            int _controllerOption = 0;

            foreach (var _controller in _playerDevices)
            {
                if (_controller is Gamepad _gamepad)
                {
                    _controllerOption = getControllerOption(_gamepad);
                    break;
                }
            }

            selectors[i].SetData(_player, $"Player {_player.playerIndex}", _hasKeyboard, controllerDropdownOptions, _controllerOption);
        }
    }

    private int getControllerOption(Gamepad _gamepad)
    {
        if (_gamepad == null)
        {
            return 0;
        }

        for (int i = 0; i < controllerOptions.Count; i++)
        {
            if (controllerOptions[i].AssignedGamepad == _gamepad)
            {
                return i + 1; //+1 to account for the "None" option at index 0
            }
        }

        return 0;
    }

    private void updateDropdownOptions()
    {
        controllerDropdownOptions.Clear();
        controllerDropdownOptions.Add("None");

        for (int i = 0; i < controllerOptions.Count; i++)
        {
            controllerDropdownOptions.Add(controllerOptions[i].OptionName);
        }
    }

    private void onPlayerCountChanged(PlayerInput _player)
    {
        startUpdateCoroutine();
    }

    private void onDeviceChanged(InputDevice _device, InputDeviceChange _change)
    {
        Debug.Log("Device changed: " + _device.name + " - " + _change.ToString());

        if (_change is InputDeviceChange.Added
            or InputDeviceChange.Reconnected
            or InputDeviceChange.Removed
            or InputDeviceChange.Disconnected)
        {
            startUpdateCoroutine();
        }
    }

    private void stopUpdateCoroutine()
    {
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
            updateCoroutine = null;
        }
    }

    private void startUpdateCoroutine(int _frameCount = 1)
    {
        stopUpdateCoroutine();
        updateCoroutine = StartCoroutine(updateAvailableControllerOptionsAfterFrame(_frameCount));
    }

    private IEnumerator updateAvailableControllerOptionsAfterFrame(int _frameCount = 1)
    {
        for (int i = 0; i < _frameCount; i++)
        {
            yield return null;
        }

        UpdateAvailableControllerOptions();
        stopUpdateCoroutine();
    }

    private string getOptionName(InputDevice _device, int _numberOfValidDevices)
    {
        string _deviceName = _device.displayName;
        string _optionName;
        int _deviceID = 0;

        do
        {
            _optionName = _deviceName + " " + _deviceID;
            _deviceID++;
        }
        while (_doesThisOptionExist(_optionName));

        return _optionName;

        bool _doesThisOptionExist(string _name)
        {
            for (int i = 0; i < _numberOfValidDevices && i < controllerOptions.Count; i++)
            {
                if (controllerOptions[i].OptionName == _name)
                {
                    return true;
                }
            }

            return false;
        }
    }

    private void onNewSelectorCreated(PlayerControllerSelector _selector, int _id)
    {
        _selector.OnControllerOptionChanged += onControllerOptionChanged;
    }

    private void onBeforeSelectorDestroyed(PlayerControllerSelector _selector, int _id)
    {
        _selector.OnControllerOptionChanged -= onControllerOptionChanged;
    }

    private void onControllerOptionChanged(PlayerControllerSelector _selector, int _id)
    {
        _selector.AssignedPlayer.DebugCurrentDevices($"Changed option : {_id}");

        if (_id == 0) //None
        {
            bool _cleared = PlayerInputManagerProvider.SetPlayerGamepad(_selector.AssignedPlayer, null);

            if (_cleared)
            {
                startUpdateCoroutine();
            }

            return;
        }

        _id--; //Remove the None option

        if (controllerOptions.IsIndexOutOfRange(_id))
        {
            return;
        }

        bool _set = PlayerInputManagerProvider.SetPlayerGamepad(_selector.AssignedPlayer, controllerOptions[_id].AssignedGamepad);

        if (_set)
        {
            startUpdateCoroutine();
        }
    }

    private void onAutoAssignButtonClicked(int _id, CustomButton _button)
    {
        if (PlayerInputManagerProvider != null)
        {
            while (PlayerInputManagerProvider.JoinPlayerWithFreeController(null))
            {
                //Keep joining players until there are no free controllers
            }
        }
    }
}
