using UnityEngine;

public class PlayerController : PlayerProvider
{
    [SerializeField] private ThirdPersonController thirdPersonController = null;

    private PlayerIndicator playerIndicator = null;

    protected override void Awake()
    {
        base.Awake();

        if (thirdPersonController != null)
        {
            thirdPersonController.OnJumpPerformed += onJump;
        }

        playerIndicator = GetComponentInChildren<PlayerIndicator>(true);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (thirdPersonController != null)
        {
            thirdPersonController.OnJumpPerformed -= onJump;
        }
    }

    private void onJump(ThirdPersonController _controller)
    {
        GroundAnimator.ShowCircle(transform.position, playerIndicator != null ? playerIndicator.PlayerColor : Color.white);
    }
}
