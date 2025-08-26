using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIMenuScene : MonoBehaviour
{
    [Header("UI References")]
    public Button UIPlayButton;
    public SceneLoader sceneLoader;

    [Header("Audio References")]
    public AudioSource BGMAudioSource;
    public AudioSource SFXAudioSource;
    public AudioClip buttonClickSFX;

    private Animator canvasAnimator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvasAnimator = GetComponent<Animator>();

        if (UIPlayButton != null)
            UIPlayButton.onClick.AddListener(OnPlayButtonClicked);
    }

    void OnPlayButtonClicked()
    {
        if (canvasAnimator != null)
            canvasAnimator.SetTrigger("Transition");

        if (BGMAudioSource != null)
            StartCoroutine(FadeOutBGM(0.5f));

        if (SFXAudioSource != null && buttonClickSFX != null)
            SFXAudioSource.PlayOneShot(buttonClickSFX);

        if (sceneLoader != null)
            sceneLoader.LoadScene("GameScene");
    }

    private IEnumerator FadeOutBGM(float fadeTime)
    {
        float startVolume = BGMAudioSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float newVolume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeTime);
            BGMAudioSource.volume = newVolume;
            yield return null;
        }

        BGMAudioSource.volume = 0f;
    }
}