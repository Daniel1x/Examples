using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : UnitCharacterController
{
    private GameObject playerCamera = null;
    private CameraTargetProvider cameraTargetProvider = null;

    protected override void Awake()
    {
        base.Awake();

        if (playerCamera == null)
        {
            playerCamera = gameObject.GetComponentInChildren<Camera>()?.gameObject;
        }

        if (cameraTargetProvider == null)
        {
            cameraTargetProvider = gameObject.GetComponent<CameraTargetProvider>();
        }
    }

    protected override Vector3 adjustTargetMovementDirection(Vector3 _targetDirection)
    {
        if (cameraTargetProvider != null && cameraTargetProvider.CameraManager != null)
        {
            _targetDirection = cameraTargetProvider.CameraManager.AdjustMovementVector(transform, _targetDirection, out _);
        }

        return _targetDirection;
    }

    protected override float getTargetRotation(Vector2 _inputs)
    {
        GameObject _camObject = cameraTargetProvider != null && cameraTargetProvider.VirtualCamera != null
            ? cameraTargetProvider.VirtualCamera.gameObject
            : playerCamera;

        if (_camObject != null)
        {
            return Mathf.Atan2(_inputs.x, _inputs.y) * Mathf.Rad2Deg + _camObject.transform.eulerAngles.y;
        }

        return base.getTargetRotation(_inputs);
    }
}
