using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    public static event UnityAction OnPlayerCountUpdated = null;

    private static List<PlayerController> activePlayerControllers = new List<PlayerController>();

    [SerializeField] private ThirdPersonController thirdPersonController = null;

    private PlayerIndicator playerIndicator = null;

    protected void Awake()
    {
        if (thirdPersonController != null)
        {
            thirdPersonController.OnJumpPerformed += onJump;
        }

        playerIndicator = GetComponentInChildren<PlayerIndicator>(true);
    }

    protected void OnDestroy()
    {
        if (thirdPersonController != null)
        {
            thirdPersonController.OnJumpPerformed -= onJump;
        }
    }

    private void OnEnable()
    {
        activePlayerControllers.Add(this);
        OnPlayerCountUpdated?.Invoke();
    }

    private void OnDisable()
    {
        activePlayerControllers.Remove(this);
        OnPlayerCountUpdated?.Invoke();
    }

    private void onJump(UnitCharacterController _controller)
    {
        GroundAnimator.ShowCircle(transform.position, playerIndicator != null ? playerIndicator.PlayerColor : Color.white);
    }

    public static PlayerController GetClosestPlayer(Vector3 _position)
    {
        PlayerController closestPlayer = null;
        float _closestDistance = float.MaxValue;

        foreach (var _player in activePlayerControllers)
        {
            if (_player == null || _player.transform == null)
            {
                continue;
            }

            float _distance = Vector3.Distance(_position, _player.transform.position);

            if (_distance < _closestDistance)
            {
                _closestDistance = _distance;
                closestPlayer = _player;
            }
        }

        return closestPlayer;
    }
}
