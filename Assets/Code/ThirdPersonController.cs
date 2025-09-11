using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : UnitCharacterController<PlayerBasicInputs>
{
    private GameObject playerCamera = null;
    private CameraTargetProvider cameraTargetProvider = null;
    private UnitStats playerCharacterStats = null;

    private bool sprintBlocked = false;

    protected override bool isSprinting
    {
        get
        {
            if (!base.isSprinting)
            {
                return false; // No inputs
            }

            if (playerCharacterStats == null)
            {
                return true; // No need to check stats
            }

            if (sprintBlocked)
            {
                if (playerCharacterStats.IsStaminaAboveThreshold)
                {
                    sprintBlocked = false;
                }
                else
                {
                    return false; // Wait for stamina to regenerate
                }
            }

            float _requiredStamina = playerCharacterStats.StaminaRunCost * Time.deltaTime;

            if (playerCharacterStats.CanUseStamina(_requiredStamina))
            {
                return true;
            }
            else
            {
                sprintBlocked = true;
                return false; // Wait for stamina to regenerate
            }
        }
    }

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

        if (playerCharacterStats == null)
        {
            playerCharacterStats = gameObject.GetComponent<UnitStats>();
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
