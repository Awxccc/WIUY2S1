using UnityEngine;

public class BuildingProgress : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public Sprite constructionSprite;
    private Sprite finishedSprite;

    private int turnsLeft;

    public void Initialize(Sprite finished, int turnsToBuild)
    {
        finishedSprite = finished;
        turnsLeft = turnsToBuild;

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
    }
}
