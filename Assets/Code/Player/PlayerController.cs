using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private ThirdPersonController thirdPersonController = null;

    private PlayerIndicator playerIndicator = null;

    protected void Awake()
    {
        if (thirdPersonController != null)
        {
            thirdPersonController.OnJumpPerformed += onJump;
        }

        playerIndicator = GetComponentInChildren<PlayerIndicator>(true);
    }

    protected void OnDestroy()
    {
        if (thirdPersonController != null)
        {
            thirdPersonController.OnJumpPerformed -= onJump;
        }
    }

    private void onJump(UnitCharacterController _controller)
    {
        GroundAnimator.ShowCircle(transform.position, playerIndicator != null ? playerIndicator.PlayerColor : Color.white);
    }
}
