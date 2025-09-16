using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EquipmentSocket : MonoBehaviour
{
    [System.Serializable]
    public class AssetReferenceData
    {
        public AssetReferenceGameObject Reference { get; private set; } = null;
        public GameObject Instance { get; private set; } = null;
        public IEquipment Equipment { get; private set; } = null;

        private bool isEnabled = false;
        private Transform assetParent = null;

        private int ownerLayer = -1;
        private ICanApplyDamage canApplyDamageProvider = null;

        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled == value) return;
                isEnabled = value;

                if (Instance != null)
                {
                    Instance.SetActive(isEnabled);
                }
                else if (isEnabled)
                {
                    EnsureSpawnedAsset();
                }
            }
        }

        public AssetReferenceData(AssetReferenceGameObject _reference, Transform _parent, int _ownerLayer, ICanApplyDamage _iCanApplyDamage, bool _initialEnabled = false)
        {
            Reference = _reference;
            assetParent = _parent;
            isEnabled = _initialEnabled;
            ownerLayer = _ownerLayer;
            canApplyDamageProvider = _iCanApplyDamage;

            EnsureSpawnedAsset();
        }

        public void ReleaseInstance()
        {
            if (Instance != null)
            {
                Reference.ReleaseInstance(Instance);
                Instance = null;
            }

            isEnabled = false;
        }

        public void EnsureSpawnedAsset()
        {
            if (Reference == null
                || assetParent == null
                || Instance != null)
            {
                return;
            }

            Reference.InstantiateAsync(assetParent).Completed += onAssetInstantiated;
        }

        private void onAssetInstantiated(AsyncOperationHandle<GameObject> _handle)
        {
            if (_handle.Status != AsyncOperationStatus.Succeeded)
            {
                return;
            }

            if (Instance != null)
            {
                Reference.ReleaseInstance(_handle.Result);
                return;
            }

            Instance = _handle.Result;
            Instance.transform.SetParent(assetParent);
            Instance.transform.ResetLocalTransformValues();

            Equipment = Instance.GetComponent<IEquipment>();

            if (Equipment != null)
            {
                Equipment.Initialize(ownerLayer, canApplyDamageProvider);
            }

            Instance.SetActive(isEnabled);
        }
    }

    private readonly Dictionary<AssetReferenceGameObject, AssetReferenceData> availableAssets = new();
    private AssetReferenceData enabledAsset { get; set; } = null;

    public int OwnerLayer { get; private set; } = 0;
    public ICanApplyDamage ICanApplyDamage { get; private set; } = null;

    private void OnDestroy()
    {
        ReleaseAllInstances(true);
    }

    public void Initialize(int _ownerLayer, ICanApplyDamage _canApplyDamage)
    {
        OwnerLayer = _ownerLayer;
        ICanApplyDamage = _canApplyDamage;
    }

    public void AddAssetReference(AssetReferenceGameObject _assetReference, bool _enabled = false)
    {
        if (_assetReference == null)
        {
            Debug.LogError("AssetReference is null.");
            return;
        }

        if (availableAssets.ContainsKey(_assetReference))
        {
            Debug.LogWarning("Asset already tracked.");
            return;
        }

        availableAssets.Add(_assetReference, new AssetReferenceData(_assetReference, transform, OwnerLayer, ICanApplyDamage, _enabled));

        if (_enabled)
        {
            SetEnabledAsset(_assetReference);
        }
    }

    public void RemoveAssetReference(AssetReferenceGameObject _assetReference)
    {
        if (_assetReference == null)
        {
            return;
        }

        if (availableAssets.TryGetValue(_assetReference, out var _data))
        {
            if (enabledAsset == _data)
            {
                enabledAsset = null;
            }

            _data.ReleaseInstance();
            availableAssets.Remove(_assetReference);
        }
    }

    public void SetEnabledAsset(AssetReferenceGameObject _assetReference)
    {
        if (_assetReference == null)
        {
            // Disable current asset
            if (enabledAsset != null)
            {
                enabledAsset.IsEnabled = false;
                enabledAsset = null;
            }

            return;
        }

        if (!availableAssets.TryGetValue(_assetReference, out var _data))
        {
            Debug.LogWarning("Asset not tracked. Adding and enabling it.");
            AddAssetReference(_assetReference, true);
        }
        else
        {
            if (enabledAsset != null && enabledAsset != _data)
            {
                enabledAsset.IsEnabled = false;
            }

            enabledAsset = _data;
            enabledAsset.IsEnabled = true;
        }
    }

    public void ReleaseAllInstances(bool _clearAssignedAssetReferences = false)
    {
        foreach (var _data in availableAssets)
        {
            _data.Value.ReleaseInstance();
        }

        if (_clearAssignedAssetReferences)
        {
            availableAssets.Clear();
        }

        enabledAsset = null;
    }

    public static EquipmentSocket SetupSocket(Transform _parent, string _socketName, Vector3 _localPosition, Quaternion _localRotation)
    {
        if (_parent == null)
        {
            Debug.LogError("Parent transform is null.");
            return null;
        }

        if (_socketName.IsNullEmptyOrWhitespace())
        {
            _socketName = "Socket_New";
        }

        Transform _existing = _parent.Find(_socketName);
        GameObject _socketObject = _existing != null
            ? _existing.gameObject
            : new GameObject(_socketName);

        _socketObject.transform.SetParent(_parent);
        _socketObject.transform.localPosition = _localPosition;
        _socketObject.transform.localRotation = _localRotation;
        _socketObject.transform.localScale = Vector3.one;

        return _socketObject.GetOrAddComponent<EquipmentSocket>(out _);
    }
}
