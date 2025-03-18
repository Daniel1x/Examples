using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    public static event UnityAction<PlayerController, RenderTexture> OnNewPlayerSpawned = null;
    public static event UnityAction<PlayerController, RenderTexture> OnPlayerDestroyed = null;

    [SerializeField] private ThirdPersonController thirdPersonController = null;

    private Camera playerCamera = null;
    private RenderTexture playerViewTexture = null;

    private bool canHandleInputs = false;

    public bool CanHandleInputs
    {
        get => canHandleInputs;
        set
        {
            canHandleInputs = value;

            if (thirdPersonController != null)
            {
                thirdPersonController.CanHandleInputs = value;
            }
        }
    }

    public Vector2Int RenderTextureSize { get; private set; } = new Vector2Int(16, 16);

    private void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        playerCamera.targetTexture = UpdateRenderTextureSize(RenderTextureSize);

        if (thirdPersonController != null)
        {
            thirdPersonController.OnJumpPerformed += onJump;
        }

        OnNewPlayerSpawned?.Invoke(this, playerViewTexture);
    }

    private void OnDestroy()
    {
        OnPlayerDestroyed?.Invoke(this, playerViewTexture);

        if (thirdPersonController != null)
        {
            thirdPersonController.OnJumpPerformed -= onJump;
        }

        playerCamera.targetTexture = null;
        playerViewTexture.ClearSpawnedRenderTexture();
    }

    private void onJump(ThirdPersonController _controller)
    {
        GroundAnimator.ShowCircle(transform.position);
    }

    public RenderTexture UpdateRenderTextureSize(Vector2Int _size)
    {
        RenderTextureSize = _size;
        RenderTextureExtensions.UpdateSpawnedRenderTextureSize(ref playerViewTexture, "rt_PlayerView", RenderTextureSize.x, RenderTextureSize.y, _onNewCreated: onNewRTCreated);
        return playerViewTexture;
    }

    private void onNewRTCreated(RenderTexture _rt)
    {
        playerCamera.targetTexture = _rt;
    }
}
