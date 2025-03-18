using UnityEngine;

public class RigidBodyPush : MonoBehaviour
{
    public LayerMask PushLayers = default;
    public bool CanPush = true;
    [Range(0.1f, 10f)] public float Strength = 1.1f;

    private void OnControllerColliderHit(ControllerColliderHit _hit)
    {
        if (CanPush)
        {
            PushRigidBodies(_hit);
        }
    }

    public void PushRigidBodies(ControllerColliderHit _hit)
    {
        Rigidbody _body = _hit.collider.attachedRigidbody;

        if (_body == null || _body.isKinematic)
        {
            return; //Make sure we hit a non kinematic rigidbody
        }

        int _bodyLayerMask = 1 << _body.gameObject.layer;

        if ((_bodyLayerMask & PushLayers.value) == 0)
        {
            return; //Make sure we only push desired layer(s)
        }

        if (_hit.moveDirection.y < -0.3f)
        {
            return; // We dont want to push objects below us
        }

        // Calculate push direction from move direction, horizontal motion only
        Vector3 _pushDir = new Vector3(_hit.moveDirection.x, 0f, _hit.moveDirection.z);

        _body.AddForce(_pushDir * Strength, ForceMode.Impulse);
    }
}
