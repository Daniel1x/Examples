using UnityEngine;

public interface IPoolBehaviour<T> where T : MonoBehaviour, IPoolable
{
    public void InitializePool();
    public void AddNewInstance();
    public T GetObjectFromPool(bool activate);
}
