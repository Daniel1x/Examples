using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "eventData_New", menuName = "ScriptableObjects/Animations/Animation Event Data", order = 1)]
public class AnimationEventData : ScriptableObject
{
    [System.Serializable]
    public class Action
    {
        [SerializeField] private ActionType actionType = default;
        [SerializeField] private string socketName = string.Empty;

        public ActionType ActionType => actionType;
        public string SocketName => socketName;
    }

    [SerializeField] private Action[] actions = new Action[] { };

    public IReadOnlyList<Action> Actions => actions;
}
