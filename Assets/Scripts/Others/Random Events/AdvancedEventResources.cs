using UnityEngine;

[CreateAssetMenu(fileName = "New Random Event", menuName = "Random Events/Advanced Random Event")]
public class AdvancedEventResources : RandomEvents
{
    [Header("Building Settings")]
    public string targetBuildingName = "";

    [Header("Option 1 Requirements")]
    public int option1RequiredFunds;
    public int option1RequiredWood;
    public int option1RequiredStone;
    public int option1RequiredMetal;
    public int option1RequiredPopulation;
    public float option1RequiredMood;

    [Header("Option 1 Effects")]
    public CalculationType option1CalculationType;
    public float option1FundsChange;
    public float option1WoodChange;
    public float option1StoneChange;
    public float option1MetalChange;
    public float option1PopulationChange;
    public float option1MoodChange;

    [Header("Option 2 Requirements")]
    public int option2RequiredFunds;
    public int option2RequiredWood;
    public int option2RequiredStone;
    public int option2RequiredMetal;
    public int option2RequiredPopulation;
    public float option2RequiredMood;

    [Header("Option 2 Effects")]
    public CalculationType option2CalculationType;
    public float option2FundsChange;
    public float option2WoodChange;
    public float option2StoneChange;
    public float option2MetalChange;
    public float option2PopulationChange;
    public float option2MoodChange;

    [Header("No Choice Effects")]
    public CalculationType noChoiceCalculationType;
    public float noChoiceFundsChange;
    public float noChoiceWoodChange;
    public float noChoiceStoneChange;
    public float noChoiceMetalChange;
    public float noChoicePopulationChange;
    public float noChoiceMoodChange;

    public enum CalculationType
    {
        FlatPerBuilding,        // Fixed amount per building
        PercentagePerBuilding,  // Percentage of current resource per building
        FlatAmount              // Fixed amount ignoring building
    }

    public override void TriggerNoChoice()
    {
        ApplyResourceChanges(noChoiceCalculationType, noChoiceFundsChange, noChoiceWoodChange, noChoiceStoneChange, noChoiceMetalChange, noChoicePopulationChange, noChoiceMoodChange);
    }

    public override void TriggerOption1()
    {
        if (CanTriggerOption1())
            ApplyResourceChanges(option1CalculationType, option1FundsChange, option1WoodChange, option1StoneChange, option1MetalChange, option1PopulationChange, option1MoodChange);
        else
            GameManager.Instance.RemoveMood(10);
    }

    public override void TriggerOption2()
    {
        if (CanTriggerOption2())
            ApplyResourceChanges(option2CalculationType, option2FundsChange, option2WoodChange, option2StoneChange, option2MetalChange, option2PopulationChange, option2MoodChange);
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
    void ApplyResourceChanges(CalculationType calcType, float funds, float wood, float stone, float metal, float population, float mood)
    {
        float finalFunds = CalculateEffect(calcType, funds, "Funds");
        float finalWood = CalculateEffect(calcType, wood, "Wood");
        float finalStone = CalculateEffect(calcType, stone, "Stone");
        float finalMetal = CalculateEffect(calcType, metal, "Metal");
        float finalPopulation = CalculateEffect(calcType, population, "Population");
        float finalMood = CalculateEffect(calcType, mood, "Mood");

        ApplyResourceEffect(finalFunds, finalWood, finalStone, finalMetal, finalPopulation, finalMood);
    }

    // Apply resource effect after the triggered event has ended
    void ApplyResourceEffect(float funds, float wood, float stone, float metal, float population, float mood)
    {
        // Funds Resource Change
        if (funds != 0)
        {
            int roundedFunds = Mathf.CeilToInt(Mathf.Abs(funds));
            if (funds > 0) GameManager.Instance.AddFunds(roundedFunds);
            else GameManager.Instance.RemoveFunds(roundedFunds);
        }

        // Wood Resource Change
        if (wood != 0)
        {
            int roundedWood = Mathf.CeilToInt(Mathf.Abs(wood));
            if (wood > 0) GameManager.Instance.AddWood(roundedWood);
            else GameManager.Instance.RemoveWood(roundedWood);
        }

        // Stone Resource Change
        if (stone != 0)
        {
            int roundedStone = Mathf.CeilToInt(Mathf.Abs(stone));
            if (stone > 0) GameManager.Instance.AddStone(roundedStone);
            else GameManager.Instance.RemoveStone(roundedStone);
        }

        // Metal Resource Change
        if (metal != 0)
        {
            int roundedMetal = Mathf.CeilToInt(Mathf.Abs(metal));
            if (metal > 0) GameManager.Instance.AddMetal(roundedMetal);
            else GameManager.Instance.RemoveMetal(roundedMetal);
        }

        // Population Resource Change
        if (population != 0)
        {
            int roundedPopulation = Mathf.CeilToInt(Mathf.Abs(population));
            if (population > 0) GameManager.Instance.AddPopulation(roundedPopulation);
            else GameManager.Instance.RemovePopulation(roundedPopulation);
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

    // Count how many buildings match the target name
    int CountBuildings()
    {
        int count = 0;

        // Find the Game Environment parent and search all 4 quadrants
        GameObject gameEnvironment = GameObject.Find("Game Environment");
        if (gameEnvironment != null)
        {
            foreach (Transform quadrant in gameEnvironment.transform)
            {
                if (quadrant.name.StartsWith("Grid_Quadrant"))
                {
                    // Count all of the buildings in this quadrant with the specified name
                    foreach (Transform building in quadrant.transform)
                    {
                        if (building.name.StartsWith(targetBuildingName + "_Building"))
                            count++;
                    }
                }
            }
        }

        return count;
    }

    // Calculate effect based on calculation type
    float CalculateEffect(CalculationType calcType, float value, string resourceName)
    {
        if (value == 0) return 0;

        if (calcType == CalculationType.FlatPerBuilding)
        {
            int buildingCount = CountBuildings();
            return buildingCount * value;
        }
        else if (calcType == CalculationType.PercentagePerBuilding)
        {
            int buildingCount = CountBuildings();
            float currentAmount = GetCurrentResourceByName(resourceName);
            return buildingCount * (currentAmount * value / 100f);
        }
        else
        {
            return value;
        }
    }

    // Get the current resource by name
    float GetCurrentResourceByName(string resourceName)
    {
        switch (resourceName)
        {
            case "Funds": return GameManager.Instance.Funds;
            case "Wood": return GameManager.Instance.Wood;
            case "Stone": return GameManager.Instance.Stone;
            case "Metal": return GameManager.Instance.Metal;
            case "Population": return GameManager.Instance.Population;
            case "Mood": return GameManager.Instance.Mood;
            default: return 0f;
        }
    }
}