using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class UnitEquipmentManager : MonoBehaviour
{
    [System.Serializable]
    public class SocketSetup
    {
        [SerializeField] private EquipmentSocket socket = null;

        public IAssetListProvider AssetListProvider { get; private set; } = null;

        public EquipmentSocket Socket
        {
            get => socket;
            private set => socket = value;
        }

        public void Initialize(IAssetListProvider _provider)
        {
            if (socket == null)
            {
                Debug.LogWarning("Socket instance is not assigned.");
                return;
            }

            AssetListProvider = _provider;

            if (AssetListProvider != null)
            {
                List<AssetReferenceGameObject> _assets = AssetListProvider.GetAssets();

                for (int i = 0; i < _assets.Count; i++)
                {
                    Socket.AddAssetReference(_assets[i], false);
                }
            }
        }
    }

    [SerializeField] private SocketSetup rightArmSocket = new();
    [SerializeField] private SocketSetup leftArmSocket = new();
    [SerializeField] private AssetListProvider armWeapons = new();

    private int currentRightArmIndex = -1;
    private int currentLeftArmIndex = -1;

    private void Start()
    {
        InitializeSockets();
    }

    private void OnDestroy()
    {
        ReleaseAllSockets();
    }

    public void InitializeSockets()
    {
        rightArmSocket.Initialize(armWeapons);
        leftArmSocket.Initialize(armWeapons);
    }

    public void ReleaseAllSockets()
    {
        if (rightArmSocket.Socket != null)
        {
            rightArmSocket.Socket.ReleaseAllInstances(true);
        }

        if (leftArmSocket.Socket != null)
        {
            leftArmSocket.Socket.ReleaseAllInstances(true);
        }
    }

    [ActionButton]
    public void ChangeRightArmEquipmentToNext()
    {
        currentRightArmIndex = changeArmEquipment(rightArmSocket, currentRightArmIndex, true);
    }

    [ActionButton]
    public void ChangeRightArmEquipmentToPrevious()
    {
        currentRightArmIndex = changeArmEquipment(rightArmSocket, currentRightArmIndex, false);
    }

    [ActionButton]
    public void ChangeLeftArmEquipmentToNext()
    {
        currentLeftArmIndex = changeArmEquipment(leftArmSocket, currentLeftArmIndex, true);
    }

    [ActionButton]
    public void ChangeLeftArmEquipmentToPrevious()
    {
        currentLeftArmIndex = changeArmEquipment(leftArmSocket, currentLeftArmIndex, false);
    }

    private int changeArmEquipment(SocketSetup _setup, int _current, bool _next)
    {
        if (_setup.Socket == null || _setup.AssetListProvider == null)
        {
            Debug.LogWarning("Socket not initialized.");
            return -1;
        }

        int _newIndex = _setup.AssetListProvider.GetNextAssetIndex(_current, _next);

        if (_newIndex == _current)
        {
            return _current; // No change
        }

        AssetReferenceGameObject _newAsset = _setup.AssetListProvider.GetAsset(_newIndex);
        _setup.Socket.SetEnabledAsset(_newAsset);

        return _newIndex;
    }
}
