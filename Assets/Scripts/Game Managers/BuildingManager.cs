using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [Header("Building Manager Settings")]
    public static BuildingManager Instance;
    public bool isInPlacementMode = false;
    public UICoreScript uiCore;

    private PlotData selectedPlotData;

    private bool lastPlacementMode = false;
    private int lastViewingQuadrant = -1;

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

    void Update()
    {
        // Toggle grid template visibility when placement state or quadrant changes
        if ((isInPlacementMode != lastPlacementMode) || (GameManager.Instance.ViewingQuadrant != lastViewingQuadrant))
        {
            if (GameManager.Instance == null || GameManager.Instance.ViewingQuadrant == 0)
                return;

            GameObject currentQuadrant = GetCurrentQuadrant();
            if (currentQuadrant == null) return;

            GridTemplateScript[] allGrids = currentQuadrant.GetComponentsInChildren<GridTemplateScript>(true);

            foreach (GridTemplateScript grid in allGrids)
            {
                grid.gameObject.SetActive(isInPlacementMode);
            }

            lastPlacementMode = isInPlacementMode;
            lastViewingQuadrant = GameManager.Instance != null ? GameManager.Instance.ViewingQuadrant : -1;
        }
    }

    // Called when player clicks a building button
    public void StartPlacement(PlotData plotData)
    {
        selectedPlotData = plotData;
        isInPlacementMode = true;
    }

    // Called when placement is cancelled
    public void CancelPlacement()
    {
        isInPlacementMode = false;
        selectedPlotData = null;
    }

    // Attempt to place a building at the clicked tile
    public void PlaceBuildingAtGrid(Vector3 gridPosition, int clickedX, int clickedY, string gridID)
    {
        if (!isInPlacementMode || selectedPlotData == null)
            return;

        PlotData finalPlotData;
        int finalFunds, finalWood, finalStone, finalMetal;

        // Check the Quick Build toggle when placing
        if (uiCore != null && uiCore.isQuickBuildMode)
        {
            PlotData highestAffordablePlot = selectedPlotData;
            int totalFunds = selectedPlotData.CostFunds;
            int totalWood = selectedPlotData.CostWood;
            int totalStone = selectedPlotData.CostStone;
            int totalMetal = selectedPlotData.CostMetal;
            PlotData currentPlot = selectedPlotData;

            while (currentPlot.Upgrades != null && currentPlot.Upgrades.Length > 0 && currentPlot.Upgrades[0] != null)
            {
                PlotData nextUpgrade = currentPlot.Upgrades[0];
                int nextLevelTotalFunds = totalFunds + nextUpgrade.CostFunds;
                int nextLevelTotalWood = totalWood + nextUpgrade.CostWood;
                int nextLevelTotalStone = totalStone + nextUpgrade.CostStone;
                int nextLevelTotalMetal = totalMetal + nextUpgrade.CostMetal;

                if (GameManager.Instance.HasEnoughFunds((int)(nextLevelTotalFunds * 1.5f)) &&
                GameManager.Instance.HasEnoughWood((int)(nextLevelTotalWood * 1.5f)) &&
                GameManager.Instance.HasEnoughStone((int)(nextLevelTotalStone * 1.5f)) &&
                GameManager.Instance.HasEnoughMetal((int)(nextLevelTotalMetal * 1.5f)))
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

            finalPlotData = highestAffordablePlot;
            if (highestAffordablePlot != selectedPlotData)
            {
                finalFunds = Mathf.CeilToInt(totalFunds * 1.5f);
                finalWood = Mathf.CeilToInt(totalWood * 1.5f);
                finalStone = Mathf.CeilToInt(totalStone * 1.5f);
                finalMetal = Mathf.CeilToInt(totalMetal * 1.5f);
            }
            else
            {
                finalFunds = totalFunds;
                finalWood = totalWood;
                finalStone = totalStone;
                finalMetal = totalMetal;
            }
        }
        else
        {
            finalPlotData = selectedPlotData;
            finalFunds = selectedPlotData.CostFunds;
            finalWood = selectedPlotData.CostWood;
            finalStone = selectedPlotData.CostStone;
            finalMetal = selectedPlotData.CostMetal;
        }

        //Check resources with the final calculated costs
        if (!GameManager.Instance.HasEnoughFunds(finalFunds) ||
            !GameManager.Instance.HasEnoughWood(finalWood) ||
            !GameManager.Instance.HasEnoughStone(finalStone) ||
            !GameManager.Instance.HasEnoughMetal(finalMetal))
        {
            Debug.Log("Not enough resources to place this building!");
            return;
        }

        // Ensure the Highest level building can fit in the grid
        if (!CanBuildingFitInGrid(gridID, finalPlotData))
        {
            Debug.Log($"{finalPlotData.PlotName} is too large for this grid!");
            return;
        }

        int finalX = FindBestPlacementX(clickedX, gridID, finalPlotData);
        if (finalX == -1)
        {
            Debug.Log($"{finalPlotData.PlotName} cannot be placed as there is no available space!");
            return;
        }

        // Try placing the final building
        if (TryPlaceAt(finalX, 0, gridID, finalPlotData, finalFunds, finalWood, finalStone, finalMetal))
        {
            GameManager.Instance.RemoveFunds(finalFunds);
            GameManager.Instance.RemoveWood(finalWood);
            GameManager.Instance.RemoveStone(finalStone);
            GameManager.Instance.RemoveMetal(finalMetal);
            CancelPlacement();
        }
    }

    // Actually spawn the building prefab
    bool TryPlaceAt(int gridX, int gridY, string gridID, PlotData plotData, int funds, int wood, int stone, int metal)
    {
        if (!CanPlaceOnTileType(gridX, gridY, gridID, plotData))
        {
            Debug.Log($"{plotData.PlotName} cannot be placed on this tile type!");
            return false;
        }

        BuildingPosition[] allBuildings = FindObjectsByType<BuildingPosition>(FindObjectsSortMode.None);
        for (int x = gridX; x < gridX + plotData.TileSizeWidth; x++)
        {
            for (int y = gridY; y < gridY + plotData.TileSizeHeight; y++)
            {
                foreach (BuildingPosition building in allBuildings)
                {
                    if (building.gridID == gridID && x >= building.gridX && x < building.gridX + building.width && y >= building.gridY && y < building.gridY + building.height)
                        return false;
                }
            }
        }

        GameObject floorTile = GameObject.Find($"{gridID}_GridSquare_Pos{gridX}{gridY}");
        Vector3 spawnPosition = floorTile != null ? new Vector3(floorTile.transform.position.x - 0.5f, floorTile.transform.position.y - 0.5f, 0f) : new Vector3(gridX, gridY, 0f);

        GameObject newBuilding = Instantiate(plotData.BuildingPrefab, spawnPosition, Quaternion.identity);
        newBuilding.name = plotData.PlotName + "_Building";
        newBuilding.transform.localScale = new Vector3(plotData.TileSizeWidth, plotData.TileSizeHeight, 1f);

        GameObject gridTemplate = GameObject.Find(gridID);
        if (gridTemplate != null)
        {
            if (gridTemplate.transform.parent != null)
            {
                newBuilding.transform.SetParent(gridTemplate.transform.parent);
            }
            else
            {
                newBuilding.transform.SetParent(gridTemplate.transform);
            }
        }
        else
            Debug.LogWarning($"Could not find grid: {gridID}");

        BuildingPosition position = newBuilding.AddComponent<BuildingPosition>();
        position.gridX = gridX;
        position.gridY = gridY;
        position.width = plotData.TileSizeWidth;
        position.height = plotData.TileSizeHeight;
        position.gridID = gridID;
        position.totalFundsSpent = funds;
        position.totalWoodSpent = wood;
        position.totalStoneSpent = stone;
        position.totalMetalSpent = metal;

        if (newBuilding.TryGetComponent<BuildingProgress>(out var bp))
        {
            bp.Initialize(plotData, selectedPlotData.TurnsToBuild);
            GameManager.Instance.allBuildings.Add(bp);
        }
        return true;
    }
    //Check tile type
    bool CanPlaceOnTileType(int gridX, int gridY, string gridID, PlotData plotData)
    {
        GameObject tile = GameObject.Find($"{gridID}_GridSquare_Pos{gridX}{gridY}");
        if (tile == null) return false;
        GridTemplateScript gridTemplate = tile.GetComponentInParent<GridTemplateScript>();
        if (gridTemplate == null) return false;

        return (int)plotData.PlotBuildable == (int)gridTemplate.defaultBuildType;
    }
    //ensure building fits in template bounds
    bool CanBuildingFitInGrid(string gridID, PlotData plotData)
    {
        GameObject gridTemplate = GameObject.Find(gridID);
        if (gridTemplate == null) return false;
        if (!gridTemplate.TryGetComponent<GridTemplateScript>(out var grid)) return false;

        return plotData.TileSizeWidth <= grid.templateWidth && plotData.TileSizeHeight <= grid.templateHeight;
    }
    //Try finding a legal position starting from clicked tile
    int FindBestPlacementX(int clickedX, string gridID, PlotData plotData)
    {
        GameObject gridTemplate = GameObject.Find(gridID);
        if (gridTemplate == null) return -1;
        if (!gridTemplate.TryGetComponent<GridTemplateScript>(out var grid)) return -1;

        int maxValidX = grid.templateWidth - plotData.TileSizeWidth;
        for (int testX = Mathf.Min(clickedX, maxValidX); testX >= 0; testX--)
        {
            if (CanPlaceAtPosition(testX, 0, gridID, plotData))
                return testX;
        }
        return -1;
    }

    bool CanPlaceAtPosition(int gridX, int gridY, string gridID, PlotData plotData)
    {
        if (!CanPlaceOnTileType(gridX, gridY, gridID, plotData))
            return false;

        BuildingPosition[] allBuildings = FindObjectsByType<BuildingPosition>(FindObjectsSortMode.None);
        for (int x = gridX; x < gridX + plotData.TileSizeWidth; x++)
        {
            for (int y = gridY; y < gridY + plotData.TileSizeHeight; y++)
            {
                foreach (BuildingPosition building in allBuildings)
                {
                    if (building.gridID == gridID && x >= building.gridX && x < building.gridX + building.width && y >= building.gridY && y < building.gridY + building.height)
                        return false;
                }
            }
        }
        return true;
    }

    GameObject GetCurrentQuadrant()
    {
        string quadrantName = "Grid_Quadrant" + GameManager.Instance.ViewingQuadrant;
        return GameObject.Find(quadrantName);
    }
    public void UpgradeBuilding(GameObject buildingToUpgrade)
    {
        BuildingProgress bp = buildingToUpgrade.GetComponent<BuildingProgress>();
        if (bp == null || bp.plotData == null || bp.plotData.Upgrades.Length == 0)
        {
            Debug.Log("This building cannot be upgraded.");
            return;
        }

        PlotData upgradeData = bp.plotData.Upgrades[0];
        BuildingPosition pos = buildingToUpgrade.GetComponent<BuildingPosition>();

        if (!GameManager.Instance.HasEnoughFunds(upgradeData.CostFunds) ||
        !GameManager.Instance.HasEnoughWood(upgradeData.CostWood) ||
        !GameManager.Instance.HasEnoughStone(upgradeData.CostStone) ||
        !GameManager.Instance.HasEnoughMetal(upgradeData.CostMetal))
        {
            Debug.Log("Not enough resources to upgrade.");
            return;
        }

        GameManager.Instance.RemoveFunds(upgradeData.CostFunds);
        GameManager.Instance.RemoveWood(upgradeData.CostWood);
        GameManager.Instance.RemoveStone(upgradeData.CostStone);
        GameManager.Instance.RemoveMetal(upgradeData.CostMetal);

        if (bp.plotData.PlotCategory == PlotManager.PlotCategory.Housing)
        {
            GameManager.Instance.RemovePopulation(bp.plotData.GainPopulation);
        }

        // Remove old building from the list
        GameManager.Instance.allBuildings.Remove(bp);

        // Instantiate the new building
        GameObject newBuilding = Instantiate(upgradeData.BuildingPrefab, buildingToUpgrade.transform.position, Quaternion.identity);
        newBuilding.name = upgradeData.PlotName + "_Building";
        newBuilding.transform.localScale = new Vector3(pos.width, pos.height, 1f);
        newBuilding.transform.SetParent(buildingToUpgrade.transform.parent);

        BuildingPosition newPos = newBuilding.AddComponent<BuildingPosition>();
        newPos.gridX = pos.gridX;
        newPos.gridY = pos.gridY;
        newPos.width = pos.width;
        newPos.height = pos.height;
        newPos.gridID = pos.gridID;

        if (newBuilding.TryGetComponent<BuildingProgress>(out var newBp))
        {
            newBp.Initialize(upgradeData, upgradeData.TurnsToBuild);
            GameManager.Instance.allBuildings.Add(newBp);
        }
        if (pos != null)
        {
            newPos.totalFundsSpent = pos.totalFundsSpent + upgradeData.CostFunds;
            newPos.totalWoodSpent = pos.totalWoodSpent + upgradeData.CostWood;
            newPos.totalStoneSpent = pos.totalStoneSpent + upgradeData.CostStone;
            newPos.totalMetalSpent = pos.totalMetalSpent + upgradeData.CostMetal;
        }
        Destroy(buildingToUpgrade);
    }
}

// Tracks which tile a building occupies
public class BuildingPosition : MonoBehaviour
{
    public int gridX, gridY, width, height;
    public string gridID;

    public int totalFundsSpent;
    public int totalWoodSpent;
    public int totalStoneSpent;
    public int totalMetalSpent;
}