using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

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
    [SerializeField] private CustomButton startButton = null;
    [SerializeField] private BehaviourCollection<PlayerControllerSelector> selectors = new();

    [Header("Player Characters")]
    [SerializeField] private GameObject playerPrefab = null;
    [SerializeField] private Transform playerSpawnPoint = null;

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

        if (startButton != null)
        {
            startButton.OnItemClicked += onStartButtonClicked;
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

        if (startButton != null)
        {
            startButton.OnItemClicked -= onStartButtonClicked;
        }

        selectors.OnNewItemCreated -= onNewSelectorCreated;
        selectors.OnBeforeItemDestroyed -= onBeforeSelectorDestroyed;
    }

    private void OnEnable()
    {
        InputSystem.onDeviceChange += onDeviceChanged;
        PlayerInputManagerProvider.OnAnyPlayerJoined += onPlayerCountChanged;
        PlayerInputManagerProvider.OnAnyPlayerLeft += onPlayerCountChanged;

        if (autoAssignButton != null)
        {
            autoAssignButton.Select();
        }
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

        updateStartButtonState();
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
        _selector.OnActionButtonPressed += onActionButtonPressed;
    }

    private void onBeforeSelectorDestroyed(PlayerControllerSelector _selector, int _id)
    {
        _selector.OnControllerOptionChanged -= onControllerOptionChanged;
        _selector.OnActionButtonPressed -= onActionButtonPressed;
    }

    private void onActionButtonPressed(PlayerControllerSelector _selector, CustomButton _button)
    {
        if (_selector.AssignedPlayer == null)
        {
            return;
        }

        Selectable _nextToSelect = _button.FindSelectableOnUp();

        Destroy(_selector.AssignedPlayer.gameObject);

        if (_nextToSelect != null)
        {
            _nextToSelect.Select();
        }
    }

    private void onControllerOptionChanged(PlayerControllerSelector _selector, int _id)
    {
        if (_id == 0) //None
        {
            PlayerInputManagerProvider.SetPlayerGamepad(_selector.AssignedPlayer, null);
        }
        else
        {
            _id--; //Remove the None option

            if (controllerOptions.IsIndexOutOfRange(_id) == false)
            {
                PlayerInputManagerProvider.SetPlayerGamepad(_selector.AssignedPlayer, controllerOptions[_id].AssignedGamepad);
            }
        }

        startUpdateCoroutine(); //Refresh dropdown options
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

    private void onStartButtonClicked(int _id, CustomButton _button)
    {
        if (_button.interactable == false)
        {
            return;
        }

        if (allPlayersHasAssignedDevices())
        {
            gameObject.SetActive(false); //Hide the menu, spawn characters and start game

            ReadOnlyArray<PlayerInput> _allPlayers = PlayerInput.all;

            foreach (var _player in _allPlayers)
            {
                if (_player == null)
                {
                    continue;
                }

                if (_player.playerIndex == 0 && _player.devices.Any(_dev => _dev is Gamepad))
                {
                    _player.SwitchCurrentControlScheme("Gamepad", _player.devices.ToArray()); //Make sure the first player is using the gamepad scheme
                }

                GameObject _newCharacter = Instantiate(playerPrefab, playerSpawnPoint);

                if (_newCharacter != null && _player.GetComponent<PlayerInputInstance>() is PlayerInputInstance _inputs)
                {
                    if (_newCharacter.GetComponent<ThirdPersonController>() is ThirdPersonController _controller)
                    {
                        _controller.BasicInput = _inputs.PlayerBasicInputs;
                    }

                    if (_newCharacter.GetComponentInChildren<PlayerIndicator>(true) is PlayerIndicator _indicator)
                    {
                        _indicator.Player = _player;
                    }
                }
            }
        }
    }

    private void updateStartButtonState()
    {
        if (startButton == null)
        {
            return;
        }

        startButton.interactable = allPlayersHasAssignedDevices();
        startButton.transform.SetAsLastSibling();

        if (startButton.interactable == false && startButton.IsSelected)
        {
            startButton.FindSelectableOnUp()?.Select();
        }
    }

    private bool allPlayersHasAssignedDevices()
    {
        ReadOnlyArray<PlayerInput> _allPlayers = PlayerInput.all;

        if (_allPlayers.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < _allPlayers.Count; i++)
        {
            if (_allPlayers[i].devices.Count == 0)
            {
                return false;
            }
        }

        return true;
    }
}
