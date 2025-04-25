using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class SelectionEventController
{
    [System.Flags]
    public enum SelectionEventMode
    {
        None = 0,
        SetObjectActiveOnSelect = 1,
        SetBehaviourEnabledOnSelect = 2,
        SetObjectDisabledOnSelect = 4,
        SetBehaviourDisabledOnSelect = 8
    }

    public interface INavigationItem
    {
        bool IsObjectSelectedByEventSystem();
        bool ForceSelectEventOnEnable { get; }
        bool ForceDeselectEventOnDisable { get; }
        bool SelectInProgressTriggeredFromOnEnable { get; set; }
    }

    public event UnityAction<bool> OnSelectionUpdated = null;

    [SerializeField] private SelectionEventMode selectionEventMode = SelectionEventMode.None;
    [SerializeField] private GameObject[] objectsToEnableOnSelect = new GameObject[] { };
    [SerializeField] private Behaviour[] behavioursToEnableOnSelect = new Behaviour[] { };
    [SerializeField] private GameObject[] objectsToDisableOnSelect = new GameObject[] { };
    [SerializeField] private Behaviour[] behavioursToDisableOnSelect = new Behaviour[] { };

    private bool isSelected = false;

    public bool IsSelected
    {
        get => isSelected;
        set
        {
            isSelected = value;
            UpdateSelectionState();

            OnSelectionUpdated?.Invoke(isSelected);
        }
    }

    public void UpdateIsSelectedOnEnable<T>(Selectable _selectable, T _naviItem) where T : INavigationItem
    {
        IsSelected = _naviItem.IsObjectSelectedByEventSystem();

        if (IsSelected && _naviItem.ForceSelectEventOnEnable)
        {
            _naviItem.SelectInProgressTriggeredFromOnEnable = true;
            _selectable.OnSelect(null);
            _naviItem.SelectInProgressTriggeredFromOnEnable = false;
        }
    }

    public void TryToForceOnDeselect<T>(Selectable _selectable, T _naviItem) where T : INavigationItem
    {
        if (IsSelected && _naviItem.ForceDeselectEventOnDisable)
        {
            _selectable.OnDeselect(null);
        }
    }

    public void UpdateSelectionState()
    {
        if (selectionEventMode == SelectionEventMode.None)
        {
            return;
        }

        if (isModeActive(SelectionEventMode.SetObjectActiveOnSelect))
        {
            setObjectsEnabled(objectsToEnableOnSelect, isSelected);
        }

        if (isModeActive(SelectionEventMode.SetBehaviourEnabledOnSelect))
        {
            setBehavioursEnabled(behavioursToEnableOnSelect, isSelected);
        }

        if (isModeActive(SelectionEventMode.SetObjectDisabledOnSelect))
        {
            setObjectsEnabled(objectsToDisableOnSelect, !isSelected);
        }

        if (isModeActive(SelectionEventMode.SetBehaviourDisabledOnSelect))
        {
            setBehavioursEnabled(behavioursToDisableOnSelect, !isSelected);
        }
    }

    private bool isModeActive(SelectionEventMode _event)
    {
        return (selectionEventMode & _event) != 0;
    }

    private void setBehavioursEnabled<T>(T[] _behaviours, bool _enabled) where T : Behaviour
    {
        foreach (T _behaviour in _behaviours)
        {
            if (_behaviour == null)
            {
                continue;
            }

            if (_behaviour.enabled != _enabled)
            {
                _behaviour.enabled = _enabled;
            }
        }
    }

    private void setObjectsEnabled(GameObject[] _objects, bool _enabled)
    {
        foreach (GameObject _gameObject in _objects)
        {
            if (_gameObject == null)
            {
                continue;
            }

            if (_gameObject.activeSelf != _enabled)
            {
                _gameObject.SetActive(_enabled);
            }
        }
    }
}
