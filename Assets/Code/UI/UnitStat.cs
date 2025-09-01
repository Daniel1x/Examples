using UnityEngine;

[System.Serializable]
public struct UnitStat
{
    public float Current;
    public float Max;

    public bool IsMax => Current >= Max;
    public float Percent01 => Max > 0f ? Current / Max : 0f;

    public UnitStat(float _max) : this(_max, _max) { }

    public UnitStat(float _current, float _max)
    {
        Current = _current;
        Max = _max;
    }

    public bool ChangeCurrent(float _delta)
    {
        float _previous = Current;
        Current = Mathf.Clamp(Current + _delta, 0f, Max);

        return Current != _previous;
    }
}
