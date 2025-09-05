using UnityEngine;

/// <summary>
/// Central manager / facade for spawning visual effects (VFX) via pooled Addressables-based object pools.
/// Provides a simple API to request a VFX instance of a given <see cref="VFXType"/> at a position & rotation.
/// </summary>
/// <remarks>
/// Features:
/// - Simple enum based selection of effect type.
/// - Uses object pools (configured in the inspector) instead of instantiating new objects at runtime.
/// - Gracefully returns null if pool or an instance is not available (no hard exceptions).
/// - Optional parenting of the spawned effect transform.
/// </remarks>
public class VFXManager : MonoBehaviour
{
    /// <summary>
    /// Enumerates the supported visual effect categories. Extend as needed and assign pools in the inspector.
    /// </summary>
    public enum VFXType
    {
        Explosion = 0,
        SmallExplosion = 1,
        EnergyExplosion = 2,
        PlasmaExplosion = 3,
        Fog = 4,
    }

    /// <summary>
    /// Global singleton-style access (kept minimal; alternative: dependency injection or service locator).
    /// </summary>
    public static VFXManager Instance { get; private set; } = null;

    [Header("Pools (assign in inspector)")]
    [SerializeField] private ObjectPoolBehaviour explosionPool = null;
    [SerializeField] private ObjectPoolBehaviour smallExplosionPool = null;
    [SerializeField] private ObjectPoolBehaviour energyExplosionPool = null;
    [SerializeField] private ObjectPoolBehaviour plasmaExplosionPool = null;
    [SerializeField] private ObjectPoolBehaviour fogPool = null;

    private void Awake()
    {
        // Simple singleton guard (not persistent across scenes unless you add DontDestroyOnLoad).
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Returns the pool associated with the requested <paramref name="_vfxType"/> or null if not assigned.
    /// </summary>
    public ObjectPoolBehaviour GetPool(VFXType _vfxType)
    {
        return _vfxType switch
        {
            VFXType.Explosion => explosionPool,
            VFXType.SmallExplosion => smallExplosionPool,
            VFXType.EnergyExplosion => energyExplosionPool,
            VFXType.PlasmaExplosion => plasmaExplosionPool,
            VFXType.Fog => fogPool,
            _ => null,
        };
    }

    /// <summary>
    /// Spawns a visual effect (VFX) of the specified type at the given position.
    /// </summary>
    /// <param name="_vfxType">The type of visual effect to spawn.</param>
    /// <param name="_position">The position in world space where the visual effect should be spawned.</param>
    /// <param name="_parent">An optional parent transform to which the spawned visual effect will be attached. If null, the visual effect
    /// will not be parented.</param>
    /// <returns>The <see cref="GameObject"/> representing the spawned visual effect.</returns>
    public GameObject SpawnVFX(VFXType _vfxType, Vector3 _position, Transform _parent = null)
    {
        return SpawnVFX(_vfxType, _position, Quaternion.identity, null);
    }

    /// <summary>
    /// Spawns a visual effect (VFX) of the specified type at the given position and rotation, optionally attaching it
    /// to a parent transform.
    /// </summary>
    /// <remarks>This method retrieves an inactive object from the appropriate object pool, positions and
    /// rotates it as specified, and activates it.  If the object pool for the specified VFX type is exhausted or not
    /// found, the method logs an error and returns <see langword="null"/>.</remarks>
    /// <param name="_vfxType">The type of VFX to spawn, used to determine the appropriate object pool.</param>
    /// <param name="_position">The world position where the VFX should be spawned.</param>
    /// <param name="_rotation">The rotation to apply to the spawned VFX.</param>
    /// <param name="_parent">An optional parent transform to which the spawned VFX will be attached. If null, the VFX will not be parented.</param>
    /// <returns>The <see cref="GameObject"/> representing the spawned VFX, or <see langword="null"/> if the object pool for the
    /// specified VFX type is exhausted or does not exist.</returns>
    public GameObject SpawnVFX(VFXType _vfxType, Vector3 _position, Quaternion _rotation, Transform _parent = null)
    {
        ObjectPoolBehaviour _pool = GetPool(_vfxType);

        if (_pool == null)
        {
            Debug.LogError($"VFXManager: No pool found for VFXType {_vfxType}");
            return null;
        }

        // Request an inactive object (we will manually activate after positioning).
        ObjectSpawnedByPool _pulledObject = _pool.GetObjectFromPool(false);

        if (_pulledObject == null)
        {
            // Pool exhausted (and not auto-expanded yet); caller should handle null gracefully.
            return null;
        }

        Transform _t = _pulledObject.transform;
        _t.position = _position;
        _t.rotation = _rotation;

        if (_parent != null)
        {
            _t.SetParent(_parent, worldPositionStays: true);
        }

        _pulledObject.gameObject.SetActive(true);
        return _pulledObject.gameObject;
    }
}
