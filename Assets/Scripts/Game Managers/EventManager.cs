using UnityEngine;

public class EventManager : MonoBehaviour
{
    [System.Serializable]
    public class EventData
    {
        [Header("Event Data Settings")]
        public string EventName;
        public int EventChance;
        public int MinTurnTrigger;
        public int MaxTurnTrigger;
    }

    [Header("Event Manager Settings")]
    public static EventManager Instance;
    public EventData[] allEvents;

    // Awake is called before the application starts
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Check for random events every turn
    public void CheckForEvents()
    {
        if (GameManager.Instance == null) return;

        int currentTurn = GameManager.Instance.CurrentTurn;

        // Go through each event and check if it should trigger
        foreach (EventData eventData in allEvents)
        {
            CheckEventToTrigger(eventData, currentTurn);
        }
    }

    // Go through the selected event to determine if it can be triggered
    void CheckEventToTrigger(EventData eventData, int currentTurn)
    {
        // Check to see if the event is within the mix and max range of turns to be allowed to trigger
        if (currentTurn < eventData.MinTurnTrigger || currentTurn > eventData.MaxTurnTrigger)
        {
            return;
        }

        // If the roll succeeds, the event will be triggered
        if (Random.Range(1, eventData.EventChance + 1) == 1)
        {
            TriggerEvent(eventData);
        }
    }

    // Trigger the selected event
    void TriggerEvent(EventData eventData)
    {
        Debug.Log($"{eventData.EventName} has been triggered!");
    }

    // Force the manager to roll another event to trigger
    public void ForceEventTrigger()
    {
        CheckForEvents();
    }
}