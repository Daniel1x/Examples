using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ObjectPoolBehaviourTemplate<T> : MonoBehaviour, IPoolBehaviour<T>, IPoolProvider where T : MonoBehaviour, IPoolable
{
    [Header("Pool Configuration")]
    [SerializeField] private AssetReferenceGameObject assetReference = new(string.Empty);
    [SerializeField] private Transform parentTransform = null;
    [SerializeField, Min(1)] private int poolSize = 10;
    [SerializeField, Min(-1f)] private float objectLifetime = 5f; // <= 0 means infinite
    [SerializeField] private bool expandPoolIfNeeded = false;
    [SerializeField] private bool initializeOnStart = true;

    private List<T> pool = null;

    public float ObjectMaxLifetime => objectLifetime;
    public Transform ParentTransform => parentTransform;

    private void OnValidate()
    {
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
        if (pool == null)
        {
            return;
        }

        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i] != null)
            {
                pool[i].CanReturnToPool = false;
                Addressables.ReleaseInstance(pool[i].gameObject);
            }
        }
    }

    public void InitializePool()
    {
        if (pool != null)
        {
            return;
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

        pool = new List<T>(poolSize);

        for (int i = 0; i < poolSize; i++)
        {
            AddNewInstance();
        }
    }

    public T GetObjectFromPool(bool _activate)
    {
        if (pool == null)
        {
            return null;
        }

        foreach (T _obj in pool)
        {
            if (_obj != null && !_obj.gameObject.activeInHierarchy)
            {
                if (_activate)
                {
                    _obj.gameObject.SetActive(true);
                }

                _obj.OnPulledFromPool();
                return _obj;
            }
        }

        if (expandPoolIfNeeded)
        {
            AddNewInstance(); // async, result not immediate
        }

        return null;
    }

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

    private void onNewObjectSpawned(AsyncOperationHandle<GameObject> _handle)
    {
        if (_handle.Status != AsyncOperationStatus.Succeeded)
        {
            return;
        }

        GameObject _newInstance = _handle.Result;
        _newInstance.SetActive(false);

        T _pooled = _newInstance.GetOrAddComponent<T>(out _);
        _pooled.Initialize(this);
        pool.Add(_pooled);
    }
}
