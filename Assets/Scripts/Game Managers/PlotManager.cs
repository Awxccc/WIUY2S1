using UnityEngine;
using UnityEngine.UI;

public class PlotManager : MonoBehaviour
{
    public enum PlotCategory { General, Housing, Productivity, Recreational }
    public enum PlotBuildable { Land, Costal, Sea }

    [System.Serializable]
    public class PlotData
    {
        [Header("Plot Data Settings")]
        public string PlotName;
        public int Level = 1;
        public Sprite PlotImage;
        public GameObject BuildingPrefab;
        public PlotCategory PlotCategory;
        public PlotBuildable PlotBuildable;
        public int AvailableBuildByTurn;
        public int TurnsToBuild;
        public int TileSizeWidth;
        public int TileSizeHeight;
        public int CostFunds;
        public int CostWood;
        public int CostStone;
        public int CostMetal;
        public int GainFunds;
        public int GainWood;
        public int GainStone;
        public int GainPopulation;
        public string Description = "";

        [SerializeReference]
        public PlotData[] Upgrades;
    }

    [Header("Building Data Settings")]
    public PlotData[] BuildingPlots;

    [Header("UI References Settings")]
    public GameObject PlotButtonPrefab;
    public Transform GeneralContent;
    public Transform HousingContent;
    public Transform ProductivityContent;
    public Transform RecreationalContent;

    private int lastCheckedTurn = -1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < BuildingPlots.Length; i++)
        {
            // Get every building plots that the game will have
            PlotData plot = BuildingPlots[i];

            Transform content = FindPlotGridByCategory(plot.PlotCategory);
            if (content == null) continue;

            // Create the buttons that will be shown in the build UI
            GameObject button = Instantiate(PlotButtonPrefab, content);
            button.name = plot.PlotName + "_Building";

            // Update button image sprites
            Transform buttonImage = button.transform.Find("ButtonImage");
            if (buttonImage != null && plot.PlotImage != null)
            {
                buttonImage.GetComponent<Image>().sprite = plot.PlotImage;
            }

            // Pass plot data to the instantiated button
            UIButtonData buttonData = button.GetComponent<UIButtonData>();
            if (buttonData != null) buttonData.SetPlotData(plot);
        }

        UpdateNavButtonVisibility();
    }

    // Update is called once per frame
    void Update()
    {
        // Set visibility after a turn has passed to determine if we can build this building
        if (GameManager.Instance != null && GameManager.Instance.CurrentTurn != lastCheckedTurn)
        {
            UpdateNavButtonVisibility();
            lastCheckedTurn = GameManager.Instance.CurrentTurn;
        }
    }

    // Method to scan through all of the GridNavs and update their visibility
    void UpdateNavButtonVisibility()
    {
        if (GameManager.Instance == null) return;

        CheckPlotGrid(GeneralContent, GameManager.Instance.CurrentTurn);
        CheckPlotGrid(HousingContent, GameManager.Instance.CurrentTurn);
        CheckPlotGrid(ProductivityContent, GameManager.Instance.CurrentTurn);
        CheckPlotGrid(RecreationalContent, GameManager.Instance.CurrentTurn);
    }

    // Check each individual plot grid if the buildings can be built in the current turn
    void CheckPlotGrid(Transform plotContent, int currentTurn)
    {
        if (plotContent == null) return;

        for (int i = 0; i < plotContent.childCount; i++)
        {
            Transform child = plotContent.GetChild(i);
            UIButtonData buttonData = child.GetComponent<UIButtonData>();

            if (buttonData != null)
            {
                PlotData matchingPlot = FindPlotDataByName(buttonData.BuildingName);
                if (matchingPlot != null)
                {
                    bool shouldBeVisible = currentTurn >= matchingPlot.AvailableBuildByTurn;
                    child.gameObject.SetActive(shouldBeVisible);
                }
            }
        }
    }

    // Self explanatory: Find the plot data by its literal name
    PlotData FindPlotDataByName(string buildingName)
    {
        for (int i = 0; i < BuildingPlots.Length; i++)
        {
            if (BuildingPlots[i].PlotName == buildingName)
            {
                return BuildingPlots[i];
            }
        }
        return null;
    }

    // Self explanatory: Find the plot grid by category
    Transform FindPlotGridByCategory(PlotCategory category)
    {
        switch (category)
        {
            case PlotCategory.General:
                return GeneralContent;

            case PlotCategory.Housing:
                return HousingContent;

            case PlotCategory.Productivity:
                return ProductivityContent;

            case PlotCategory.Recreational:
                return RecreationalContent;

            default:
                return null;
        }
    }
}