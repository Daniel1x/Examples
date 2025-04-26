using UnityEngine.Events;
using UnityEngine.UI;

public interface INavigationItem<T> : INavigationItem where T : Selectable, INavigationItem
{
    public event UnityAction<int, T> OnItemSelected;
    public event UnityAction<int, T> OnItemDeselected;
    public event UnityAction<int, T> OnItemDestroyed;

    public int ItemIndex { get; set; }

    public T NavigationItem { get; }
}

public interface INavigationItem
{
    public static UnityAction<INavigationItem> OnAnyItemSelected = null;

    public INavigationItemPart Owner { get; set; }

    bool AllowNavigationToOtherParent { get; }
    bool AllowNavigationToThisObject { get; }
    bool ForceSelectEventOnEnable { get; }
    bool ForceDeselectEventOnDisable { get; }
    bool SelectInProgressTriggeredFromOnEnable { get; set; }

    SelectableExplicitNavigator ExplicitNavigator { get; }
    SelectionEventController SelectionEventController { get; }
    GraphicsTransitionController GraphicsTransitionController { get; }

    Selectable FindSelectableOnDown();
    Selectable FindSelectableOnLeft();
    Selectable FindSelectableOnRight();
    Selectable FindSelectableOnUp();
    Selectable FindClosestSelectable(bool _forcePermissionForOtherParent = false);

    public virtual bool IsSelected => SelectionEventController.IsSelected;
}
