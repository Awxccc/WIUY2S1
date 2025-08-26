using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class EndScreen : MonoBehaviour
{
    [Header("References")]
    public GameManager gamemanager;
    public TextMeshProUGUI resultTextWin;
    public TextMeshProUGUI resultTextLose;

    [Header("Events")]
    public UnityEvent OnWin;
    public UnityEvent OnLose;

    private bool hasTriggered = false;

    private void Update()
    {
        if (!hasTriggered && gamemanager.CurrentTurn >= gamemanager.MaximumTurn + 1)
        {
            ShowEndResult();
            hasTriggered = true;
        }
    }

    private void ShowEndResult()
    {
        int GDP = gamemanager.turnCalculations.GDPcalculator();

        if (GDP > 91000)
        {
            resultTextWin.text = $"GDP: {GDP} / 91000\n<size=120%><color=green>You Win!</color></size>";
            OnWin.Invoke();
            StartCoroutine(FadeText(resultTextWin, 2f)); // 2 seconds fade-in
        }
        else
        {
            resultTextLose.text = $"GDP: {GDP} / 91000\n<size=120%><color=red>You Lose!</color></size>";
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










