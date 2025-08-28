using TMPro;
using UnityEngine;
public class TurnCalculations : MonoBehaviour
{
    public GameManager gamemanager;
    public TextMeshProUGUI GDPUI;
    public PlotManager plot;
    public EndScreen endscreen;
    private float currentmoodchange;
    private int currentcashchange;
    private int currentwoodchange;
    private int currentstonechange;

    void Start()
    {
        GDPUI.text = $" {0} / 182";
        GDPUI.color = new Color(1f, 0.6f, 0.55f, 1f);
    }
    // add for current mood change (so that it can collate at the end of the turn)
    public void Addmoodchange(float changeamt)
    {
        currentmoodchange += changeamt;
        Debug.Log("changeamt " + changeamt + " mood! Current Mood Change: " + currentmoodchange);
    }

    // add for current cash change
    public void Addcashchange(int changeamt)
    {
        currentcashchange += changeamt;
    }

    // add for current wood change
    public void Addwoodchange(int changeamt)
    {
        currentwoodchange += changeamt;
    }

    // add for current stone change
    public void Addstonechange(int changeamt)
    {
        currentstonechange += changeamt;
    }

    // change the mood stats
    public void Updatemood()
    {
        // round off to whole number
        float roundedmood = (float)System.Math.Round(currentmoodchange, 0);
        gamemanager.AddMood(roundedmood);
    }

    // change the cash stats
    public void Updatecash()
    {
        gamemanager.AddFunds(currentcashchange);
    }

    // change the wood stats
    public void Updatewood()
    {
        gamemanager.AddWood(currentwoodchange);
    }

    // change the stone stats
    public void Updatestone()
    {
        gamemanager.AddStone(currentstonechange);
    }

    //calculate GDP at the end of turn
    public int GDPcalculator()
    {
        int population = gamemanager.Population;
        if (population == 0) return 0;
        int GDP = Mathf.RoundToInt((float)currentcashchange / population);

        return GDP;
    }
    public void AddBuildingGains(int funds, int wood, int stone, int population)
    {
        currentcashchange += funds;
        currentwoodchange += wood;
        currentstonechange += stone;
    }
    public void Updateall()
    {
        int GDP = GDPcalculator();
        if (gamemanager.CurrentTurn >= gamemanager.MaximumTurn)
        {
            endscreen.ShowEndResult();
        }
        if (GDP >= 182)
        {
            GDPUI.text = $" {GDP} / 182";
            GDPUI.color = new Color(0.625f, 1f, 0.55f, 1f);
        }
        else
        {
            GDPUI.text = $" {GDP} / 182";
            GDPUI.color = new Color(1f, 0.6f, 0.55f, 1f);
        }

        gamemanager.AddFunds(currentcashchange);
        gamemanager.AddWood(currentwoodchange);
        gamemanager.AddStone(currentstonechange);
        float roundedmood = (float)System.Math.Round(currentmoodchange, 0);
        gamemanager.AddMood(roundedmood);
    }

    //reset all values
    public void Turnend()
    {
        currentcashchange = 0;
        currentmoodchange = 0f;
        currentwoodchange = 0;
        currentstonechange = 0;
    }
    public int GetCurrentCashChange()
    {
        return currentcashchange;
    }
}
