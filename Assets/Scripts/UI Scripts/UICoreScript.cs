using TMPro;
using UnityEngine;

public class UICoreScript : MonoBehaviour
{
    [Header("UI References Settings")]
    public GameObject UIGameViewer;
    public GameObject UIPlotViewer;
    public GameObject UIWorldMap;

    [Header("Grid Quadrant References")]
    public GameObject GridQuadrant1;
    public GameObject GridQuadrant2;
    public GameObject GridQuadrant3;
    public GameObject GridQuadrant4;

    [Header("Building Category References")]
    public GameObject UICategoryButton;
    public GameObject PlotGridGeneral;
    public GameObject PlotGridHousing;
    public GameObject PlotGridProductivity;
    public GameObject PlotGridRecreational;

    private Vector3 showPosition = new(-17.35f, 64.6f, -6.66f);
    private Vector3 hidePosition = new(-17.35f, -152f, -6.66f);

    private bool showPlotViewer = false;
    private int lastViewingQuadrant = -1;

    public TMP_Text turnsText;
    public TMP_Text turnTimeText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Check if we should show the plot grid viewer and the category buttons
        if (UIPlotViewer != null)
        {
            Vector3 startPos = showPlotViewer ? showPosition : hidePosition;

            UIPlotViewer.transform.localPosition = startPos;
            UICategoryButton.SetActive(showPlotViewer);
        }

        UpdateUIBasedOnQuadrant();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if QuadrantView has changed values, if so we hide quadrant selector and show game UI
        if (GameManager.Instance != null && GameManager.Instance.ViewingQuadrant != lastViewingQuadrant)
        {
            UpdateUIBasedOnQuadrant();
            lastViewingQuadrant = GameManager.Instance.ViewingQuadrant;
        }

        // Move the UIPlotViewer depending on whether it should be visible or not
        if (UIPlotViewer == null) return;
        Vector3 targetPosition = showPlotViewer ? showPosition : hidePosition;
        Vector3 currentPosition = UIPlotViewer.transform.localPosition;
        Vector3 newPosition = Vector3.Lerp(currentPosition, targetPosition, 5.0f * Time.deltaTime);

        UIPlotViewer.transform.localPosition = newPosition;
        UICategoryButton.SetActive(showPlotViewer);
        UpdateTurnUI();
    }

    // Toggles the plot grid viewer for world building
    public void TogglePlotViewer()
    {
        showPlotViewer = !showPlotViewer;
    }

    // Go back to the isometric world map for quadrant selecting
    public void ViewWorldMap()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ViewingQuadrant = 0;
            showPlotViewer = false;
        }
    }

    // Show individual plot grid
    public void ShowPlotGrid(string gridName)
    {
        // Hide everything first
        HideAllPlotGrids();

        // Show the selected grid based on its name
        switch (gridName)
        {
            case "General":
                if (PlotGridGeneral != null)
                    PlotGridGeneral.SetActive(true);
                break;

            case "Housing":
                if (PlotGridHousing != null)
                    PlotGridHousing.SetActive(true);
                break;

            case "Productivity":
                if (PlotGridProductivity != null)
                    PlotGridProductivity.SetActive(true);
                break;

            case "Recreational":
                if (PlotGridRecreational != null)
                    PlotGridRecreational.SetActive(true);
                break;

            default:
                Debug.LogWarning(gridName + " is not a valid grid name!");
                break;
        }
    }

    // Sets every plot grid's visibility to false
    private void HideAllPlotGrids()
    {
        PlotGridGeneral.SetActive(false);
        PlotGridHousing.SetActive(false);
        PlotGridProductivity.SetActive(false);
        PlotGridRecreational.SetActive(false);
    }

    void UpdateUIBasedOnQuadrant()
    {
        // First hide all quadrant grids
        HideAllQuadrantGrids();

        if (GameManager.Instance.ViewingQuadrant == 0) // If we are not viewing any quadrants
        {
            // Show world map, hide game UI
            if (UIWorldMap != null) UIWorldMap.SetActive(true);
            if (UIGameViewer != null) UIGameViewer.SetActive(false);
            if (UIPlotViewer != null) UIPlotViewer.SetActive(false);
        }
        else // We are viewing one of the four available quadrants
        {
            // Hide world map, show game UI
            if (UIWorldMap != null) UIWorldMap.SetActive(false);
            if (UIGameViewer != null) UIGameViewer.SetActive(true);
            if (UIPlotViewer != null) UIPlotViewer.SetActive(true);

            // Show the specific quadrant grid based on ViewingQuadrant value
            switch (GameManager.Instance.ViewingQuadrant)
            {
                case 1:
                    if (GridQuadrant1 != null) GridQuadrant1.SetActive(true);
                    break;

                case 2:
                    if (GridQuadrant2 != null) GridQuadrant2.SetActive(true);
                    break;

                case 3:
                    if (GridQuadrant3 != null) GridQuadrant3.SetActive(true);
                    break;

                case 4:
                    if (GridQuadrant4 != null) GridQuadrant4.SetActive(true);
                    break;

                default:
                    Debug.LogWarning("Invalid ViewingQuadrant: " + GameManager.Instance.ViewingQuadrant);
                    break;
            }
        }
    }

    // Hide all quadrant grids
    private void HideAllQuadrantGrids()
    {
        if (GridQuadrant1 != null) GridQuadrant1.SetActive(false);
        if (GridQuadrant2 != null) GridQuadrant2.SetActive(false);
        if (GridQuadrant3 != null) GridQuadrant3.SetActive(false);
        if (GridQuadrant4 != null) GridQuadrant4.SetActive(false);
    }

    private void UpdateTurnUI()
    {
        if (GameManager.Instance != null)
        {
            turnsText.text = "Turns: " + GameManager.Instance.CurrentTurn.ToString();
            turnTimeText.text = "Time: " + GameManager.Instance.CurrentTurnTime.ToString("F0") + "s";
        }
    }
}