using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Manager Settings")]
    public static GameManager Instance;
    public List<BuildingProgress> allBuildings = new();
    public TurnCalculations turnCalculations;
    public Trading trading;
    public int ViewingQuadrant, Funds, Wood, Stone, Metal, Population;
    public float Mood;

    [SerializeField] private int currentTurn;
    [SerializeField] private int maximumTurn;
    [SerializeField] private float currentTurnTime;
    [SerializeField] private float initialTurnTimeCount;

    public int CurrentTurn => currentTurn;
    public int MaximumTurn => maximumTurn;
    public float CurrentTurnTime => currentTurnTime;

    // Awake is called before the application starts
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        currentTurn = 1;
        currentTurnTime = initialTurnTimeCount;
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateTurns();
    }

    // Resource Management
    public void AddFunds(int amount) { Funds += amount; Debug.Log($"Added {amount} funds! Current: {Funds}"); }
    public void RemoveFunds(int amount) { Funds = Mathf.Max(0, Funds - amount); Debug.Log($"Removed {amount} funds! Current: {Funds}"); }
    public void SetFunds(int amount) { Funds = amount; Debug.Log($"Set funds to: {Funds}"); }

    public void AddWood(int amount) { Wood += amount; Debug.Log($"Added {amount} wood! Current: {Wood}"); }
    public void RemoveWood(int amount) { Wood = Mathf.Max(0, Wood - amount); Debug.Log($"Removed {amount} wood! Current: {Wood}"); }
    public void SetWood(int amount) { Wood = amount; Debug.Log($"Set wood to: {Wood}"); }

    public void AddStone(int amount) { Stone += amount; Debug.Log($"Added {amount} stone! Current: {Stone}"); }
    public void RemoveStone(int amount) { Stone = Mathf.Max(0, Stone - amount); Debug.Log($"Removed {amount} stone! Current: {Stone}"); }
    public void SetStone(int amount) { Stone = amount; Debug.Log($"Set stone to: {Stone}"); }

    public void AddMetal(int amount) { Metal += amount; Debug.Log($"Added {amount} metal! Current: {Metal}"); }
    public void RemoveMetal(int amount) { Metal = Mathf.Max(0, Metal - amount); Debug.Log($"Removed {amount} metal! Current: {Metal}"); }
    public void SetMetal(int amount) { Metal = amount; Debug.Log($"Set metal to: {Metal}"); }

    public void AddPopulation(int amount) { Population += amount; Debug.Log($"Added {amount} population! Current: {Population}"); }
    public void RemovePopulation(int amount) { Population = Mathf.Max(0, Population - amount); Debug.Log($"Removed {amount} population! Current: {Population}"); }
    public void SetPopulation(int amount) { Population = amount; Debug.Log($"Set population to: {Population}"); }

    public void AddMood(float amount) { Mood += amount; Debug.Log($"Increased {amount} mood! Current: {Mood}"); }
    public void RemoveMood(float amount) { Mood = Mathf.Max(0, Mood - amount); Debug.Log($"Decreased {amount} mood! Current: {Mood}"); }
    public void SetMood(float amount) { Mood = amount; Debug.Log($"Set mood to: {Mood}"); }

    // Resource Checker
    public bool HasEnoughFunds(int amount) => Funds >= amount;
    public bool HasEnoughWood(int amount) => Wood >= amount;
    public bool HasEnoughStone(int amount) => Stone >= amount;
    public bool HasEnoughMetal(int amount) => Metal >= amount;
    public bool HasEnoughPopulation(int amount) => Population >= amount;
    public bool HasEnoughMood(float amount) => Mood >= amount;

    private void UpdateTurns()
    {
        if (currentTurn < maximumTurn)
        {
            currentTurnTime -= Time.deltaTime;
            if (currentTurnTime <= 0)
            {
                AdvanceTurn();
            }
        }
    }

    private void AdvanceTurn()
    {
        currentTurn++;
        currentTurnTime = initialTurnTimeCount;

        if (trading != null)
        {
            trading.Gettradeamt();
        }

        if (EventManager.Instance != null)
            EventManager.Instance.CheckForEvents();

        if (turnCalculations != null)
        {//Loop through all placed buildings
            foreach (BuildingProgress building in allBuildings)
            {
                if (building != null && building.PlotData != null && building.IsComplete)
                {//Check if buildings are valid and finished construction then gain resources
                    turnCalculations.AddBuildingGains(building.PlotData.GainFunds, building.PlotData.GainWood, building.PlotData.GainStone, 0);
                }
            }
            turnCalculations.Updateall();
            turnCalculations.Turnend();
        }

        foreach (BuildingProgress b in allBuildings)
        {
            if (b != null)
            {//Progress building construction
                b.BuildTurn();
            }
        }
    }

    public void SkipTurn()
    {
        AdvanceTurn();
    }
}