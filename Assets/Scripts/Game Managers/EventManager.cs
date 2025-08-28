using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventManager : MonoBehaviour
{
    [Header("Event Manager Settings")]
    public static EventManager Instance;
    public RandomEvents[] randomEventsList;

    [Header("UI Event References")]
    public GameObject UIEventViewer;
    public TextMeshProUGUI EventNameText;
    public TextMeshProUGUI EventInfoText;
    public Button OptionOneBtn;
    public Button OptionTwoBtn;
    public Button CloseBtn;
    public TextMeshProUGUI OptionOneBtnText;
    public TextMeshProUGUI OptionTwoBtnText;
    public Image EventImageIcon;

    private RandomEvents currentActiveEvent;

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
        // Hide the 'UI-EventViewer' on load
        UIEventViewer.SetActive(false);
    }

    // Check for random events every turn
    public void CheckForEvents()
    {
        if (GameManager.Instance == null) return;

        int currentTurn = GameManager.Instance.CurrentTurn;

        // Go through each event and check if it should trigger
        foreach (RandomEvents eventData in randomEventsList)
            CheckEventToTrigger(eventData, currentTurn);
    }

    // Go through the selected event to determine if it can be triggered
    void CheckEventToTrigger(RandomEvents eventData, int currentTurn)
    {
        // Check to see if the event is within the min and max range of turns
        if (currentTurn < eventData.minTurnTrigger || currentTurn > eventData.maxTurnTrigger)
            return;

        // If the roll succeeds, the event will be triggered
        if (Random.Range(1, eventData.eventChance + 1) == 1)
            TriggerEvent(eventData);
    }

    // Trigger the selected event
    void TriggerEvent(RandomEvents eventData)
    {
        // Store current active event & update the UI with event data
        currentActiveEvent = eventData;
        EventNameText.text = eventData.eventName;
        EventInfoText.text = eventData.eventDescription;
        OptionOneBtnText.text = eventData.option1Text;
        OptionTwoBtnText.text = eventData.option2Text;
        EventImageIcon.sprite = eventData.eventImage;
        UIEventViewer.SetActive(true);

        // Check if we have choices to display or not and also toggle the closeBtn accordingly
        bool hasChoices = eventData.HasPlayerChoices();
        OptionOneBtn.gameObject.SetActive(!string.IsNullOrEmpty(eventData.option1Text));
        OptionTwoBtn.gameObject.SetActive(!string.IsNullOrEmpty(eventData.option2Text));
        CloseBtn.gameObject.SetActive(!hasChoices);

        // If the event has no player choice, execute no-choice effects immediately
        if (!hasChoices)
            eventData.TriggerNoChoice();

        AudioManager.Instance.ForcePlaceSFX(2);
        Debug.Log($"{eventData.eventName} has been triggered!");
    }

    // Handle the option button clicks when an event with options is shown
    public void OnOptionButtonClicked(int buttonNumber)
    {
        if (currentActiveEvent == null) return;

        if (buttonNumber == 1)
            currentActiveEvent.TriggerOption1();
        else if (buttonNumber == 2)
            currentActiveEvent.TriggerOption2();

        CloseEventUI();
    }

    // Close the event UI
    public void CloseEventUI()
    {
        UIEventViewer.SetActive(false);
        currentActiveEvent = null;
    }

    // Force the manager to roll another event to trigger
    public void ForceEventTrigger()
    {
        CheckForEvents();
    }
}