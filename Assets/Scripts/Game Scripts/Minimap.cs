using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    public Camera mainCamera;
    public Camera minimapCamera;
    public RawImage minimapImage;
    public RectTransform minimapRectTransform;
    public RectTransform cameraIndicator;
    public GameObject buildingIndicatorPrefab;
    public RectTransform buildingIndicatorParent;

    [Header("Minimap Settings")]
    public float minimapOrthographicSize = 64f;
    public float mapCenterY = 56f;
    public float cameraIndicatorYOffset = 12f;

    private RenderTexture renderTexture;
    private Dictionary<BuildingProgress, GameObject> buildingIndicatorMap = new();//Track each building

    // Start is called before the first frame update.
    void Start()
    {
        if (mainCamera == null || minimapCamera == null || minimapImage == null || cameraIndicator == null || buildingIndicatorPrefab == null || buildingIndicatorParent == null)
        {
            return;
        }

        //Get what the minimap camera sees then place on my minimap image
        renderTexture = new RenderTexture(256, 256, 16);
        minimapCamera.targetTexture = renderTexture;
        minimapImage.texture = renderTexture;
        minimapCamera.orthographicSize = minimapOrthographicSize;

        Vector3 camPos = minimapCamera.transform.position;
        minimapCamera.transform.position = new Vector3(camPos.x, mapCenterY, camPos.z);
    }
    void LateUpdate()
    {
        UpdateCameraIndicator();
        UpdateBuildingIndicators();
    }

    void UpdateCameraIndicator()
    {
        Vector3 mainCamPos = mainCamera.transform.position;
        Vector3 minimapPoint = minimapCamera.WorldToViewportPoint(mainCamPos);

        float mapWidth = minimapRectTransform.rect.width;
        float mapHeight = minimapRectTransform.rect.height;

        cameraIndicator.anchoredPosition = new Vector2((minimapPoint.x - 0.5f) * mapWidth, ((minimapPoint.y - 0.5f) * mapHeight) - cameraIndicatorYOffset);
    }

    void UpdateBuildingIndicators()
    {
        if (GameManager.Instance == null) return;

        // Check all buildings in the game
        foreach (var building in GameManager.Instance.allBuildings)
        {
            if (building != null && !buildingIndicatorMap.ContainsKey(building))
            {
                CreateBuildingIndicator(building);
            }
        }

        var destroyedBuildings = buildingIndicatorMap.Keys.Where(b => b == null).ToList();
        foreach (var destroyedBuilding in destroyedBuildings)
        {
            if (buildingIndicatorMap.TryGetValue(destroyedBuilding, out GameObject indicator))
            {
                Destroy(indicator);
                buildingIndicatorMap.Remove(destroyedBuilding);
            }
        }

        // Only show building if on correct quadrant
        string currentQuadrantName = "Grid_Quadrant" + GameManager.Instance.ViewingQuadrant;
        foreach (var pair in buildingIndicatorMap)
        {
            BuildingProgress building = pair.Key;
            GameObject indicator = pair.Value;

            if (building != null && building.transform.parent != null)
            {
                bool isInCurrentQuadrant = building.transform.parent.name == currentQuadrantName;
                indicator.SetActive(isInCurrentQuadrant);
            }
        }
    }

    void CreateBuildingIndicator(BuildingProgress building)
    {
        GameObject indicator = Instantiate(buildingIndicatorPrefab, buildingIndicatorParent);
        RectTransform indicatorRect = indicator.GetComponent<RectTransform>();

        PlotData plotData = building.PlotData;
        if (plotData == null) return;

        // Center the indicator since all the sprites set are bottom left pivot
        float centerX = building.transform.position.x + (plotData.TileSizeWidth / 2.0f);
        float centerY = building.transform.position.y + (plotData.TileSizeHeight / 2.0f);
        Vector3 buildingCenterPos = new(centerX, centerY, building.transform.position.z);

        Vector3 minimapPoint = minimapCamera.WorldToViewportPoint(buildingCenterPos);
        float mapWidth = minimapRectTransform.rect.width;
        float mapHeight = minimapRectTransform.rect.height;
        indicatorRect.anchoredPosition = new Vector2((minimapPoint.x - 0.5f) * mapWidth, (minimapPoint.y - 0.5f) * mapHeight);

        buildingIndicatorMap.Add(building, indicator);

        if (building.transform.parent != null)
        {
            string currentQuadrantName = "Grid_Quadrant" + GameManager.Instance.ViewingQuadrant;
            bool isInCurrentQuadrant = building.transform.parent.name == currentQuadrantName;
            indicator.SetActive(isInCurrentQuadrant);
        }
    }
}
