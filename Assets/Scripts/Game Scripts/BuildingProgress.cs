using TMPro;
using UnityEngine;

public class BuildingProgress : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public Sprite constructionSprite;
    private Sprite finishedSprite;
    public GameObject progressUIPrefab;
    private GameObject progressUIInstance;
    private TextMeshProUGUI turnsText;
    private int turnsLeft;

    private Vector3 initialUIScale;
    private bool hasInitialized = false;
    public int TurnsLeft => turnsLeft;
    public PlotData PlotData { get; private set; }
    public int CurrentLevel { get; private set; }
    public bool IsComplete { get; private set; }
    public void Initialize(PlotData data, int turnsToBuild)
    {
        PlotData = data;
        CurrentLevel = data.Level;
        finishedSprite = data.PlotImage;
        turnsLeft = turnsToBuild;
        IsComplete = false;
        if (spriteRenderer != null && constructionSprite != null)
            spriteRenderer.sprite = constructionSprite;

        if (progressUIPrefab != null)
        {
            float spriteHeight = spriteRenderer.bounds.size.y;
            Vector3 uiPosition = transform.position + new Vector3(0, (spriteHeight / 2) + 0.5f, 0);

            progressUIInstance = Instantiate(progressUIPrefab, uiPosition, Quaternion.identity, transform);

            initialUIScale = progressUIInstance.transform.localScale;

            turnsText = progressUIInstance.GetComponentInChildren<TextMeshProUGUI>();
            UpdateProgressText();
        }
        hasInitialized = true;
    }
    void LateUpdate()
    {
        if (hasInitialized && progressUIInstance != null)
        {
            progressUIInstance.transform.localScale = new Vector3(
                initialUIScale.x / transform.localScale.x,
                initialUIScale.y / transform.localScale.y,
                initialUIScale.z / transform.localScale.z
            );
        }
    }
    public void BuildTurn()
    {
        if (turnsLeft > 0)
        {
            turnsLeft--;
            UpdateProgressText();
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
        IsComplete = true;
        if (progressUIInstance != null)
        {
            Destroy(progressUIInstance);
        }
        if (PlotData.PlotCategory == PlotManager.PlotCategory.Housing)
        {
            GameManager.Instance.AddPopulation(PlotData.GainPopulation);
        }
    }
    private void UpdateProgressText()
    {
        if (turnsText != null)
        {
            turnsText.text = $"Turns: {turnsLeft}";
        }
    }
}
