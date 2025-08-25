using TMPro;
using UnityEngine;

public class BuildingUIManager : MonoBehaviour
{
    [Header("Building UI References")]
    public static BuildingUIManager Instance;
    public GameObject UIBuildingViewer;
    public LayerMask buildingLayerMask = -1;
    public TextMeshProUGUI BuildingNameText;
    public TextMeshProUGUI BuildingInfoText;

    private BuildingPosition selectedBuildingData;
    private GameObject selectedBuilding;

    private string docks = "Dock_Building"; //exact name (dont change dock_building)
    public GameObject Tradingbtn;

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
    void Start()
    {
        if (UIBuildingViewer != null)
            UIBuildingViewer.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Only allow building interaction when not in placement mode
            if (BuildingManager.Instance != null && BuildingManager.Instance.isInPlacementMode)
                return;

            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;

            RaycastHit2D[] hits = Physics2D.RaycastAll(mouseWorldPos, Vector2.zero, 0f, buildingLayerMask);

            foreach (RaycastHit2D hit in hits)
            {
                BuildingClick buildingClick = hit.collider.GetComponent<BuildingClick>();
                if (buildingClick != null)
                {
                    buildingClick.HandleClick();
                    return;
                }
            }
        }
    }

    // Show the 'UI-BuildingViewer'
    public void ShowBuildingInfo(GameObject building, BuildingPosition buildingData)
    {
        selectedBuilding = building;
        selectedBuildingData = buildingData;

        if (UIBuildingViewer != null)
        {
            UIBuildingViewer.SetActive(true);

            if (selectedBuildingData.name == docks)
            {
                Tradingbtn.SetActive(true);
            }
        }
        UpdateBuildingContent();
    }

    // Hide the 'UI-BuildingViewer'
    public void HideBuildingInfo()
    {
        if (UIBuildingViewer != null)
        {
            UIBuildingViewer.SetActive(false);
            Tradingbtn.SetActive(false);
        }
        selectedBuilding = null;
        selectedBuildingData = null;
    }

    // Update 'UI-BuildingViewer' content with the selected building info
    void UpdateBuildingContent()
    {
        if (selectedBuildingData == null) return;
        BuildingProgress bp = selectedBuilding.GetComponent<BuildingProgress>();
        if (bp == null) return;

        // Show the building name and level on the UI
        if (BuildingNameText != null)
        {
            string buildingName = selectedBuilding.name.Replace("_Building", "");
            BuildingNameText.text = $"{buildingName} - Level {bp.currentLevel}";
        }

        // Show building details on the UI
        if (BuildingInfoText != null)
        {
            BuildingInfoText.text =
                $"Size: {selectedBuildingData.width}x{selectedBuildingData.height}\n" +
                $"Position: ({selectedBuildingData.gridX}, {selectedBuildingData.gridY})\n" +
                $"Grid: {selectedBuildingData.gridID}";
        }
    }

    // Upgrade the selected building
    public void UpgradeSelectedBuilding()
    {
        if (selectedBuilding == null)
            return;

        BuildingProgress bp = selectedBuilding.GetComponent<BuildingProgress>();
        if (bp != null && bp.plotData != null && bp.plotData.Upgrades.Length > 0)
        {
            // The cost is determined by the nextlevel's PlotData
            PlotManager.PlotData nextLevelData = bp.plotData.Upgrades[0];

            // Check for resources for the next level
            if (GameManager.Instance.HasEnoughFunds(nextLevelData.CostFunds) &&
                GameManager.Instance.HasEnoughWood(nextLevelData.CostWood) &&
                GameManager.Instance.HasEnoughStone(nextLevelData.CostStone) &&
                GameManager.Instance.HasEnoughMetal(nextLevelData.CostMetal))
            {
                BuildingManager.Instance.UpgradeBuilding(selectedBuilding);
                HideBuildingInfo();
            }
            else
            {
                Debug.Log("Not enough resources to upgrade");
            }
        }
        else
        {
            Debug.Log("No upgrade available for this building.");
        }
    }

    // Destroy the selected building
    public void DemolishSelectedBuilding()
    {
        if (selectedBuilding == null)
            return;

        BuildingProgress bp = selectedBuilding.GetComponent<BuildingProgress>();
        if (bp != null && bp.plotData != null && bp.plotData.PlotCategory == PlotManager.PlotCategory.Housing)
        {
            GameManager.Instance.RemovePopulation(bp.plotData.GainPopulation);
        }

        Destroy(selectedBuilding);
        HideBuildingInfo();
    }
}