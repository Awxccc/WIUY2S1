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
            UIBuildingViewer.SetActive(true);

        UpdateBuildingContent();
    }

    // Hide the 'UI-BuildingViewer'
    public void HideBuildingInfo()
    {
        if (UIBuildingViewer != null)
            UIBuildingViewer.SetActive(false);

        selectedBuilding = null;
        selectedBuildingData = null;
    }

    // Update 'UI-BuildingViewer' content with the selected building info
    void UpdateBuildingContent()
    {
        if (selectedBuildingData == null) return;

        // Show the building name on the UI
        if (BuildingNameText != null)
        {
            string buildingName = selectedBuilding.name.Replace("_Building", ""); // Found this gem online, gonna steal it.
            BuildingNameText.text = buildingName;
        }

        // Show building details on the UI (for debugging purposes)
        // TODO: Replace this with something more final later on!
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

        Debug.Log("Upgrade building pressed!");
    }

    // Destroy the selected building
    public void DemolishSelectedBuilding()
    {
        if (selectedBuilding == null)
            return;

        Destroy(selectedBuilding);
        HideBuildingInfo();
    }
}