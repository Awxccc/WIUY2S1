using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Manager Settings")]
    public static GameManager Instance;
    public int ViewingQuadrant;

    // Game Resources
    public int Funds, Wood, Stone, Metal, Population;
    public float Mood;

    // Turn System
    [SerializeField] private int currentTurn;
    [SerializeField] private int maximumTurn;
    [SerializeField] private float currentTurnTime;
    [SerializeField] private float initialTurnTimeCount;

    public int CurrentTurn => currentTurn;
    public float CurrentTurnTime => currentTurnTime;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        currentTurn = 1;
        currentTurnTime = initialTurnTimeCount;
    }

    private void Update()
    {
        UpdateTurns();
    }

    // =============================
    // Resource Management
    // =============================

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

        if (EventManager.Instance != null)
            EventManager.Instance.CheckForEvents();

        BuildingProgress[] buildings = FindObjectsByType<BuildingProgress>(FindObjectsSortMode.None);
        foreach (var b in buildings)
        {
            b.BuildTurn();
        }
    }

    public void SkipTurn()
    {
        AdvanceTurn();
    }
}
