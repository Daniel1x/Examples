using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Manages a pool of reusable objects instantiated from an Addressable asset reference.
/// </summary>
/// <remarks>This class provides functionality for creating, managing, and reusing a pool of objects to optimize
/// performance and reduce memory allocations. Objects are instantiated asynchronously using the Addressables system and
/// can be retrieved or returned to the pool as needed. The pool supports optional expansion and object lifetimes.  Key
/// features: - Configurable pool size and object lifetime. - Option to expand the pool dynamically if no objects are
/// available. - Automatic initialization on start, if enabled. - Integration with the Addressables system for efficient
/// asset management.  Use this class to manage frequently instantiated objects, such as projectiles, enemies, or visual
/// effects, in performance-critical applications.</remarks>
public class ObjectPoolBehaviour : MonoBehaviour
{
    [Header("Pool Configuration")]
    [SerializeField] private AssetReferenceGameObject assetReference = new(string.Empty);
    [SerializeField] private Transform parentTransform = null;
    [SerializeField, Min(1)] private int poolSize = 10;
    [SerializeField, Min(-1f)] private float objectLifetime = 5f; // <= 0 means 'infinite'
    [SerializeField] private bool expandPoolIfNeeded = false;
    [SerializeField] private bool initializeOnStart = true;

    /// <summary>Internal list storing pooled instances (both active and inactive).</summary>
    private List<ObjectSpawnedByPool> pool = null;

    /// <summary>Configured lifetime (seconds) for spawned objects; &lt;= 0 => no auto return.</summary>
    public float ObjectLifetime => objectLifetime;

    /// <summary>Transform under which pooled objects are instantiated; defaults to this GameObject's transform.</summary>
    public Transform ParentTransform => parentTransform;

    private void OnValidate()
    {
        // Normalize lifetime: values <= 0 treated as 'no timeout'
        if (objectLifetime <= 0f)
        {
            objectLifetime = -1f;
        }
    }

    private void Start()
    {
        if (initializeOnStart)
        {
            InitializePool();
        }
    }

    private void OnDestroy()
    {
        // Release all Addressables instances on pool destruction
        if (pool == null)
        {
            return;
        }

        foreach (ObjectSpawnedByPool _instance in pool)
        {
            if (_instance == null)
            {
                continue;
            }

            Addressables.ReleaseInstance(_instance.gameObject);

        }
    }

    /// <summary>
    /// Creates the initial pool of objects (if not already created).
    /// Does nothing if called multiple times.
    /// </summary>
    public void InitializePool()
    {
        if (pool != null)
        {
            return; // Already initialized
        }

        if (assetReference == null
            || string.IsNullOrEmpty(assetReference.AssetGUID)
            || assetReference.RuntimeKeyIsValid() == false)
        {
            Debug.LogWarning($"ObjectPoolBehaviour: Invalid AssetReference on '{gameObject.name}'");
            return;
        }

        if (parentTransform == null)
        {
            parentTransform = transform;
        }

        pool = new List<ObjectSpawnedByPool>(poolSize);

        for (int i = 0; i < poolSize; i++)
        {
            AddNewInstance(); // Asynchronous instantiation
        }
    }

    /// <summary>
    /// Retrieves an inactive object from the pool (optional activation).
    /// If none are available and expansion is allowed, a new instance request is queued.
    /// Returns null immediately if no instance is currently free (even if expansion occurs).
    /// </summary>
    /// <param name="_getAsActive">If true, activates the object before returning.</param>
    public ObjectSpawnedByPool GetObjectFromPool(bool _getAsActive)
    {
        if (pool == null)
        {
            return null;
        }

        // Linear scan (pool typically small / medium sized; can be optimized if needed)
        foreach (ObjectSpawnedByPool obj in pool)
        {
            if (obj != null && !obj.gameObject.activeInHierarchy)
            {
                if (_getAsActive)
                {
                    obj.gameObject.SetActive(true);
                }

                obj.OnPulledFromPool();
                return obj;
            }
        }

        // No available object
        if (expandPoolIfNeeded)
        {
            AddNewInstance(); // Asynchronous; result not immediately usable
        }

        return null;
    }

    /// <summary>
    /// Returns (deactivates) the object to the pool.
    /// Caller is responsible for ensuring the object belongs to this pool.
    /// </summary>
    public void ReturnObjectToPool(GameObject _obj)
    {
        if (_obj == null)
        {
            return;
        }

        _obj.SetActive(false);
    }

    /// <summary>
    /// Instantiates a new object from the specified asset reference and adds it to the object pool.
    /// </summary>
    /// <remarks>This method uses the Addressables system to asynchronously instantiate the object.  If the
    /// <see cref="assetReference"/> is invalid, a warning is logged, and no object is instantiated.</remarks>
    public void AddNewInstance()
    {
        if (assetReference == null
            || string.IsNullOrEmpty(assetReference.AssetGUID)
            || assetReference.RuntimeKeyIsValid() == false)
        {
            Debug.LogWarning($"ObjectPoolBehaviour: Invalid AssetReference on '{gameObject.name}'");
            return;
        }

        Addressables.InstantiateAsync(assetReference, parentTransform).Completed += onNewObjectSpawned;
    }

    /// <summary>
    /// Callback executed when an Addressables instantiation completes.
    /// Adds appropriate ObjectSpawnedByPool component (with or without lifetime).
    /// </summary>
    private void onNewObjectSpawned(AsyncOperationHandle<GameObject> _handle)
    {
        if (_handle.Status != AsyncOperationStatus.Succeeded)
        {
            return;
        }

        GameObject newObject = _handle.Result;
        newObject.SetActive(false);

        // Attach behaviour that manages return logic (with optional lifetime).
        ObjectSpawnedByPool pooledComponent = objectLifetime > 0f
            ? newObject.AddComponent<ObjectSpawnedByPoolWithLifetime>()
            : newObject.AddComponent<ObjectSpawnedByPool>();

        pooledComponent.Initialize(this);

        pool.Add(pooledComponent);
    }
}
