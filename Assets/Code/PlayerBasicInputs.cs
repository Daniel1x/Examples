using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBasicInputs : CharacterInputProvider
{
    [Header("Character Input Values")]
    [SerializeField, ReadOnlyProperty] private Vector2 move = default;
    [SerializeField, ReadOnlyProperty] private bool jump = false;
    [SerializeField, ReadOnlyProperty] private bool sprint = false;
    [SerializeField, ReadOnlyProperty] private bool changeRightWeapon = false;
    [SerializeField, ReadOnlyProperty] private bool changeLeftWeapon = false;

    [Header("Movement Settings")]
    [SerializeField] private bool analogMovement = false;

    public override Vector2 Move => move;
    public override bool Jump { get => jump; set => jump = value; }
    public override bool Sprint => sprint;
    public override bool AnalogMovement => analogMovement;
    public override bool ChangeRightArmWeapon { get => changeRightWeapon; set => changeRightWeapon = value; }
    public override bool ChangeLeftArmWeapon { get => changeLeftWeapon; set => changeLeftWeapon = value; }

    public void OnMove(InputValue _input) => MoveInput(_input.Get<Vector2>());
    public void OnJump(InputValue _input) => JumpInput(_input.isPressed);
    public void OnSprint(InputValue _input) => SprintInput(_input.isPressed);
    public void OnChangeRightWeapon(InputValue _input) => ChangeRightWeaponInput(_input.isPressed);
    public void OnChangeLeftWeapon(InputValue _input) => ChangeLeftWeaponInput(_input.isPressed);

    public void MoveInput(Vector2 _newMoveDirection) => move = _newMoveDirection;
    public void JumpInput(bool _newJumpState) => jump = _newJumpState;
    public void SprintInput(bool _newSprintState) => sprint = _newSprintState;
    public void ChangeRightWeaponInput(bool _newChangeState) => changeRightWeapon = _newChangeState;
    public void ChangeLeftWeaponInput(bool _newChangeState) => changeLeftWeapon = _newChangeState;
}
