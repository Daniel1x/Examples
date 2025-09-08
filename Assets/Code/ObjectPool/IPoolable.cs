public interface IPoolable
{
    void Initialize(IPoolProvider _pool);
    void OnPulledFromPool();
    void ReturnToPool();
}