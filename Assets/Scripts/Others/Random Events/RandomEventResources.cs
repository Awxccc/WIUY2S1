using UnityEngine;

[CreateAssetMenu(fileName = "New Random Event", menuName = "Random Events/Simple Random Event")]
public class RandomEventResources : RandomEvents
{
    [Header("Option 1 Requirements")]
    public int option1RequiredFunds;
    public int option1RequiredWood;
    public int option1RequiredStone;
    public int option1RequiredMetal;
    public int option1RequiredPopulation;
    public float option1RequiredMood;

    [Header("Option 1 Effects")]
    public int option1FundsChange;
    public int option1WoodChange;
    public int option1StoneChange;
    public int option1MetalChange;
    public int option1PopulationChange;
    public float option1MoodChange;

    [Header("Option 2 Requirements")]
    public int option2RequiredFunds;
    public int option2RequiredWood;
    public int option2RequiredStone;
    public int option2RequiredMetal;
    public int option2RequiredPopulation;
    public float option2RequiredMood;

    [Header("Option 2 Effects")]
    public int option2FundsChange;
    public int option2WoodChange;
    public int option2StoneChange;
    public int option2MetalChange;
    public int option2PopulationChange;
    public float option2MoodChange;

    [Header("No Choice Effects")]
    public int noChoiceFundsChange;
    public int noChoiceWoodChange;
    public int noChoiceStoneChange;
    public int noChoiceMetalChange;
    public int noChoicePopulationChange;
    public float noChoiceMoodChange;

    public override void TriggerNoChoice()
    {
        ApplyResourceChanges(noChoiceFundsChange, noChoiceWoodChange, noChoiceStoneChange, noChoiceMetalChange, noChoicePopulationChange, noChoiceMoodChange);
    }

    public override void TriggerOption1()
    {
        if (CanTriggerOption1())
            ApplyResourceChanges(option1FundsChange, option1WoodChange, option1StoneChange, option1MetalChange, option1PopulationChange, option1MoodChange);
        else
            GameManager.Instance.RemoveMood(10);
    }

    public override void TriggerOption2()
    {
        if (CanTriggerOption2())
            ApplyResourceChanges(option2FundsChange, option2WoodChange, option2StoneChange, option2MetalChange, option2PopulationChange, option2MoodChange);
        else
            GameManager.Instance.RemoveMood(10);
    }

    public override bool CanTriggerOption1()
    {
        return HasEnoughResources(option1RequiredFunds, option1RequiredWood, option1RequiredStone, option1RequiredMetal, option1RequiredPopulation, option1RequiredMood);
    }

    public override bool CanTriggerOption2()
    {
        return HasEnoughResources(option2RequiredFunds, option2RequiredWood, option2RequiredStone, option2RequiredMetal, option2RequiredPopulation, option2RequiredMood);
    }

    // Apply resource changes after the triggered event has ended
    void ApplyResourceChanges(int funds, int wood, int stone, int metal, int population, float mood)
    {
        // Funds Resource Change
        if (funds != 0)
        {
            if (funds > 0) GameManager.Instance.AddFunds(funds);
            else GameManager.Instance.RemoveFunds(-funds);
        }

        // Wood Resource Change
        if (wood != 0)
        {
            if (wood > 0) GameManager.Instance.AddWood(wood);
            else GameManager.Instance.RemoveWood(-wood);
        }

        // Stone Resource Change
        if (stone != 0)
        {
            if (stone > 0) GameManager.Instance.AddStone(stone);
            else GameManager.Instance.RemoveStone(-stone);
        }

        // Metal Resource Change
        if (metal != 0)
        {
            if (metal > 0) GameManager.Instance.AddMetal(metal);
            else GameManager.Instance.RemoveMetal(-metal);
        }

        // Population Resource Change
        if (population != 0)
        {
            if (population > 0) GameManager.Instance.AddPopulation(population);
            else GameManager.Instance.RemovePopulation(-population);
        }

        // Mood Resource Change
        if (mood != 0)
        {
            if (mood > 0) GameManager.Instance.AddMood(mood);
            else GameManager.Instance.RemoveMood(-mood);
        }
    }

    // Check to see if the player has enough resources for the selected event options
    bool HasEnoughResources(int funds, int wood, int stone, int metal, int population, float mood)
    {
        return GameManager.Instance.HasEnoughFunds(funds) &&
               GameManager.Instance.HasEnoughWood(wood) &&
               GameManager.Instance.HasEnoughStone(stone) &&
               GameManager.Instance.HasEnoughMetal(metal) &&
               GameManager.Instance.HasEnoughPopulation(population) &&
               GameManager.Instance.HasEnoughMood(mood);
    }
}