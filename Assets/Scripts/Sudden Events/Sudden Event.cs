using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SuddenEvent", menuName = "Scriptable Objects/SuddenEvent")]
public class SuddenEvent : ScriptableObject
{
    public int maxEvents = 10;
    public List<SuddenEventInstance> Events = new List<SuddenEventInstance>();

    //for the randomiser to know how much to split between
    public int Itemcount()
    {
        return Events.Count;
    }

    public SuddenEventInstance GetEvent(int index)
    {
        return Events[index];
    }
}
