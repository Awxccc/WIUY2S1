using UnityEngine;

public class BuildingProgress : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public Sprite constructionSprite;
    private Sprite finishedSprite;

    private int turnsLeft;
    public PlotManager.PlotData plotData { get; private set; }
    public int currentLevel { get; private set; }
    public bool isComplete { get; private set; }
    public void Initialize(PlotManager.PlotData data, int turnsToBuild)
    {
        plotData = data;
        currentLevel = data.Level;
        finishedSprite = data.PlotImage;
        turnsLeft = turnsToBuild;
        isComplete = false;
        if (spriteRenderer != null && constructionSprite != null)
            spriteRenderer.sprite = constructionSprite;
    }

    public void BuildTurn()
    {
        if (turnsLeft > 0)
        {
            turnsLeft--;
            if (turnsLeft <= 0)
            {
                CompleteBuilding();
            }
        }
    }

    private void CompleteBuilding()
    {
        if (spriteRenderer != null && finishedSprite != null)
        {
            spriteRenderer.sprite = finishedSprite;
        }
        isComplete = true;
    }
}
