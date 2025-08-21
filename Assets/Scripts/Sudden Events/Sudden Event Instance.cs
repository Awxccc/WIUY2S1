using System.ComponentModel;
using UnityEngine;

[System.Serializable]
public class SuddenEventInstance
{
    public SuddenEventData suddeneventData;
    public SuddenEventEffect suddenEventEffect;

    //setting the items to have the sudden event
    public SuddenEventInstance(SuddenEventData suddeneventData, SuddenEventEffect suddenEventEffect)
    {
        this.suddeneventData = suddeneventData;
        this.suddenEventEffect = suddenEventEffect;
    }
}
