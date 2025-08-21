using UnityEngine;

[CreateAssetMenu(fileName = "Fire", menuName = "Scriptable Objects/Fire")]
public class Fire : SuddenEventEffect
{
    //do the effecting of the stuff here
    public override void Effect(GameObject user)
    {
        var turncalc = user.GetComponent<TurnCalculations>();
        turncalc.addmoodchange(-10f);
    }
}
