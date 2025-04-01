using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using static ExampleBehaviourClass;

///<summary>This class doesn't do much, but it shows the style in which I write code.</summary>
public class ExampleBehaviourClass : MonoBehaviour
{
    #region Data types, structs and classes

    public interface EnumIDContainer<EnumType> where EnumType : System.Enum
    {
        public EnumType ID { get; set; }
    }

    public enum DataType
    {
        Default = 0,
        State1 = 1,
        State2 = 2,
    }

    [System.Serializable]
    public struct ExampleStruct<Data> : EnumIDContainer<DataType>
    {
        [SerializeField] private DataType dataType;
        [SerializeField] private Data type;

        public DataType ID { get => dataType; set => dataType = value; }
    }

    [System.Serializable]
    public class ExampleDataClass<EnumType, DataType> : EnumIDContainer<EnumType> where EnumType : System.Enum
    {
        public string Description = string.Empty;

        [SerializeField] private EnumType mode = default;
        [SerializeField] private DataType data = default;

        public EnumType ID { get => mode; set => mode = value; }
        public DataType Data => data;
    }

    public abstract class SequenceContainer : ScriptableObject
    {
        public abstract string Description { get; }
        public abstract IEnumerator Perform();
    }

    #endregion

    public static event UnityAction<ExampleBehaviourClass> OnNewBehaviourCreated = null;
    public static event UnityAction<ExampleBehaviourClass> OnSequenceStarted = null;
    public static event UnityAction<ExampleBehaviourClass> OnSequenceFinished = null;

    private static ExampleBehaviourClass lastActiveInstance = null;

    public ExampleStruct<SequenceContainer> Struct = new();

    [SerializeField] private ExampleDataClass<DataType, float> someFloatData = new();
    [SerializeField] private ExampleDataClass<DataType, int> someIntData = new();

    [Header("Sequences")]
    [SerializeField] private SequenceContainer[] startSequences = new SequenceContainer[] { };
    [ReadOnlyProperty, SerializeField] private string activeSequenceDescription = string.Empty;

    private Coroutine startSequenceRoutine = null;

    public ExampleDataClass<DataType, float> SomeFloatData => someFloatData;
    public ExampleDataClass<DataType, int> SomeIntData => someIntData;
    public bool IsStartSequenceInProgress => startSequenceRoutine != null;

    public string SequenceDescription
    {
        get => activeSequenceDescription;
        private set
        {
            activeSequenceDescription = value;

#if UNITY_EDITOR
            if (value.IsNullEmptyOrWhitespace() == false)
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

    private void OnEnable()
    {
        lastActiveInstance = this;
        restartStartSequence();
    }

    private void OnDisable()
    {
        stopStartSequence();
    }

    public IEnumerator WaitForStartSequenceToCompltete(bool _useWaiter)
    {
        if (IsStartSequenceInProgress == false)
        {
            yield break;
        }

        if (_useWaiter)
        {
            yield return new WaitWhile(() => IsStartSequenceInProgress);
        }
        else
        {
            while (IsStartSequenceInProgress)
            {
                yield return null;
            }
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

        startSequenceRoutine = StartCoroutine(_handleStart());

        IEnumerator _handleStart()
        {
            OnSequenceStarted?.Invoke(this);

            _handleSequences(startSequences);

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
    public static IEnumerator HandleSequences(this SequenceContainer[] _sequences, System.Action<SequenceContainer> _onSequenceStarted = null)
    {
        if (_sequences == null)
        {
            yield break;
        }

        foreach (SequenceContainer _sequence in _sequences)
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
