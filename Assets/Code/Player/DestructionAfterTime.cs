using UnityEngine;

public class DestructionAfterTime : MonoBehaviour
{
    [SerializeField] private float lifetime = 1f;

    private float timer = 0f;

    public float Lifetime
    {
        get => lifetime;
        set => lifetime = Mathf.Max(0f, value);
    }

    private void OnEnable()
    {
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= lifetime)
        {
            gameObject.ReleaseAssetInstanceOrDestroy();
        }
    }
}
