using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Manager Settings")]
    public static GameManager Instance;
    public int ViewingQuadrant; // Determine which Quadrant is currently in view
    public int Funds, Wood, Stone, Metal, Population; // Game Resources
    public float Mood;

    //Turns
    [SerializeField] private ScriptableTurns scriptableTurns;
    private int currentTurn;
    private int buildTurn;
    private float currentTurnTime;
    private float initialTurnTimeCount;
    public int CurrentTurn { get { return currentTurn; } }
    public int BuildTurn { get { return buildTurn; } }
    public float CurrentTurnTime { get { return currentTurnTime; } }
    // Awake is called before the application starts
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        initialTurnTimeCount = scriptableTurns.TimeForEachTurn;
        currentTurnTime = initialTurnTimeCount;
        currentTurn = scriptableTurns.StartingTurn;
    }
    private void Update()
    {
        UpdateTurns();
    }

    // Global Functions (Add-Remove-Set resources)
    public void AddFunds(int amount)
    {
        Funds += amount;
        Debug.Log("Added " + amount + " funds! Current Funds: $" + Funds);
    }

    public void RemoveFunds(int amount)
    {
        Funds -= amount;
        if (Funds < 0) Funds = 0;
        Debug.Log("Removed " + amount + " funds! Current Funds $: " + Funds);
    }

    public void SetFunds(int amount)
    {
        Funds = amount;
        Debug.Log("Set funds to: $" + Funds);
    }

    public void AddWood(int amount)
    {
        Wood += amount;
        Debug.Log("Added " + amount + " wood! Current Wood: " + Wood);
    }

    public void RemoveWood(int amount)
    {
        Wood -= amount;
        if (Wood < 0) Wood = 0;
        Debug.Log("Removed " + amount + " wood! Current Wood: " + Wood);
    }

    public void SetWood(int amount)
    {
        Wood = amount;
        Debug.Log("Set wood to: " + Wood);
    }

    public void AddStone(int amount)
    {
        Stone += amount;
        Debug.Log("Added " + amount + " stone! Current Stone: " + Stone);
    }

    public void RemoveStone(int amount)
    {
        Stone -= amount;
        if (Stone < 0) Stone = 0;
        Debug.Log("Removed " + amount + " stone! Current Stone: " + Stone);
    }

    public void SetStone(int amount)
    {
        Stone = amount;
        Debug.Log("Set stone to: " + Stone);
    }

    public void AddMetal(int amount)
    {
        Metal += amount;
        Debug.Log("Added " + amount + " metal! Current Metal: " + Metal);
    }

    public void RemoveMetal(int amount)
    {
        Metal -= amount;
        if (Metal < 0) Metal = 0;
        Debug.Log("Removed " + amount + " metal! Current Metal: " + Metal);
    }

    public void SetMetal(int amount)
    {
        Metal = amount;
        Debug.Log("Set metal to: " + Metal);
    }

    public void AddPopulation(int amount)
    {
        Population += amount;
        Debug.Log("Added " + amount + " population! Current Population: " + Population);
    }

    public void RemovePopulation(int amount)
    {
        Population -= amount;
        if (Population < 0) Population = 0;
        Debug.Log("Removed " + amount + " population! Current Population: " + Population);
    }

    public void SetPopulation(int amount)
    {
        Population = amount;
        Debug.Log("Set population to: " + Population);
    }

    public void AddMood(float amount)
    {
        Mood += amount;
        Debug.Log("Increased " + amount + " mood! Current Mood: " + Mood);
    }

    public void RemoveMood(float amount)
    {
        Mood -= amount;
        if (Population < 0) Population = 0;
        Debug.Log("Decreased " + amount + " mood! Current Mood: " + Mood);
    }

    public void SetMood(float amount)
    {
        Mood = amount;
        Debug.Log("Set population to: " + Mood);
    }

    // Resource Checker
    public bool HasEnoughFunds(int amount)
    {
        return Funds >= amount;
    }

    public bool HasEnoughWood(int amount)
    {
        return Wood >= amount;
    }

    public bool HasEnoughStone(int amount)
    {
        return Stone >= amount;
    }

    public bool HasEnoughMetal(int amount)
    {
        return Metal >= amount;
    }

    public bool HasEnoughPopulation(int amount)
    {
        return Population >= amount;
    }

    public bool HasEnoughMood(int amount)
    {
        return Mood >= amount;
    }
    private void UpdateTurns()
    {
        if (currentTurn < scriptableTurns.MaxTurns)
        {
            currentTurnTime -= Time.deltaTime;
        }
        if (currentTurnTime <= 0)
        {
            currentTurn++;
            currentTurnTime = initialTurnTimeCount;
        }
    }

    public void EndTurn()
    {
        currentTurnTime = 0;
    }

}