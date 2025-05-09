using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using OptionData = TMPro.TMP_Dropdown.OptionData;

[RequireComponent(typeof(CustomDropdown))]
public class DropdownOptionsHandler : MonoBehaviour, INavigationItemContainer<CustomDropdown>, IDropdownContainer
{
    public event UnityAction<int> OnDropdownValueChanged = null;

    [SerializeField] protected string[] options = new string[] { };

    protected OptionData[] optionDatas = new OptionData[] { };
    protected CustomDropdown dropdown = null;

    public CustomDropdown Dropdown => dropdown;
    public CustomDropdown NavigationItem => dropdown;

    protected void Awake()
    {
        if (dropdown == null)
        {
            dropdown = GetComponent<CustomDropdown>();
        }

        if (dropdown != null)
        {
            dropdown.onValueChanged.AddListener(dropdownValueChanged);
        }

        resetOptionDatasOnDropdown();
    }

    protected void OnDestroy()
    {
        if (dropdown != null)
        {
            dropdown.onValueChanged.RemoveListener(dropdownValueChanged);
        }
    }

    public void SetDropdownValueWithoutNotify(int _value)
    {
        if (_value.IsInRange(0, optionDatas.Length - 1) && dropdown.value != _value)
        {
            dropdown.SetValueWithoutNotify(_value);
        }
    }

    public void SetNewOptions(string[] _options, int _currentSelected)
    {
        if (_options.IsNullOrEmpty() || _options.IsIndexOutOfRange(_currentSelected))
        {
            Debug.LogError($"Dropdown Options Handler: Set New Options: Invalid options or current selected index. Options: {_options}, Current Selected: {_currentSelected}");
            return;
        }

        options = _options;
        resetOptionDatasOnDropdown();
        SetDropdownValueWithoutNotify(_currentSelected);
    }

    protected void resetOptionDatasOnDropdown()
    {
        dropdown.ClearOptions();

        int _count = options.Length;
        optionDatas = new OptionData[_count];

        for (int i = 0; i < _count; i++)
        {
            optionDatas[i] = new OptionData(options[i]);
        }

        dropdown.options = optionDatas.ToList();
    }

    protected void dropdownValueChanged(int _index)
    {
        OnDropdownValueChanged?.Invoke(_index);
    }
}
