using UnityEngine;
using UnityEngine.InputSystem;

public class BasicInputs : MonoBehaviour
{
    [Header("Character Input Values")]
    public Vector2 Move = default;
    public Vector2 Look = default;
    public bool Jump = false;
    public bool Sprint = false;

    [Header("Movement Settings")]
    public bool AnalogMovement = false;

    [Header("Mouse Cursor Settings")]
    public bool CursorLocked = true;
    public bool CursorInputForLook = true;

    private void OnApplicationFocus(bool _hasFocus) => SetCursorState(CursorLocked);

    public void OnMove(InputValue _input) => MoveInput(_input.Get<Vector2>());

    public void OnLook(InputValue _input)
    {
        if (CursorInputForLook)
        {
            LookInput(_input.Get<Vector2>());
        }
    }

    public void OnJump(InputValue _input) => JumpInput(_input.isPressed);
    public void OnSprint(InputValue _input) => SprintInput(_input.isPressed);

    public void MoveInput(Vector2 _newMoveDirection) => Move = _newMoveDirection;
    public void LookInput(Vector2 _newLookDirection) => Look = _newLookDirection;
    public void JumpInput(bool _newJumpState) => Jump = _newJumpState;
    public void SprintInput(bool _newSprintState) => Sprint = _newSprintState;

    public void SetCursorState(bool _newState)
    {
        Cursor.lockState = _newState
            ? CursorLockMode.Locked
            : CursorLockMode.None;
    }
}
