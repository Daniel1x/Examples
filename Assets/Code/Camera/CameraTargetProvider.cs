using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

public class CameraTargetProvider : MonoBehaviour
{
    public static event UnityAction<CameraTargetProvider> OnTargetEnabled = null;
    public static event UnityAction<CameraTargetProvider> OnTargetDisabled = null;
    public static event UnityAction<CameraTargetProvider> OnTargetSettingsChanged = null;

    [SerializeField] private int priority = 0;
    [SerializeField] private float weight = 1f;
    [SerializeField] private float radius = 1f;

    public CinemachineCamera VirtualCamera { get; set; } = null;
    public CameraManager CameraManager { get; set; } = null;
    public virtual Transform TransformToFollow => transform;

    public int Priority
    {
        get => priority;
        set
        {
            if (priority != value)
            {
                priority = value;
                OnTargetSettingsChanged?.Invoke(this);
            }
        }
    }

    public float Weight
    {
        get => weight;
        set
        {
            if (weight != value)
            {
                weight = value;
                OnTargetSettingsChanged?.Invoke(this);
            }
        }
    }

    public float Radius
    {
        get => radius;
        set
        {
            if (radius != value)
            {
                radius = value;
                OnTargetSettingsChanged?.Invoke(this);
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        OnTargetSettingsChanged?.Invoke(this);
    }
#endif

    private void OnEnable()
    {
        OnTargetEnabled?.Invoke(this);
    }

    private void OnDisable()
    {
        OnTargetDisabled?.Invoke(this);
    }
}
