using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialScript : MonoBehaviour
{
    [Header("Tutorial Content")]
    public Sprite[] tutorialImages;
    public string[] tutorialTexts;

    [Header("UI References")]
    public GameObject tutorialViewer;
    public Image tutorialImage;
    public TextMeshProUGUI tutorialDescription;
    public TextMeshProUGUI tutorialPages;
    public Button nextButton;
    public Button closeButton;

    private TextMeshProUGUI nextButtonText;
    private int currentPage, totalPages;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        totalPages = Mathf.Max(tutorialImages.Length, tutorialTexts.Length);
        nextButtonText = nextButton.GetComponentInChildren<TextMeshProUGUI>();

        nextButton.onClick.AddListener(GoToNextPage);
        closeButton.onClick.AddListener(CloseTutorial);

        tutorialViewer.SetActive(true);
        ShowCurrentPage();
    }

    void ShowCurrentPage()
    {
        bool hasImage = currentPage < tutorialImages.Length && tutorialImages[currentPage] != null;
        tutorialImage.gameObject.SetActive(hasImage);

        if (hasImage)
            tutorialImage.sprite = tutorialImages[currentPage];

        bool hasText = currentPage < tutorialTexts.Length && !string.IsNullOrEmpty(tutorialTexts[currentPage]);
        tutorialDescription.text = hasText ? tutorialTexts[currentPage] : "Tutorial information description will be written here.";

        tutorialPages.text = $"Page {currentPage + 1} / {totalPages}";
        nextButtonText.text = currentPage >= totalPages - 1 ? "Okay, thanks!" : "Next Page";
    }

    public void GoToNextPage()
    {
        currentPage++;

        if (currentPage >= totalPages)
        {
            tutorialViewer.SetActive(false);
            return;
        }

        ShowCurrentPage();
    }

    public void CloseTutorial()
    {
        tutorialViewer.SetActive(false);
    }
}