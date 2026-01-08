using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This class doesn't do much, but it shows the style in which I write code.
/// </summary>
public class ExampleBehaviourClass : MonoBehaviour
{
    #region Data types, structs and classes

    public interface IEnumIdContainer<TEnum> where TEnum : System.Enum
    {
        TEnum Id { get; set; }
    }

    public enum DataState
    {
        Default = 0,
        State1 = 1,
        State2 = 2,
    }

    [System.Serializable]
    public struct ExampleStruct<TData> : IEnumIdContainer<DataState>
    {
        [SerializeField] private DataState dataState;
        [SerializeField] private TData data;

        public DataState Id
        {
            get => dataState;
            set => dataState = value;
        }

        public TData Data => data;
    }

    [System.Serializable]
    public class ExampleData<TEnum, TData> : IEnumIdContainer<TEnum> where TEnum : System.Enum
    {
        public string Description = string.Empty;

        [SerializeField] private TEnum id = default;
        [SerializeField] private TData data = default;

        public TEnum Id
        {
            get => id;
            set => id = value;
        }

        public TData Data => data;
    }

    public abstract class SequenceContainer : ScriptableObject
    {
        public abstract string Description { get; }
        public abstract IEnumerator Perform();
    }

    #endregion

    public static event UnityAction<ExampleBehaviourClass> OnNewBehaviourCreated;
    public static event UnityAction<ExampleBehaviourClass> OnSequenceStarted;
    public static event UnityAction<ExampleBehaviourClass> OnSequenceFinished;

    private static ExampleBehaviourClass lastActiveInstance;

    public static ExampleBehaviourClass LastActiveInstance => lastActiveInstance;

    public ExampleStruct<SequenceContainer> Struct = new();

    [Header("Data")]
    [SerializeField] private ExampleData<DataState, float> someFloatData = new();
    [SerializeField] private ExampleData<DataState, int> someIntData = new();

    [Header("Sequences")]
    [SerializeField] private SequenceContainer[] startSequences = new SequenceContainer[] { };
    [ReadOnlyProperty, SerializeField] private string activeSequenceDescription = string.Empty;

    private Coroutine startSequenceRoutine;

    public ExampleData<DataState, float> SomeFloatData => someFloatData;
    public ExampleData<DataState, int> SomeIntData => someIntData;
    public bool IsStartSequenceInProgress => startSequenceRoutine != null;

    public string SequenceDescription
    {
        get => activeSequenceDescription;
        private set
        {
            activeSequenceDescription = value;

#if UNITY_EDITOR
            if (!string.IsNullOrWhiteSpace(value))
            {
                MyLog.Log($"New active sequence: {activeSequenceDescription}");
            }
#endif
        }
    }

    private void Awake()
    {
        if (lastActiveInstance == null)
        {
            lastActiveInstance = this;
        }

        OnNewBehaviourCreated?.Invoke(this);
    }

    private void OnDestroy()
    {
        if (lastActiveInstance == this)
        {
            lastActiveInstance = null;
        }
    }

    private void OnEnable()
    {
        lastActiveInstance = this;
        restartStartSequence();
    }

    private void OnDisable()
    {
        stopStartSequence();
    }

    public IEnumerator WaitForStartSequenceToComplete()
    {
        while (IsStartSequenceInProgress)
        {
            yield return null;
        }
    }

    private void stopStartSequence()
    {
        if (startSequenceRoutine != null)
        {
            StopCoroutine(startSequenceRoutine);
            startSequenceRoutine = null;
        }

        SequenceDescription = string.Empty;
    }

    private void restartStartSequence(params SequenceContainer[] _additionalSequences)
    {
        stopStartSequence();

        startSequenceRoutine = StartCoroutine(HandleStart());

        IEnumerator HandleStart()
        {
            OnSequenceStarted?.Invoke(this);

            yield return _handleSequences(startSequences);

            if (_additionalSequences != null)
            {
                yield return _handleSequences(_additionalSequences);
            }

            startSequenceRoutine = null;
            SequenceDescription = string.Empty;

            OnSequenceFinished?.Invoke(this);
        }

        IEnumerator _handleSequences(SequenceContainer[] _sequences)
        {
            yield return _sequences.HandleSequences(_seq => SequenceDescription = _seq.Description);
        }
    }
}

public static class ExampleBehaviourClassExtensions
{
    public static IEnumerator HandleSequences(
        this ExampleBehaviourClass.SequenceContainer[] _sequences,
        System.Action<ExampleBehaviourClass.SequenceContainer> _onSequenceStarted = null)
    {
        if (_sequences == null)
        {
            yield break;
        }

        foreach (var _sequence in _sequences)
        {
            if (_sequence == null)
            {
                continue;
            }

            _onSequenceStarted?.Invoke(_sequence);

            yield return _sequence.Perform();
        }
    }
}
