using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerControllerSelector : MonoBehaviour
{
    public event UnityAction<PlayerControllerSelector> OnActionButtonPressed = null;
    public event UnityAction<PlayerControllerSelector, bool> OnUseKeyboardChanged = null;
    public event UnityAction<PlayerControllerSelector, int> OnControllerOptionChanged = null;

    [SerializeField] private TMP_Text playerNameText = null;
    [SerializeField] private CustomButton actionButton = null;
    [SerializeField] private CustomToggle keyboardToggle = null;
    [SerializeField] private CustomDropdown controllerDropdown = null;
    [SerializeField] private DropdownOptionsHandler controllerDropdownOptionsHandler = null;

    private List<string> controllerOptions = null;

    public PlayerInput AssignedPlayer { get; private set; } = null;

    private void Awake()
    {
        if (actionButton != null)
        {
            actionButton.OnItemClicked += onActionButtonClicked;
        }

        if (keyboardToggle != null)
        {
            keyboardToggle.interactable = false;
            keyboardToggle.OnItemValueChanged += onKeyboardToggleChanged;
        }

        if (controllerDropdownOptionsHandler != null)
        {
            controllerDropdownOptionsHandler.OnDropdownValueChanged += onControllerDropdownHandlerValueChanged;
        }
    }

    private void OnDestroy()
    {
        if (actionButton != null)
        {
            actionButton.OnItemClicked -= onActionButtonClicked;
        }

        if (keyboardToggle != null)
        {
            keyboardToggle.OnItemValueChanged -= onKeyboardToggleChanged;
        }

        if (controllerDropdownOptionsHandler != null)
        {
            controllerDropdownOptionsHandler.OnDropdownValueChanged -= onControllerDropdownHandlerValueChanged;
        }
    }

    public void SetData(PlayerInput _player, string _playerName, bool _hasKeyboard, List<string> _availableControllerOptions, int _currentControllerID = 0)
    {
        AssignedPlayer = _player;

        if (playerNameText != null)
        {
            playerNameText.text = _playerName;
        }

        controllerOptions = _availableControllerOptions;

        if (controllerDropdownOptionsHandler != null)
        {
            if (controllerOptions.IsIndexOutOfRange(_currentControllerID))
            {
                _currentControllerID = 0; //Set to None if the current ID is invalid
            }

            controllerDropdownOptionsHandler.SetNewOptions(controllerOptions, _currentControllerID);
        }

        if (keyboardToggle != null)
        {
            keyboardToggle.SetIsOnWithoutNotify(_hasKeyboard);
        }
    }

    private void onControllerDropdownHandlerValueChanged(int _value) => OnControllerOptionChanged?.Invoke(this, _value);
    private void onKeyboardToggleChanged(int _id, CustomToggle _toggle, bool _value) => OnUseKeyboardChanged?.Invoke(this, _value);
    private void onActionButtonClicked(int _id, CustomButton _button) => OnActionButtonPressed?.Invoke(this);
}
