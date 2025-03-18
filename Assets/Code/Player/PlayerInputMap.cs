using DL.Enum;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[System.Serializable]
public class PlayerInputMap
{
    [System.Flags]
    public enum InputMap
    {
        Nothing = 0,

        System = 1 << 0,
        Player = 1 << 1,

        All = ~0
    }

    [System.Serializable]
    public abstract class InputMapModule
    {
        protected Input_Actions inputActions = null;
        protected bool isEnabled = false;

        public bool IsEnabled
        {
            get => isEnabled;
            set => SetEnabled(value);
        }

        public abstract InputMap MapType { get; }

        public InputMapModule(Input_Actions _inputActions)
        {
            inputActions = _inputActions;
            setCallbacks();
        }

        public void SetEnabled(bool _enabled, bool _forceUpdate = false)
        {
            if (_enabled != isEnabled || _forceUpdate)
            {
                isEnabled = _enabled;
                setMapEnabled(_enabled);
            }
        }

        public void UpdateEnabled(InputMap _enabledMaps, bool _forceUpdate = false)
        {
            SetEnabled(_enabledMaps.ContainsFlag(MapType), _forceUpdate);
        }

        public virtual void Update() { }

        protected abstract void setCallbacks();
        protected abstract void setMapEnabled(bool _enabled);
    }

    public event UnityAction<InputMap> OnInputMapChanged = null;

    private List<InputMapModule> modules = null;
    private InputMap enabledMaps = InputMap.All;

    public InputMap EnabledMaps
    {
        get => enabledMaps;
        set
        {
            value = (InputMap)value.AddFlag(InputMap.System); //Force system flag to be always enabled

            if (enabledMaps != value)
            {
                enabledMaps = value;
                UpdateEnabledMaps();
                OnInputMapChanged?.Invoke(enabledMaps);
            }
        }
    }

    public PlayerInputMap(PlayerInput _playerInput, Input_Actions _actions)
    {
        //Create modules
        modules = new List<InputMapModule>() { };
    }

    public void UpdateEnabledMaps(bool _forceUpdate = false)
    {
        for (int i = 0; i < modules.Count; i++)
        {
            modules[i].UpdateEnabled(EnabledMaps, _forceUpdate);
        }
    }
}