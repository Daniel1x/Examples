using UnityEngine;
using UnityEngine.Events;

public class PlayerProvider : MonoBehaviour
{
    public static event UnityAction<PlayerProvider, RenderTexture> OnNewPlayerSpawned = null;
    public static event UnityAction<PlayerProvider, RenderTexture> OnPlayerDestroyed = null;

    protected Camera playerCamera = null;
    protected RenderTexture playerViewTexture = null;

    public Vector2Int RenderTextureSize { get; private set; } = new Vector2Int(16, 16);

    protected virtual void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();

        if (playerCamera != null)
        {
            playerCamera.targetTexture = UpdateRenderTextureSize(RenderTextureSize);
        }

        OnNewPlayerSpawned?.Invoke(this, playerViewTexture);
    }

    protected virtual void OnDestroy()
    {
        OnPlayerDestroyed?.Invoke(this, playerViewTexture);

        if (playerCamera != null)
        {
            playerCamera.targetTexture = null;
        }

        playerViewTexture.ClearSpawnedRenderTexture();
    }

    public RenderTexture UpdateRenderTextureSize(Vector2Int _size)
    {
        RenderTextureSize = _size;
        RenderTextureExtensions.UpdateSpawnedRenderTextureSize(ref playerViewTexture, "rt_PlayerView", RenderTextureSize.x, RenderTextureSize.y, _onNewCreated: onNewRTCreated);
        return playerViewTexture;
    }

    private void onNewRTCreated(RenderTexture _rt)
    {
        _rt.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D32_SFloat;

        if (playerCamera != null)
        {
            playerCamera.targetTexture = _rt;
        }
    }
}
