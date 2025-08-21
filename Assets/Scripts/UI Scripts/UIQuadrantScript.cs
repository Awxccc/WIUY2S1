using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIQuadrantScript : MonoBehaviour
{
    [Header("UI References")]
    public Image ImageMain;
    public GameObject[] QuadrantImages = new GameObject[4];
    public Button[] QuadrantButtons = new Button[4];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ImageMain.color = Color.white;

        AddHoverToButton(0);
        AddHoverToButton(1);
        AddHoverToButton(2);
        AddHoverToButton(3);
    }

    void AddHoverToButton(int buttonIndex)
    {
        Button button = QuadrantButtons[buttonIndex];
        if (button == null) return;

        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry enterEvent = new EventTrigger.Entry();
        enterEvent.eventID = EventTriggerType.PointerEnter;
        enterEvent.callback.AddListener((data) => { ButtonHoverActive(buttonIndex); });
        trigger.triggers.Add(enterEvent);

        EventTrigger.Entry exitEvent = new EventTrigger.Entry();
        exitEvent.eventID = EventTriggerType.PointerExit;
        exitEvent.callback.AddListener((data) => { ButtonHoverUnactive(); });
        trigger.triggers.Add(exitEvent);

        EventTrigger.Entry clickEvent = new EventTrigger.Entry();
        clickEvent.eventID = EventTriggerType.PointerClick;
        clickEvent.callback.AddListener((data) => { ButtonClicked(buttonIndex); });
        trigger.triggers.Add(clickEvent);
    }

    void ButtonClicked(int buttonIndex)
    {
        int quadrantNumber = buttonIndex + 1;
        GameManager.Instance.ViewingQuadrant = quadrantNumber;
    }

    void ButtonHoverActive(int buttonIndex)
    {
        HideAllQuadrants();
        ImageMain.color = Color.gray;
        QuadrantImages[buttonIndex].SetActive(true);
    }

    void ButtonHoverUnactive()
    {
        HideAllQuadrants();
        ImageMain.color = Color.white;
    }

    void HideAllQuadrants()
    {
        for (int i = 0; i < 4; i++)
        {
            QuadrantImages[i].SetActive(false);
        }
    }
}