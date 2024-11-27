using UnityEngine;
using UnityEngine.UI;

public class PlayerView : MonoBehaviour
{
    [field: SerializeField] public RawImage TextureTarget { get; private set; } = null;
    [field: SerializeField] public GameObject Selection { get; private set; } = null;

    public RectTransform RectTransform { get; private set; } = null;

    private void Awake()
    {
        RectTransform = (RectTransform)transform;
        SetAsSelected(false);
    }

    public void SetAsSelected(bool _selected)
    {
        Selection.SetActive(_selected);
    }
}
