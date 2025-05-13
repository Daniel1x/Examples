using DL.Structs;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    [Header("Camera Manager")]
    [SerializeField] private CinemachineBrain brain = null;
    [SerializeField] private CinemachineCamera virtualCamera = null;
    [SerializeField] private CinemachineTargetGroup targetGroup = null;

    [Header("Settings")]
    [SerializeField, MinMaxSlider(0f, 100f)] private MinMax cameraDistanceRange = new MinMax(10f, 30f);
    [SerializeField] private AnimationCurve cameraDistanceAdjustmentCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve movementAdjustmentsCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    private Dictionary<int, List<CameraTargetProvider>> targetGroups = new Dictionary<int, List<CameraTargetProvider>>();
    private List<CinemachineTargetGroup.Target> createdInstances = new();
    private List<CinemachineTargetGroup.Target> currentTargets = new();

    private float cameraDistance = 10f;
    private CinemachinePositionComposer positionComposer = null;

    //Values for calculations
    private float maxDistance = 0f;

    public CinemachineBrain Brain => brain;

    private void Awake()
    {
        if (virtualCamera != null)
        {
            positionComposer = virtualCamera.GetComponent<CinemachinePositionComposer>();

            if (positionComposer != null)
            {
                cameraDistance = positionComposer.CameraDistance;
            }
        }

        PlayerInputManagerProvider.OnAnyPlayerJoined += onPlayerJoined;

        CameraTargetProvider.OnTargetEnabled += onTargetEnabled;
        CameraTargetProvider.OnTargetDisabled += onTargetDisabled;
        CameraTargetProvider.OnTargetSettingsChanged += onTargetSettingsChanged;
    }

    private void OnDestroy()
    {
        PlayerInputManagerProvider.OnAnyPlayerJoined -= onPlayerJoined;

        CameraTargetProvider.OnTargetEnabled -= onTargetEnabled;
        CameraTargetProvider.OnTargetDisabled -= onTargetDisabled;
        CameraTargetProvider.OnTargetSettingsChanged -= onTargetSettingsChanged;
    }

    private void LateUpdate()
    {
        adjustCameraDistance();
        applyCameraSettings();
    }

    public Vector3 AdjustMovementVector(Transform _movedTransform, Vector3 _movementDirection, out float _adjustment)
    {
        _adjustment = 1f;

        if (_movedTransform == null || targetGroup == null)
        {
            return _movementDirection;
        }

        Vector3 _flatDirectionToTransform = targetGroup.DirectionTo(_movedTransform).WithY(0);

        //Moved towards center
        if (Vector3.Dot(_flatDirectionToTransform, _movementDirection.WithY(0)) < 0f)
        {
            return _movementDirection;
        }

        float _distanceToTransform = _flatDirectionToTransform.magnitude;
        _adjustment = movementAdjustmentsCurve.Evaluate(_distanceToTransform);

        return _adjustment * _movementDirection;
    }

    private void applyCameraSettings()
    {
        if (positionComposer != null)
        {
            positionComposer.CameraDistance = cameraDistance;
        }
    }

    private void adjustCameraDistance()
    {
        if (currentTargets.IsNullOrEmpty() || currentTargets.Count == 1)
        {
            cameraDistance = cameraDistanceRange.Min;
        }
        else
        {
            maxDistance = 0f;

            for (int i = 0; i < currentTargets.Count; i++)
            {
                float _distance = currentTargets[i].Object.DirectionTo(targetGroup).WithY(0).magnitude;

                if (_distance > maxDistance)
                {
                    maxDistance = _distance;
                }
            }

            cameraDistance = cameraDistanceRange.GetValueFromRange(cameraDistanceAdjustmentCurve.Evaluate(maxDistance));
        }
    }

    private void onTargetSettingsChanged(CameraTargetProvider _target)
    {
        if (removeTargetFromGroup(_target))
        {
            addTargetToGroup(_target);
        }

        updateTargetsToFollow();
    }

    private bool addTargetToGroup(CameraTargetProvider _target)
    {
        if (_target == null)
        {
            return false;
        }

        int _priority = _target.Priority;

        if (targetGroups.ContainsKey(_priority) == false)
        {
            targetGroups.Add(_priority, new List<CameraTargetProvider>());
        }

        if (targetGroups[_priority].Contains(_target))
        {
            return false;
        }

        targetGroups[_priority].Add(_target);
        return true;
    }

    private bool removeTargetFromGroup(CameraTargetProvider _target)
    {
        foreach (KeyValuePair<int, List<CameraTargetProvider>> _group in targetGroups)
        {
            if (_group.Value.Contains(_target))
            {
                return _group.Value.Remove(_target);
            }
        }

        return false;
    }

    private void onTargetEnabled(CameraTargetProvider _target)
    {
        _target.VirtualCamera = virtualCamera;
        _target.CameraManager = this;

        addTargetToGroup(_target);
        updateTargetsToFollow();
    }

    private void onTargetDisabled(CameraTargetProvider _target)
    {
        removeTargetFromGroup(_target);
        updateTargetsToFollow();
    }

    private void updateTargetsToFollow()
    {
        if (targetGroup == null)
        {
            return;
        }

        if (getGroupWithHighestPriority(out var _group) == false)
        {
            return;
        }

        int _requiredCount = _group.Count;

        if (_requiredCount <= 0)
        {
            return;
        }

        //Make sure we have enough instances
        for (int i = createdInstances.Count; i < _requiredCount; i++)
        {
            createdInstances.Add(new CinemachineTargetGroup.Target());
        }

        int _currentTargetsSize = currentTargets.Count;

        if (_currentTargetsSize != _requiredCount)
        {
            int _toAdd = _requiredCount - _currentTargetsSize;

            if (_toAdd > 0)
            {
                //Add the new targets
                for (int i = 0; i < _toAdd; i++)
                {
                    currentTargets.Add(createdInstances[currentTargets.Count]);
                }
            }
            else
            {
                //Remove the excess targets
                while (currentTargets.Count > _requiredCount)
                {
                    currentTargets.RemoveAt(currentTargets.Count - 1);
                }
            }
        }

        if (_requiredCount != currentTargets.Count)
        {
            Debug.LogError($"Target count mismatch: {_requiredCount} != {currentTargets.Count}");
            return;
        }

        for (int i = 0; i < _requiredCount; i++)
        {
            CameraTargetProvider _target = _group[i];

            if (_target == null)
            {
                continue;
            }

            currentTargets[i].Object = _target.TransformToFollow;
            currentTargets[i].Weight = _target.Weight;
            currentTargets[i].Radius = _target.Radius;
        }

        targetGroup.Targets = currentTargets;
    }

    private void onPlayerJoined(PlayerInput _player)
    {
        if (targetGroup != null)
        {
            _player.transform.position = targetGroup.transform.position;
        }
    }

    private bool getGroupWithHighestPriority(out List<CameraTargetProvider> _targetGroup)
    {
        if (targetGroups.IsNullOrEmpty())
        {
            _targetGroup = null;
            return false;
        }

        int _highestPriority = int.MinValue;
        _targetGroup = null;

        foreach (var _group in targetGroups)
        {
            if (_group.Key > _highestPriority && _group.Value.IsNullOrEmpty() == false)
            {
                _highestPriority = _group.Key;
                _targetGroup = _group.Value;
            }
        }

        return _targetGroup != null;
    }
}
