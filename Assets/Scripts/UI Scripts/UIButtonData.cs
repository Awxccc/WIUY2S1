using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static PlotManager;

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

    private PlotManager.PlotData plotData;
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
    public void SetPlotData(PlotManager.PlotData plotData)
    {
        this.plotData = plotData;
        BuildingName = plotData.PlotName;
        BuildingImage = plotData.PlotImage;
        BuildingPrefab = plotData.BuildingPrefab;
        BuildingType = plotData.PlotBuildable;
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
        if (BuildingManager.Instance == null || plotData.BuildingPrefab == null)
        {
            Debug.LogWarning("BuildingManager not found or no prefab was assigned!");
            return;
        }

        // Check the toggle state from UICoreScript
        if (uiCore != null && uiCore.isQuickBuildMode)
        {
            HandleQuickBuildClick();
        }
        else
        {
            HandleNormalBuildClick();
        }
    }

    private void HandleNormalBuildClick()
    {
        // Standard placement for the Level 1 building
        BuildingManager.Instance.StartPlacement(
            plotData,
            plotData.TurnsToBuild,
            plotData.CostFunds,
            plotData.CostWood,
            plotData.CostStone,
            plotData.CostMetal
        );
    }

    private void HandleQuickBuildClick()
    {
        PlotData highestAffordablePlot = plotData;
        int totalFunds = plotData.CostFunds;
        int totalWood = plotData.CostWood;
        int totalStone = plotData.CostStone;
        int totalMetal = plotData.CostMetal;

        PlotData currentPlot = plotData;
        while (currentPlot.Upgrades != null && currentPlot.Upgrades.Length > 0)
        {
            PlotData nextUpgrade = currentPlot.Upgrades[0];

            // Accumulate the costs for the next level
            int nextLevelTotalFunds = totalFunds + nextUpgrade.CostFunds;
            int nextLevelTotalWood = totalWood + nextUpgrade.CostWood;
            int nextLevelTotalStone = totalStone + nextUpgrade.CostStone;
            int nextLevelTotalMetal = totalMetal + nextUpgrade.CostMetal;

            // Check if player can afford the *doubled* total cost for the next level
            if (GameManager.Instance.HasEnoughFunds(nextLevelTotalFunds * 2) &&
                GameManager.Instance.HasEnoughWood(nextLevelTotalWood * 2) &&
                GameManager.Instance.HasEnoughStone(nextLevelTotalStone * 2) &&
                GameManager.Instance.HasEnoughMetal(nextLevelTotalMetal * 2))
            {
                highestAffordablePlot = nextUpgrade;
                totalFunds = nextLevelTotalFunds;
                totalWood = nextLevelTotalWood;
                totalStone = nextLevelTotalStone;
                totalMetal = nextLevelTotalMetal;
                currentPlot = nextUpgrade;
            }
            else
            {
                break;
            }
        }

        if (!GameManager.Instance.HasEnoughFunds(plotData.CostFunds) ||
            !GameManager.Instance.HasEnoughWood(plotData.CostWood) ||
            !GameManager.Instance.HasEnoughStone(plotData.CostStone) ||
            !GameManager.Instance.HasEnoughMetal(plotData.CostMetal))
        {
            Debug.Log("Not enough resources to build even the Level 1 building!");
            return;
        }

        Debug.Log($"Quick Build: Placing {highestAffordablePlot.PlotName} (Level {highestAffordablePlot.Level}). " + $"Cost: {totalFunds * 2} Funds. Turns: {plotData.TurnsToBuild}");

        BuildingManager.Instance.StartPlacement(
            highestAffordablePlot,
            plotData.TurnsToBuild,
            totalFunds * 2,
            totalWood * 2,
            totalStone * 2,
            totalMetal * 2
        );
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
