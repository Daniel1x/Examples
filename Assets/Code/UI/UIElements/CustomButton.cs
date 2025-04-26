using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomButton : Button, INavigationItem<CustomButton>, IButtonContainer
{
    [System.Flags]
    public enum GraphicsFadeStyles
    {
        None = 0,
        FadeInOnSelect = 1,
        FadeOutOnDeselect = 2,
        FadeInOnClick = 4,

        Default = 3
    }

    public enum ButtonEventType
    {
        OnClick = 0,
        OnSelect = 1,
        OnDeselect = 2,
        OnSubmit = 3
    }

    public event UnityAction<int, CustomButton> OnItemSelected = null;
    public event UnityAction<int, CustomButton> OnItemDeselected = null;
    public event UnityAction<int, CustomButton> OnItemSubmitted = null;
    public event UnityAction<int, CustomButton> OnItemClicked = null;
    public event UnityAction<int, CustomButton> OnItemDestroyed = null;

    [SerializeField] protected bool invokeOnAnyItemSelected = true;
    [SerializeField] protected bool allowNavigationToOtherParent = true;
    [SerializeField] protected bool allowNavitationToThisObject = true;
    [SerializeField] protected bool forceSelectEventOnEnable = true;
    [SerializeField] protected bool forceDeselectEventOnDisable = true;
    [SerializeField] protected bool forceSelectOnPointerEnter = true;
    [SerializeField] protected bool forceClickOnPointerDown = false;
    [SerializeField] protected bool refreshColorTransitionAfterFrame = false;

    [SerializeField] protected GraphicsFadeStyles fadeStyle = GraphicsFadeStyles.None;
    [SerializeField] protected Graphic[] graphicsToFade = new Graphic[] { };

    [SerializeField] protected SelectableExplicitNavigator explicitNavigator = new SelectableExplicitNavigator();
    [SerializeField] protected SelectionEventController selectionEventController = new SelectionEventController();
    [SerializeField] protected GraphicsTransitionController graphicsTransitionController = new GraphicsTransitionController();

    protected bool blockButtonSubmit = false;
    protected bool blockButtonState = false;
    protected int itemIndex = -1;

    public int ItemIndex { get => itemIndex < 0 ? transform.GetSiblingIndex() : itemIndex; set => itemIndex = value; }

    public CustomButton NavigationItem => this;
    public INavigationItemPart Owner { get; set; } = null;

    public bool AllowNavigationToOtherParent => allowNavigationToOtherParent;
    public bool AllowNavigationToThisObject => allowNavitationToThisObject;
    public bool ForceSelectEventOnEnable => forceSelectEventOnEnable;
    public bool ForceDeselectEventOnDisable => forceDeselectEventOnDisable;
    public bool SelectInProgressTriggeredFromOnEnable { get; set; } = false;

    public SelectableExplicitNavigator ExplicitNavigator => explicitNavigator;
    public SelectionEventController SelectionEventController => selectionEventController;
    public GraphicsTransitionController GraphicsTransitionController => graphicsTransitionController;

    public CustomButton Button => this;
    public bool BlockButtonSubmit { get => blockButtonSubmit; set => blockButtonSubmit = value; }
    public bool IsSelected => selectionEventController.IsSelected;

    protected override void Awake()
    {
        selectionEventController.UpdateSelectionState();

        onClick.AddListener(onClickAction);
        graphicsTransitionController.SetSelectable(this);
    }

    protected override void OnDestroy()
    {
        onClick.RemoveListener(onClickAction);
        OnItemDestroyed?.Invoke(ItemIndex, this);

        selectionEventController = null;
        graphicsTransitionController = null;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (refreshColorTransitionAfterFrame)
        {
            RefreshColorTransitionAfterFrame();
        }

        if (fadeStyle != GraphicsFadeStyles.None)
        {
            FadeGraphicsAlpha(false, 0f, true);
        }

        selectionEventController.UpdateIsSelectedOnEnable(this);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        selectionEventController.TryToForceOnDeselect(this);
    }

    public override Selectable FindSelectableOnDown()
    {
        return this.FindNextSelectable(MoveDirection.Down, navigation.mode, navigation.selectOnDown, explicitNavigator);
    }

    public override Selectable FindSelectableOnLeft()
    {
        return this.FindNextSelectable(MoveDirection.Left, navigation.mode, navigation.selectOnLeft, explicitNavigator);
    }

    public override Selectable FindSelectableOnRight()
    {
        return this.FindNextSelectable(MoveDirection.Right, navigation.mode, navigation.selectOnRight, explicitNavigator);
    }

    public override Selectable FindSelectableOnUp()
    {
        return this.FindNextSelectable(MoveDirection.Up, navigation.mode, navigation.selectOnUp, explicitNavigator);
    }

    public Selectable FindClosestSelectable(bool _forcePermissionForOtherParent = false)
    {
        return SelectableNavigator.FindClosestSelectable(this, _forcePermissionForOtherParent);
    }

    public override void OnSubmit(BaseEventData _eventData)
    {
        if (blockButtonSubmit)
        {
            return;
        }

        base.OnSubmit(_eventData);
        OnItemSubmitted?.Invoke(ItemIndex, this);
    }

    public override void OnSelect(BaseEventData _eventData)
    {
        selectionEventController.IsSelected = true;

        if (invokeOnAnyItemSelected)
        {
            INavigationItem.OnAnyItemSelected?.Invoke(this);
        }

        base.OnSelect(_eventData);
        OnItemSelected?.Invoke(ItemIndex, this);

        if (buttonIsUsingFadeStyle(GraphicsFadeStyles.FadeInOnSelect))
        {
            FadeGraphicsAlpha(true);
        }
    }

    public override void OnPointerEnter(PointerEventData _eventData)
    {
        base.OnPointerEnter(_eventData);

        if (forceSelectOnPointerEnter && interactable)
        {
            Select();
        }
    }

    public void ChangeAllowNavigationToOtherParent(bool _allow)
    {
        allowNavigationToOtherParent = _allow;
    }

    public override void OnDeselect(BaseEventData _eventData)
    {
        selectionEventController.IsSelected = false;

        base.OnDeselect(_eventData);
        OnItemDeselected?.Invoke(ItemIndex, this);

        if (buttonIsUsingFadeStyle(GraphicsFadeStyles.FadeOutOnDeselect))
        {
            FadeGraphicsAlpha(false);
        }
    }

    public void ChangeAllowNavigationToThisObject(bool _allowNavigation)
    {
        allowNavitationToThisObject = _allowNavigation;
    }

    protected override void DoStateTransition(SelectionState _state, bool _instant)
    {
        base.DoStateTransition(_state, _instant);
        graphicsTransitionController.DoStateTransition((GraphicsTransitionController.SelectionState)_state, _instant);
    }

    public void RefreshStateTransition(bool _instant = false)
    {
        DoStateTransition(currentSelectionState, _instant);
    }

    public void RefreshColorTransitionAfterFrame(bool _instant = false)
    {
        if (gameObject.activeInHierarchy == false)
        {
            return;
        }

        StartCoroutine(_refreshAfterFrame());

        System.Collections.IEnumerator _refreshAfterFrame()
        {
            yield return null;
            RefreshStateTransition(_instant);
        }
    }

    public override void OnPointerDown(PointerEventData _eventData)
    {
        base.OnPointerDown(_eventData);

        if (forceClickOnPointerDown)
        {
            OnPointerUp(_eventData);
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (forceClickOnPointerDown == false)
        {
            base.OnPointerUp(eventData);
        }
    }

    public void FadeGraphicsAlpha(bool _fadeIn, float _fadeDuration = -1f, bool _ignoreTimeScale = true)
    {
        if (blockButtonState)
        {
            return;
        }

        int _count = graphicsToFade.Length;

        if (_count > 0)
        {
            float _newAlpha = _fadeIn ? 1f : 0f;
            float _newFadeDuration = _fadeDuration == -1f ? colors.fadeDuration : _fadeDuration;

            for (int i = 0; i < _count; i++)
            {
                graphicsToFade[i]?.CrossFadeAlpha(_newAlpha, _newFadeDuration, _ignoreTimeScale);
            }
        }
    }

    public void SetBlockButtonState(bool _blockButtonState)
    {
        blockButtonState = _blockButtonState;
    }

    private bool buttonIsUsingFadeStyle(GraphicsFadeStyles _style)
    {
        return (fadeStyle & _style) != 0;
    }

    private void onClickAction()
    {
        OnItemClicked?.Invoke(ItemIndex, this);

        if (buttonIsUsingFadeStyle(GraphicsFadeStyles.FadeInOnClick))
        {
            FadeGraphicsAlpha(true);
        }
    }
}
