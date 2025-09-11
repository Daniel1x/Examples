using UnityEngine;

[RequireComponent(typeof(UnitCharacterController))]
public class PlayerAttackController : MonoBehaviour, IRequiresCharacterInputProvider<PlayerBasicInputs>
{
    private UnitCharacterController characterController = null;

    public PlayerBasicInputs InputProvider { get; set; } = null;

    private void Awake()
    {
        characterController = GetComponent<UnitCharacterController>();
    }

    private void Update()
    {
        if (InputProvider == null || characterController == null)
        {
            return;
        }

        if (characterController.IsAnyActionBehaviourInProgress(out bool _allowMovement))
        {
            return; // If any action is in progress, block attacks
        }

        if (InputProvider.FastAttack)
        {
            characterController.Attack(ActionBehaviour.ActionType.RightSlash);
            InputProvider.FastAttack = false;
        }
        else if (InputProvider.HeavyAttack)
        {
            characterController.Attack(ActionBehaviour.ActionType.AreaSpell);
            InputProvider.HeavyAttack = false;
        }
    }
}
