using UnityEngine;
using System.Collections;

public class WeatherScript : MonoBehaviour
{
    public GameObject[] gameObjects = new GameObject[4];
    private bool isRaining = false;

    void Start()
    {
        StartCoroutine(CheckParentActiveRoutine());
        StartCoroutine(RainRoutine());
    }

    IEnumerator CheckParentActiveRoutine()
    {
        while (true)
        {
            foreach (GameObject obj in gameObjects)
            {
                if (obj.transform.parent.gameObject.activeSelf)
                {
                    obj.SetActive(isRaining);
                }
                else
                {
                    obj.SetActive(false);
                }
            }
            yield return new WaitForSeconds(30f);
        }
    }

    IEnumerator RainRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(15, 31));
            isRaining = true;
            foreach (GameObject obj in gameObjects)
            {
                if (obj.transform.parent.gameObject.activeSelf)
                {
                    obj.SetActive(true);
                }
            }
            yield return new WaitForSeconds(Random.Range(20, 41));
            isRaining = false;
            foreach (GameObject obj in gameObjects)
            {
                if (obj.transform.parent.gameObject.activeSelf)
                {
                    obj.SetActive(false);
                }
            }
        }
    }
}