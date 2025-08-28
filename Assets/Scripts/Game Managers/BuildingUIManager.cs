using UnityEngine;
using TMPro;

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

    private string docks = "Dock_Building"; // Exact name (dont change dock_building)
    public GameObject Tradingbtn;
    public Trading trading;

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
                if (hit.collider.TryGetComponent<BuildingClick>(out BuildingClick buildingClick))
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

        BuildingProgress bp = selectedBuilding.GetComponent<BuildingProgress>();

        if (UIBuildingViewer != null)
        {
            UIBuildingViewer.SetActive(true);

            if (bp != null && !bp.IsComplete)
            {
                // Show construction progress
                BuildingNameText.text = $"{selectedBuilding.name.Replace("_Building", "")} (Under Construction)";
                BuildingInfoText.gameObject.SetActive(false);
                if (Tradingbtn != null) Tradingbtn.SetActive(false);
            }
            else
            {
                // Show normal building info
                BuildingInfoText.gameObject.SetActive(true);
                UpdateBuildingContent();

                if (Tradingbtn != null)
                {
                    Tradingbtn.SetActive(false);
                }

                if (selectedBuilding.name == docks)
                {
                    if (Tradingbtn != null)
                    {
                        Tradingbtn.SetActive(true);
                    }

                    if (bp != null && trading != null)
                    {
                        trading.tradingSlots(bp.CurrentLevel);
                    }
                }
                // If the building is not a dock, ensure all extra slots are hidden
                else if (trading != null)
                {
                    trading.tradingSlots(0); // Pass 0 to hide all optional slots
                }
            }
            UpdateBuildingContent();
        }
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
        if (!selectedBuilding.TryGetComponent<BuildingProgress>(out BuildingProgress bp)) return;

        // Show the building name and level on the UI
        if (BuildingNameText != null)
        {
            string buildingName = selectedBuilding.name.Replace("_Building", "");
            BuildingNameText.text = $"{buildingName} - Level {bp.CurrentLevel}";
        }

        // Show building details on the UI
        if (BuildingInfoText != null)
        {
            BuildingInfoText.text =
                $" Size: {selectedBuildingData.width}x{selectedBuildingData.height}\n" +
                $" Benefits: {selectedBuildingData.fundsEarned} Funds, {selectedBuildingData.woodEarned} Wood, {selectedBuildingData.stoneEarned} Stone\n" +
                $" Population: {selectedBuildingData.populationEarned}\n" +
                $" Upgrade Cost: {selectedBuildingData.upgradeFunds} Funds, {selectedBuildingData.upgradeWoods} Wood, {selectedBuildingData.upgradeStones} Stone, {selectedBuildingData.upgradeMetals} Metal";
        }
    }

    // Upgrade the selected building
    public void UpgradeSelectedBuilding()
    {
        if (selectedBuilding == null)
            return;

        BuildingProgress bp = selectedBuilding.GetComponent<BuildingProgress>();
        if (bp != null && bp.PlotData != null && bp.PlotData.Upgrades.Length > 0 && bp.PlotData.Upgrades[0] != null)
        {
            // The cost is determined by the nextlevel's PlotData
            PlotData nextLevelData = bp.PlotData.Upgrades[0];

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
                AudioManager.Instance.ForcePlaceSFX(1);
                Debug.Log("Not enough resources to upgrade");
            }
        }
        else
        {
            AudioManager.Instance.ForcePlaceSFX(1);
            Debug.Log("No upgrade available for this building.");
        }
    }

    // Demolish building and refund 50%
    public void DemolishSelectedBuilding()
    {
        if (selectedBuilding == null)
            return;

        BuildingPosition pos = selectedBuilding.GetComponent<BuildingPosition>();
        BuildingProgress bp = selectedBuilding.GetComponent<BuildingProgress>();

        if (pos != null)
        {
            int fundsRefund = Mathf.CeilToInt(pos.totalFundsSpent / 2.0f);
            int woodRefund = Mathf.CeilToInt(pos.totalWoodSpent / 2.0f);
            int stoneRefund = Mathf.CeilToInt(pos.totalStoneSpent / 2.0f);
            int metalRefund = Mathf.CeilToInt(pos.totalMetalSpent / 2.0f);

            GameManager.Instance.AddFunds(fundsRefund);
            GameManager.Instance.AddWood(woodRefund);
            GameManager.Instance.AddStone(stoneRefund);
            GameManager.Instance.AddMetal(metalRefund);

            Debug.Log($"Demolished {selectedBuilding.name}. Refunded: {fundsRefund} Funds, {woodRefund} Wood, {stoneRefund} Stone, {metalRefund} Metal.");
        }

        if (bp != null)
        {
            if (bp.PlotData != null && bp.PlotData.PlotCategory == PlotManager.PlotCategory.Housing)
            {
                GameManager.Instance.RemovePopulation(bp.PlotData.GainPopulation);
            }
            // Remove the building from the GameManager's list
            GameManager.Instance.allBuildings.Remove(bp);
        }

        Destroy(selectedBuilding);
        HideBuildingInfo();
    }
}