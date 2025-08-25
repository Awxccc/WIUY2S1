using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonData : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Stored Plot Data")]
    public string BuildingName;
    public Sprite BuildingImage;
    public GameObject BuildingPrefab;
    public PlotManager.PlotBuildable BuildingType;
    public int TurnsToBuild;
    public int TileSizeWidth;
    public int TileSizeHeight;
    public int CostFunds;
    public int CostWood;
    public int CostStone;
    public int CostMetal;
    public string Description;

    private GameObject UIPlotInfo;
    private Transform PlotPanel;
    private Image PlotImage;
    private TextMeshProUGUI PlotNameText, PlotResource1Text, PlotResource2Text, PlotResource3Text, PlotResource4Text;
    private TextMeshProUGUI PlotSizeText, PlotTurnsText, PlotDescriptionText;

    private PlotData plotData;
    private UICoreScript uiCore;

    void Start()
    {
        // Search through all GameObjects in the scene and find one specifically called 'UI-PlotInfo'
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "UI-PlotInfo" && obj.scene.IsValid())
            {
                UIPlotInfo = obj;
                break;
            }
        }

        // If we found the 'UI-PlotInfo', then we find every images, text and whatever to update as we hover
        if (UIPlotInfo != null)
        {
            PlotPanel = UIPlotInfo.transform.Find("Plot-Panel");

            if (PlotPanel != null)
            {
                PlotImage = PlotPanel.Find("PlotImage")?.GetComponent<Image>();
                PlotNameText = PlotPanel.Find("PlotName")?.Find("UIText")?.GetComponent<TextMeshProUGUI>();
                PlotResource1Text = PlotPanel.Find("PlotResource1")?.Find("UIText")?.GetComponent<TextMeshProUGUI>();
                PlotResource2Text = PlotPanel.Find("PlotResource2")?.Find("UIText")?.GetComponent<TextMeshProUGUI>();
                PlotResource3Text = PlotPanel.Find("PlotResource3")?.Find("UIText")?.GetComponent<TextMeshProUGUI>();
                PlotResource4Text = PlotPanel.Find("PlotResource4")?.Find("UIText")?.GetComponent<TextMeshProUGUI>();
                PlotSizeText = PlotPanel.Find("PlotSize")?.Find("UIText")?.GetComponent<TextMeshProUGUI>();
                PlotTurnsText = PlotPanel.Find("PlotTurns")?.Find("UIText")?.GetComponent<TextMeshProUGUI>();
                PlotDescriptionText = PlotPanel.Find("PlotDescription")?.Find("UIText")?.GetComponent<TextMeshProUGUI>();
            }

            UIPlotInfo.SetActive(false);
        }

        // Find the UICoreScript in the scene to access the toggle state
        uiCore = FindFirstObjectByType<UICoreScript>();

    }

    // Set the building data that was taken from PlotManager
    public void SetPlotData(PlotData plotData)
    {
        this.plotData = plotData;
        BuildingName = plotData.PlotName;
        BuildingImage = plotData.PlotImage;
        BuildingPrefab = plotData.BuildingPrefab;
        BuildingType = (PlotManager.PlotBuildable)plotData.PlotBuildable;
        TurnsToBuild = plotData.TurnsToBuild;
        TileSizeWidth = plotData.TileSizeWidth;
        TileSizeHeight = plotData.TileSizeHeight;
        CostFunds = plotData.CostFunds;
        CostWood = plotData.CostWood;
        CostStone = plotData.CostStone;
        CostMetal = plotData.CostMetal;
        Description = plotData.Description;
    }

    // Update the 'UI-PlotInfo' panel content
    void UpdateDescriptiveContent()
    {
        if (PlotImage != null && BuildingImage != null)
            PlotImage.sprite = BuildingImage;

        if (PlotNameText != null)
            PlotNameText.text = BuildingName;

        if (PlotResource1Text != null)
            PlotResource1Text.text = "$" + CostFunds;

        if (PlotResource2Text != null)
            PlotResource2Text.text = CostWood + " Wood";

        if (PlotResource3Text != null)
            PlotResource3Text.text = CostStone + " Stone";

        if (PlotResource4Text != null)
            PlotResource4Text.text = CostMetal + " Metal";

        if (PlotSizeText != null)
            PlotSizeText.text = TileSizeHeight + "x" + TileSizeWidth;

        if (PlotTurnsText != null)
            PlotTurnsText.text = TurnsToBuild + " Turns";

        if (PlotDescriptionText != null)
            PlotDescriptionText.text = Description;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (BuildingManager.Instance == null || plotData == null)
        {
            Debug.LogWarning("BuildingManager not found or no plot data was assigned!");
            return;
        }

        BuildingManager.Instance.StartPlacement(plotData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (UIPlotInfo == null) return;
        UpdateDescriptiveContent();
        UIPlotInfo.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (UIPlotInfo != null)
        {
            UIPlotInfo.SetActive(false);
        }
    }
}
