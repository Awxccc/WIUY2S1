using UnityEngine;
using System.Collections;

public class UICloudScript : MonoBehaviour
{
    [Header("Cloud Settings")]
    public GameObject cloudPrefab;
    public Transform cloudParent;
    public float spawnInterval;
    public float maxSpeed;
    public float minSpeed;
    public float maxScale;
    public float minScale;

    [Header("Canvas Positions")]
    public float spawnX;
    public float despawnX;
    public float maxY;
    public float minY;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SpawnClouds());
    }

    void SpawnCloudPrefab()
    {
        GameObject newCloud = Instantiate(cloudPrefab, cloudParent);
        RectTransform cloudRect = newCloud.GetComponent<RectTransform>();

        float randomY = Random.Range(minY, maxY);
        cloudRect.anchoredPosition = new Vector2(spawnX, randomY);

        float randomScale = Random.Range(minScale, maxScale);
        cloudRect.localScale = Vector3.one * randomScale;

        float randomSpeed = Random.Range(minSpeed, maxSpeed);
        StartCoroutine(MoveCloud(newCloud, randomSpeed));
    }

    IEnumerator SpawnClouds()
    {
        while (true)
        {
            SpawnCloudPrefab();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    IEnumerator MoveCloud(GameObject cloud, float speed)
    {
        RectTransform cloudRect = cloud.GetComponent<RectTransform>();

        while (cloudRect.anchoredPosition.x < despawnX)
        {
            Vector2 currentPos = cloudRect.anchoredPosition;
            currentPos.x += speed * Time.deltaTime;
            cloudRect.anchoredPosition = currentPos;

            yield return null;
        }

        Destroy(cloud);
    }
}