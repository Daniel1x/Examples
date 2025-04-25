using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class SelectableContainer : MonoBehaviour, ISelectHandler, IDeselectHandler, SelectionEventController.INavigationItem
{
    [SerializeField] private Selectable selectable = null;
    [SerializeField] private SelectionEventController selectionEventController = null;

    [SerializeField] private bool forceSelectEventOnEnable = true;
    [SerializeField] private bool forceDeselectEventOnDisable = true;

    public Selectable Selectable => selectable;

    public bool ForceSelectEventOnEnable => forceSelectEventOnEnable;
    public bool ForceDeselectEventOnDisable => forceDeselectEventOnDisable;
    public bool SelectInProgressTriggeredFromOnEnable { get; set; } = false;

    private void Awake()
    {
        findSelectable();
    }

    private void Reset()
    {
        findSelectable(true);
    }

    private void OnEnable()
    {
        selectionEventController.UpdateIsSelectedOnEnable(selectable, this);
    }

    private void OnDisable()
    {
        selectionEventController.TryToForceOnDeselect(selectable, this);
    }

    private void findSelectable(bool _reset = false)
    {
        if (selectable == null || _reset)
        {
            selectable = GetComponent<Selectable>();
        }
    }

    public void OnSelect(BaseEventData _eventData)
    {
        selectionEventController.IsSelected = true;
    }

    public void OnDeselect(BaseEventData _eventData)
    {
        selectionEventController.IsSelected = false;
    }

    public bool IsObjectSelectedByEventSystem()
    {
        return EventSystem.current != null
            && EventSystem.current.currentSelectedGameObject != null
            && EventSystem.current.currentSelectedGameObject == gameObject;
    }
}
