using UnityEngine;

public class TurnCalculations : MonoBehaviour
{
    public GameManager gamemanager;

    private float currentmoodchange;
    private int currentcashchange;

    // add for current mood change (so that it can collate at the end of the turn)
    public void addmoodchange(float changeamt)
    {
        currentmoodchange += changeamt;
        Debug.Log("changeamt " + changeamt + " mood! Current Mood Change: " + currentmoodchange);
    }

    // add for current cash change
    public void addcashcahnge(int changeamt)
    {
        currentcashchange += changeamt;
    }

    // change the mood stats
    public void updatemoode()
    {
        // round off to whole number
        float roundedmood = (float)System.Math.Round(currentmoodchange, 0);
        gamemanager.AddMood(roundedmood);
    }

    // change the cash stats
    public void updatecash()
    {
        gamemanager.AddFunds(currentcashchange);
    }
    
}
