using UnityEngine;

[RequireComponent(typeof(ReflectionProbe)), ExecuteAlways]
public class ReflectionProbeGizmo : MonoBehaviour
{
    public enum DrawMode
    {
        None = 0,
        OnSelected = 1,
        Always = 2,
    }

    [SerializeField, HideInInspector] private ReflectionProbe reflectionProbe = null;
    [SerializeField] private DrawMode mode = DrawMode.OnSelected;
    [SerializeField] private Color gizmoColor = new Color(1f, 0f, 0f, 0.2f);

    private void OnDrawGizmos() => drawGizmos(DrawMode.Always);

    private void OnDrawGizmosSelected() => drawGizmos(DrawMode.OnSelected);

    private void drawGizmos(DrawMode _mode)
    {
        if (_mode != mode)
        {
            return;
        }

        if (reflectionProbe == null)
        {
            reflectionProbe = GetComponent<ReflectionProbe>();

            if (reflectionProbe == null)
            {
                return;
            }
        }

        Color _defaultColor = Gizmos.color;
        Matrix4x4 _defaultMatrix = Gizmos.matrix;

        Matrix4x4 _rotationMatrix = Matrix4x4.TRS(
            reflectionProbe.transform.position,
            reflectionProbe.transform.rotation,
            reflectionProbe.transform.lossyScale
        );

        Gizmos.color = gizmoColor;
        Gizmos.matrix = _rotationMatrix;

        Vector3 _size = reflectionProbe.size;
        Vector3 _position = Vector3.zero;

        Gizmos.DrawCube(_position, _size);

        if (gizmoColor.a < 1f)
        {
            Color _wireColor = gizmoColor;
            _wireColor.a = 1f;
            Gizmos.color = _wireColor;
        }

        Gizmos.DrawWireCube(_position, _size);

        Gizmos.matrix = _defaultMatrix;
        Gizmos.color = _defaultColor;
    }
}
