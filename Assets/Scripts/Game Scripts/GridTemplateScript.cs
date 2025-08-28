using UnityEngine;

public class GridTemplateScript : MonoBehaviour
{
    public enum PlotBuildable { Land, Coastal, Sea, Unavailable }

    [Header("Grid Settings")]
    public int templateWidth;
    public int templateHeight;
    public PlotBuildable defaultBuildType;
    public GameObject gridSquarePrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Clear any grid just incase there is one
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        for (int x = 0; x < templateWidth; x++)
        {
            for (int y = 0; y < templateHeight; y++)
            {
                // Get the tile grid position
                Vector3 localPosition = new(x, y, 0f);
                Vector3 worldPosition = transform.position + localPosition;

                // Create the grid square
                GameObject square = Instantiate(gridSquarePrefab, worldPosition, Quaternion.identity);
                square.transform.SetParent(this.transform);
                square.name = $"{this.gameObject.name}_GridSquare_Pos{x}{y}";

                // Make sure the grid square has a collider for click detection
                Collider2D collider = square.GetComponent<Collider2D>();
                if (collider == null)
                    collider = square.AddComponent<BoxCollider2D>();

                // Add the click handler to detect when this tile is clicked
                GridTileClickHandler clickHandler = square.GetComponent<GridTileClickHandler>();
                if (clickHandler == null)
                    clickHandler = square.AddComponent<GridTileClickHandler>();

                clickHandler.SetGridPosition(x, y, this.gameObject.name);
                ApplyTemplateColours(square, defaultBuildType);
            }
        }
    }

    void ApplyTemplateColours(GameObject square, PlotBuildable buildType)
    {
        SpriteRenderer renderer = square.GetComponent<SpriteRenderer>();
        if (renderer == null) return;

        Vector3 rgbColor;

        switch (buildType)
        {
            case PlotBuildable.Land:
                rgbColor = new Vector3(170, 205, 115);
                break;
            case PlotBuildable.Coastal:
                rgbColor = new Vector3(225, 155, 200);
                break;
            case PlotBuildable.Sea:
                rgbColor = new Vector3(165, 185, 230);
                break;
            case PlotBuildable.Unavailable:
                rgbColor = new Vector3(175, 45, 35);
                break;
            default:
                rgbColor = new Vector3(255, 255, 255);
                break;
        }
        renderer.color = new Color(rgbColor.x / 255f, rgbColor.y / 255f, rgbColor.z / 255f, 0.5f);
    }

    // When the inspector changes values, update immediately
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                ApplyTemplateColours(transform.GetChild(i).gameObject, defaultBuildType);
            }
        }
    }

    // Draw Gizmo to show the grid size and initial grid point
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(1f, 1f, 0.1f));

        if (templateWidth > 0 && templateHeight > 0)
        {
            Gizmos.color = Color.green;
            Vector3 gridCenter = transform.position + new Vector3((templateWidth - 1) * 0.5f, (templateHeight - 1) * 0.5f, 0f);
            Vector3 gridSize = new Vector3(templateWidth, templateHeight, 0.1f);
            Gizmos.DrawWireCube(gridCenter, gridSize);
        }
    }
}