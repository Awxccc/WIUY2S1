using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Minimap : MonoBehaviour, IPointerClickHandler
{
    [Header("Minimap References")]
    public Camera mainCamera;
    public Camera minimapCamera;
    public RawImage minimapImage;
    public RectTransform minimapRectTransform;
    public RectTransform cameraIndicator;
    public GameObject buildingIndicatorPrefab;
    public RectTransform buildingIndicatorParent;

    [Header("Minimap Settings")]
    public float minimapOrthographicSize = 64f;
    public float mapCenterY = 0f;
    public float cameraIndicatorYOffset = 0f;

    private RenderTexture renderTexture;
    private Dictionary<BuildingProgress, GameObject> buildingIndicatorMap = new();

    void Start()
    {
        if (mainCamera == null || minimapCamera == null || minimapImage == null || cameraIndicator == null || buildingIndicatorPrefab == null || buildingIndicatorParent == null)
        {
            return;
        }

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

        cameraIndicator.anchoredPosition = new Vector2(
            (minimapPoint.x - 0.5f) * mapWidth,
            ((minimapPoint.y - 0.5f) * mapHeight) - cameraIndicatorYOffset
        );
    }

    void UpdateBuildingIndicators()
    {
        if (GameManager.Instance == null) return;

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
    }

    void CreateBuildingIndicator(BuildingProgress building)
    {
        GameObject indicator = Instantiate(buildingIndicatorPrefab, buildingIndicatorParent);
        RectTransform indicatorRect = indicator.GetComponent<RectTransform>();

        PlotData plotData = building.PlotData;
        if (plotData == null) return;

        float centerX = building.transform.position.x + (plotData.TileSizeWidth / 2.0f);
        float centerY = building.transform.position.y + (plotData.TileSizeHeight / 2.0f);
        Vector3 buildingCenterPos = new(centerX, centerY, building.transform.position.z);


        Vector3 minimapPoint = minimapCamera.WorldToViewportPoint(buildingCenterPos);

        float mapWidth = minimapRectTransform.rect.width;
        float mapHeight = minimapRectTransform.rect.height;

        indicatorRect.anchoredPosition = new Vector2(
            (minimapPoint.x - 0.5f) * mapWidth,
            (minimapPoint.y - 0.5f) * mapHeight
        );

        buildingIndicatorMap.Add(building, indicator);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapRectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localCursor))
            return;

        float normalizedX = (localCursor.x + minimapRectTransform.rect.width * 0.5f) / minimapRectTransform.rect.width;
        float normalizedY = (localCursor.y + minimapRectTransform.rect.height * 0.5f) / minimapRectTransform.rect.height;

        Vector3 targetWorldPoint = minimapCamera.ViewportToWorldPoint(new Vector3(normalizedX, normalizedY, minimapCamera.nearClipPlane));

        CameraScript mainCameraScript = mainCamera.GetComponent<CameraScript>();
        if (mainCameraScript != null && mainCameraScript.CameraTarget != null)
        {
            Transform cameraTarget = mainCameraScript.CameraTarget;
            cameraTarget.position = new Vector3(targetWorldPoint.x, targetWorldPoint.y, cameraTarget.position.z);
        }
    }
}
