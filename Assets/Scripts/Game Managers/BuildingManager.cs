using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [Header("Building Manager Settings")]
    public static BuildingManager Instance;
    public UICoreScript uiCore;
    public GameObject UIPlotInfo;
    public bool isInPlacementMode;

    private PlotData selectedPlotData;
    private bool lastPlacementMode;
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

    // Update is called once per frame
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

    // Start building placement mode with selected building data
    public void StartPlacement(PlotData plotData)
    {
        selectedPlotData = plotData;
        isInPlacementMode = true;
    }

    // Cancel placement mode and hide the UI panel
    public void CancelPlacement()
    {
        isInPlacementMode = false;
        selectedPlotData = null;

        if (UIPlotInfo != null)
            UIPlotInfo.SetActive(false);
    }

    // Method to place building at clicked grid position
    public void PlaceBuildingAtGrid(Vector3 gridPosition, int clickedX, int clickedY, string gridID)
    {
        if (!isInPlacementMode || selectedPlotData == null)
            return;

        PlotData finalPlotData = selectedPlotData;
        int finalFunds = selectedPlotData.CostFunds;
        int finalWood = selectedPlotData.CostWood;
        int finalStone = selectedPlotData.CostStone;
        int finalMetal = selectedPlotData.CostMetal;

        if (uiCore != null && uiCore.isAutoBuildMode)
        {
            CalculateAutoBuildCosts(ref finalPlotData, ref finalFunds, ref finalWood, ref finalStone, ref finalMetal);
        }

        if (!HasEnoughResources(finalFunds, finalWood, finalStone, finalMetal) ||
            !CanBuildingFitInGrid(gridID, finalPlotData))
            return;

        int finalX = FindBestPlacementX(clickedX, gridID, finalPlotData);
        if (finalX == -1)
        {
            AudioManager.Instance.ForcePlaceSFX(1);
            Debug.Log($"{finalPlotData.PlotName} cannot be placed as there is no available space!");
            return;
        }

        if (TryPlaceAt(finalX, 0, gridID, finalPlotData, finalFunds, finalWood, finalStone, finalMetal))
        {
            GameManager.Instance.RemoveFunds(finalFunds);
            GameManager.Instance.RemoveWood(finalWood);
            GameManager.Instance.RemoveStone(finalStone);
            GameManager.Instance.RemoveMetal(finalMetal);
            AudioManager.Instance.ForcePlaceSFX(0);
            CancelPlacement();
        }
    }

    // Calculate highest affordable upgrade level for auto-build mode
    void CalculateAutoBuildCosts(ref PlotData finalPlotData, ref int finalFunds, ref int finalWood, ref int finalStone, ref int finalMetal)
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
            else break;
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

    // Check to see if the player has enough resources for building
    bool HasEnoughResources(int funds, int wood, int stone, int metal)
    {
        if (!GameManager.Instance.HasEnoughFunds(funds) ||
            !GameManager.Instance.HasEnoughWood(wood) ||
            !GameManager.Instance.HasEnoughStone(stone) ||
            !GameManager.Instance.HasEnoughMetal(metal))
        {
            Debug.Log("Not enough resources to place this building!");
            return false;
        }
        return true;
    }

    // Attempt to spawn building at specified grid position
    bool TryPlaceAt(int gridX, int gridY, string gridID, PlotData plotData, int funds, int wood, int stone, int metal)
    {
        if (!CanPlaceOnTileType(gridX, gridY, gridID, plotData))
        {
            AudioManager.Instance.ForcePlaceSFX(1);
            Debug.Log($"{plotData.PlotName} cannot be placed on this tile type!");
            return false;
        }

        if (!IsPositionAvailable(gridX, gridY, gridID, plotData))
            return false;

        GameObject floorTile = GameObject.Find($"{gridID}_GridSquare_Pos{gridX}{gridY}");
        Vector3 spawnPosition = floorTile != null ? new Vector3(floorTile.transform.position.x - 0.5f, floorTile.transform.position.y - 0.5f, 0f) : new Vector3(gridX, gridY, 0f);
        GameObject newBuilding = Instantiate(plotData.BuildingPrefab, spawnPosition, Quaternion.identity);
        newBuilding.name = plotData.PlotName + "_Building";
        newBuilding.transform.localScale = new Vector3(plotData.TileSizeWidth, plotData.TileSizeHeight, 1f);

        SetBuildingParent(newBuilding, gridID);
        SetupBuildingPosition(newBuilding, gridX, gridY, gridID, plotData, funds, wood, stone, metal);

        if (newBuilding.TryGetComponent<BuildingProgress>(out BuildingProgress bp))
        {
            bp.Initialize(plotData, selectedPlotData.TurnsToBuild);
            GameManager.Instance.allBuildings.Add(bp);
        }
        return true;
    }

    // Set building's parent transform to grid hierarchy
    void SetBuildingParent(GameObject building, string gridID)
    {
        GameObject gridTemplate = GameObject.Find(gridID);
        if (gridTemplate != null)
        {
            building.transform.SetParent(gridTemplate.transform.parent != null ?
                gridTemplate.transform.parent : gridTemplate.transform);
        }
        else
        {
            Debug.LogWarning($"Could not find grid: {gridID}!");
        }
    }

    // Setup BuildingPosition component with all building data
    void SetupBuildingPosition(GameObject building, int gridX, int gridY, string gridID, PlotData plotData, int funds, int wood, int stone, int metal)
    {
        BuildingPosition position = building.AddComponent<BuildingPosition>();
        position.gridX = gridX;
        position.gridY = gridY;
        position.width = plotData.TileSizeWidth;
        position.height = plotData.TileSizeHeight;
        position.gridID = gridID;

        position.totalFundsSpent = funds;
        position.totalWoodSpent = wood;
        position.totalStoneSpent = stone;
        position.totalMetalSpent = metal;

        position.fundsEarned = plotData.GainFunds;
        position.woodEarned = plotData.GainWood;
        position.stoneEarned = plotData.GainStone;
        position.populationEarned = plotData.GainPopulation;

        position.upgradeLevel = 0;
        SetUpgradeData(position, plotData);
    }

    // Set next upgrade costs for building position data
    void SetUpgradeData(BuildingPosition position, PlotData plotData)
    {
        if (plotData.Upgrades != null && plotData.Upgrades.Length > 0 && plotData.Upgrades[0] != null)
        {
            PlotData upgradeData = plotData.Upgrades[0];
            position.upgradeFunds = upgradeData.CostFunds;
            position.upgradeWoods = upgradeData.CostWood;
            position.upgradeStones = upgradeData.CostStone;
            position.upgradeMetals = upgradeData.CostMetal;
        }
        else
        {
            position.upgradeFunds = 0;
            position.upgradeWoods = 0;
            position.upgradeStones = 0;
            position.upgradeMetals = 0;
        }
    }

    // Check if building area is free from other buildings
    bool IsPositionAvailable(int gridX, int gridY, string gridID, PlotData plotData)
    {
        BuildingPosition[] allBuildings = FindObjectsByType<BuildingPosition>(FindObjectsSortMode.None);
        for (int x = gridX; x < gridX + plotData.TileSizeWidth; x++)
        {
            for (int y = gridY; y < gridY + plotData.TileSizeHeight; y++)
            {
                foreach (BuildingPosition building in allBuildings)
                {
                    if (building.gridID == gridID &&
                        x >= building.gridX && x < building.gridX + building.width &&
                        y >= building.gridY && y < building.gridY + building.height)
                        return false;
                }
            }
        }
        return true;
    }

    // Check if building can be placed on this tile type
    bool CanPlaceOnTileType(int gridX, int gridY, string gridID, PlotData plotData)
    {
        GameObject tile = GameObject.Find($"{gridID}_GridSquare_Pos{gridX}{gridY}");
        if (tile == null) return false;

        GridTemplateScript gridTemplate = tile.GetComponentInParent<GridTemplateScript>();
        if (gridTemplate == null) return false;

        return (int)plotData.PlotBuildable == (int)gridTemplate.defaultBuildType;
    }

    // Check if building size fits within grid boundaries
    bool CanBuildingFitInGrid(string gridID, PlotData plotData)
    {
        GameObject gridTemplate = GameObject.Find(gridID);
        if (gridTemplate == null)
        {
            Debug.Log($"{plotData.PlotName} is too large for this grid!");
            return false;
        }

        if (!gridTemplate.TryGetComponent<GridTemplateScript>(out GridTemplateScript grid))
            return false;

        return plotData.TileSizeWidth <= grid.templateWidth && plotData.TileSizeHeight <= grid.templateHeight;
    }

    // Find the best X position for building placement from clicked position
    int FindBestPlacementX(int clickedX, string gridID, PlotData plotData)
    {
        GameObject gridTemplate = GameObject.Find(gridID);
        if (gridTemplate == null) return -1;

        if (!gridTemplate.TryGetComponent<GridTemplateScript>(out GridTemplateScript grid))
            return -1;

        int maxValidX = grid.templateWidth - plotData.TileSizeWidth;
        for (int testX = Mathf.Min(clickedX, maxValidX); testX >= 0; testX--)
        {
            if (CanPlaceAtPosition(testX, 0, gridID, plotData))
                return testX;
        }
        return -1;
    }

    // Check if the building can be placed at specific position
    bool CanPlaceAtPosition(int gridX, int gridY, string gridID, PlotData plotData)
    {
        return CanPlaceOnTileType(gridX, gridY, gridID, plotData) &&
               IsPositionAvailable(gridX, gridY, gridID, plotData);
    }

    // Upgrade the existing building to the next level
    public void UpgradeBuilding(GameObject buildingToUpgrade)
    {
        BuildingProgress bp = buildingToUpgrade.GetComponent<BuildingProgress>();
        if (bp == null || bp.PlotData == null || bp.PlotData.Upgrades.Length == 0)
        {
            AudioManager.Instance.ForcePlaceSFX(1);
            Debug.Log("This building cannot be upgraded!");
            return;
        }

        PlotData upgradeData = bp.PlotData.Upgrades[0];
        BuildingPosition pos = buildingToUpgrade.GetComponent<BuildingPosition>();

        if (!GameManager.Instance.HasEnoughFunds(upgradeData.CostFunds) ||
            !GameManager.Instance.HasEnoughWood(upgradeData.CostWood) ||
            !GameManager.Instance.HasEnoughStone(upgradeData.CostStone) ||
            !GameManager.Instance.HasEnoughMetal(upgradeData.CostMetal))
        {
            AudioManager.Instance.ForcePlaceSFX(1);
            Debug.Log("Not enough resources to upgrade!");
            return;
        }

        GameManager.Instance.RemoveFunds(upgradeData.CostFunds);
        GameManager.Instance.RemoveWood(upgradeData.CostWood);
        GameManager.Instance.RemoveStone(upgradeData.CostStone);
        GameManager.Instance.RemoveMetal(upgradeData.CostMetal);

        if (bp.PlotData.PlotCategory == PlotManager.PlotCategory.Housing)
        {
            GameManager.Instance.RemovePopulation(bp.PlotData.GainPopulation);
        }

        GameManager.Instance.allBuildings.Remove(bp);

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

        newPos.totalFundsSpent = pos.totalFundsSpent + upgradeData.CostFunds;
        newPos.totalWoodSpent = pos.totalWoodSpent + upgradeData.CostWood;
        newPos.totalStoneSpent = pos.totalStoneSpent + upgradeData.CostStone;
        newPos.totalMetalSpent = pos.totalMetalSpent + upgradeData.CostMetal;

        newPos.fundsEarned = upgradeData.GainFunds;
        newPos.woodEarned = upgradeData.GainWood;
        newPos.stoneEarned = upgradeData.GainStone;
        newPos.populationEarned = upgradeData.GainPopulation;

        newPos.upgradeLevel = pos.upgradeLevel + 1;
        SetUpgradeData(newPos, upgradeData);

        if (newBuilding.TryGetComponent<BuildingProgress>(out BuildingProgress newBp))
        {
            newBp.Initialize(upgradeData, upgradeData.TurnsToBuild);
            GameManager.Instance.allBuildings.Add(newBp);
        }
        if (upgradeData.PlotName == "Dock")
        {
            Trading trading = FindFirstObjectByType<Trading>();
            if (trading != null)
            {
                trading.UpdateTradeCapacity(newBp.CurrentLevel);
            }
        }

        AudioManager.Instance.ForcePlaceSFX(3);
        Destroy(buildingToUpgrade);
    }

    GameObject GetCurrentQuadrant()
    {
        return GameObject.Find("Grid_Quadrant" + GameManager.Instance.ViewingQuadrant);
    }
}

public class BuildingPosition : MonoBehaviour
{
    public int gridX, gridY, width, height;
    public string gridID;

    public int totalFundsSpent;
    public int totalWoodSpent;
    public int totalStoneSpent;
    public int totalMetalSpent;

    public int fundsEarned;
    public int woodEarned;
    public int stoneEarned;
    public int populationEarned;

    public int upgradeFunds;
    public int upgradeWoods;
    public int upgradeStones;
    public int upgradeMetals;

    public int upgradeLevel;
}