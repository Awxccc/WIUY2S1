using UnityEngine;

public class GridClickManager : MonoBehaviour
{
    [Header("Detection Raycast Settings")]
    public LayerMask gridLayerMask = -1;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleGridClick();
        }
    }

    void HandleGridClick()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        RaycastHit2D[] hits = Physics2D.RaycastAll(mouseWorldPos, Vector2.zero, 0f, gridLayerMask);

        foreach (RaycastHit2D hit in hits)
        {
            GridTileClickHandler tileHandler = hit.collider.GetComponent<GridTileClickHandler>();
            if (tileHandler != null)
            {
                tileHandler.HandleClick();
                return;
            }
        }
    }
}