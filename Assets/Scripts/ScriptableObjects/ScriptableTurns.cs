using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableTurns", menuName = "Scriptable Objects/ScriptableTurns")]
public class ScriptableTurns : ScriptableObject
{
    [SerializeField] private int _maxTurns = 120;
    [SerializeField] private float _timeForEachTurn = 20f;
    [SerializeField] private int _startingTurn = 1;

    public int MaxTurns
    {
        get { return _maxTurns; }
    }

    public float TimeForEachTurn
    {
        get { return _timeForEachTurn; }
    }

    public int StartingTurn
    {
        get { return _startingTurn; }
    }
}
