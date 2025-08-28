using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class EndScreen : MonoBehaviour
{
    [Header("References")]
    public GameManager gamemanager;
    public TurnCalculations turnCalculations;
    public TextMeshProUGUI resultTextWin;
    public TextMeshProUGUI resultTextLose;

    [Header("Events")]
    public UnityEvent OnWin;
    public UnityEvent OnLose;

    public void ShowEndResult()
    {
        int finalCashChange = turnCalculations.GetCurrentCashChange();
        int POP = gamemanager.Population;
        int GDP = 0;

        if (POP > 0)
        {
            GDP = Mathf.RoundToInt((float)finalCashChange / POP);
        }

        bool hasEnoughPopulation = gamemanager.HasEnoughPopulation(60);

        if (GDP >= 182 && hasEnoughPopulation)
        {
            resultTextWin.text = $"Population: {POP} / 60\nGDP: {GDP} / 182";
            OnWin.Invoke();
            StartCoroutine(FadeText(resultTextWin, 2f)); // 2 seconds fade-in
        }
        else
        {
            resultTextLose.text = $"Population: {POP} / 60\nGDP: {GDP} / 182";
            OnLose.Invoke();
            StartCoroutine(FadeText(resultTextLose, 2f));
        }
    }

    private IEnumerator FadeText(TextMeshProUGUI text, float duration)
    {
        Color originalColor = text.color;
        text.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / duration);
            text.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
    }
}