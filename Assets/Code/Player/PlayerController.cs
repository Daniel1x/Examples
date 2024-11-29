using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static event UnityAction<PlayerController, RenderTexture> OnNewPlayerSpawned = null;
    public static event UnityAction<PlayerController, RenderTexture> OnPlayerDestroyed = null;

    [SerializeField] private float MovementSpeed = 5f;
    [SerializeField] private float RotationSpeed = 90f;
    [SerializeField] private float JumpStrength = 1f;

    private Camera playerCamera = null;
    private Rigidbody rb = null;

    private PlayerInput playerInput = null;
    private Input_Actions inputActions = null;

    [SerializeField, ReadOnlyProperty] private PlayerInputMap playerInputMaps = null;

    public RenderTexture PlayerViewTexture = null;
    public bool CanHandleInputs = false;

    public Vector2Int RenderTextureSize { get; private set; } = new Vector2Int(16, 16);

    private void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        rb = GetComponent<Rigidbody>();

        playerInput = GetComponent<PlayerInput>();
        inputActions = new Input_Actions();
        playerInputMaps = new PlayerInputMap(playerInput, inputActions);

        playerCamera.targetTexture = UpdateRenderTextureSize(RenderTextureSize);

        OnNewPlayerSpawned?.Invoke(this, PlayerViewTexture);
    }

    private void OnDestroy()
    {
        OnPlayerDestroyed?.Invoke(this, PlayerViewTexture);

        playerCamera.targetTexture = null;
        PlayerViewTexture.ClearSpawnedRenderTexture();
    }

    private void Update()
    {
        if (CanHandleInputs)
        {
            float _horiz = Input.GetAxis("Horizontal");
            float _vert = Input.GetAxis("Vertical");
            float _dt = Time.deltaTime;

            transform.Translate(Vector3.forward * MovementSpeed * _vert * _dt, Space.Self);
            transform.Rotate(Vector3.up * RotationSpeed * _horiz * _dt, Space.Self);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                rb.AddForce(Vector3.up * JumpStrength, ForceMode.Impulse);
            }
        }
    }

    public RenderTexture UpdateRenderTextureSize(Vector2Int _size)
    {
        RenderTextureSize = _size;
        RenderTextureExtensions.UpdateSpawnedRenderTextureSize(ref PlayerViewTexture, "rt_PlayerView", RenderTextureSize.x, RenderTextureSize.y, _onNewCreated: onNewRTCreated);
        return PlayerViewTexture;
    }

    private void onNewRTCreated(RenderTexture _rt)
    {
        playerCamera.targetTexture = _rt;
    }
}
