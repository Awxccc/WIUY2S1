using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;


public class PopulationUI : MonoBehaviour
{

    [Header("References")]
    public GameManager gamemanager;

    public TextMeshProUGUI PopUI;

    void Update()
    {
        int POP = gamemanager.Population;
        if (POP >= 60)
        {
            PopUI.text = $" {POP} / 60";
            PopUI.color = new Color(0.625f, 1f, 0.55f, 1f);
        }
        else
        {
            PopUI.text = $" {POP} / 60";
            PopUI.color = new Color(1f, 0.6f, 0.55f, 1f);

        }
    }
}
