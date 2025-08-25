using UnityEngine;

public abstract class RandomEvents : ScriptableObject
{
    [Header("Random Event Settings")]
    public string eventName;
    public int eventChance;
    public int minTurnTrigger;
    public int maxTurnTrigger;
    public string eventDescription;
    public string option1Text;
    public string option2Text;

    public bool HasPlayerChoices()
    {
        return !string.IsNullOrEmpty(option1Text) || !string.IsNullOrEmpty(option2Text);
    }

    public abstract void TriggerNoChoice();
    public abstract void TriggerOption1();
    public abstract void TriggerOption2();
    public abstract bool CanTriggerOption1();
    public abstract bool CanTriggerOption2();
}