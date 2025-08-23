using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public GameObject npcBasePrefab;
    public List<HumanSprites> possibleNpcAppearances = new List<HumanSprites>();
    public List<Transform> npcContainers = new List<Transform>();

    public GameObject animalBasePrefab;
    public List<AnimalSprites> possibleAnimalAppearances;
    public int initialAnimalCount = 20;
    public int minimumAnimalCount = 3;

    public LayerMask groundLayer;
    public int maxSpawnAttempts = 20;
    public float minX = -50f;
    public float maxX = 50f;
    public float spawnY = 30f;

    private List<NPCController> activeNpcs = new List<NPCController>();
    private List<GameObject> activeAnimals = new List<GameObject>();

    private int currentPopulation = 0;
    private HumanSprites.Era lastCheckedEra;

    private bool animalDespawningActive = false;
    private int lastTurnAnimalDespawned = -1;

    private bool initialNpcSpawnDone = false;
    private bool initialAnimalSpawnDone = false;
    private bool isInitialized = false;

    void Update()
    {
        if (GameManager.Instance == null) return;

        if (!initialNpcSpawnDone || !initialAnimalSpawnDone)
        {
            if (FindGroundPosition() != null)
            {
                if (!initialNpcSpawnDone) TryInitialNpcSpawn();
                if (!initialAnimalSpawnDone) SpawnInitialAnimals();
            }
        }

        if (!isInitialized) return;

        int currentTurn = GameManager.Instance.CurrentTurn;

        if (GameManager.Instance.Population > currentPopulation)
        {
            int populationDifference = GameManager.Instance.Population - currentPopulation;
            for (int i = 0; i < populationDifference; i++) SpawnNpc();
            currentPopulation = GameManager.Instance.Population;
        }

        HumanSprites.Era currentEra = GetCurrentEra(currentTurn);
        if (currentEra != lastCheckedEra)
        {
            UpdateAllNpcAppearances(currentEra);
            lastCheckedEra = currentEra;
        }

        if (!animalDespawningActive && currentEra == HumanSprites.Era.Modern)
        {
            animalDespawningActive = true;
        }

        if (animalDespawningActive && currentTurn > lastTurnAnimalDespawned)
        {
            lastTurnAnimalDespawned = currentTurn;
            DespawnOneAnimal();
        }
    }

    private void TryInitialNpcSpawn()
    {
        currentPopulation = GameManager.Instance.Population;
        for (int i = 0; i < currentPopulation; i++) SpawnNpc();

        lastCheckedEra = GetCurrentEra(GameManager.Instance.CurrentTurn);
        initialNpcSpawnDone = true;
        CheckInitialization();
    }

    private void SpawnInitialAnimals()
    {
        for (int i = 0; i < initialAnimalCount; i++) SpawnAnimal();
        initialAnimalSpawnDone = true;
        CheckInitialization();
    }

    private void CheckInitialization()
    {
        if (initialNpcSpawnDone && initialAnimalSpawnDone)
        {
            isInitialized = true;
        }
    }
    private void SpawnNpc()
    {
        if (npcBasePrefab == null || npcContainers.Count == 0 || possibleNpcAppearances.Count == 0) return;
        Vector2? groundPos = FindGroundPosition();
        if (groundPos.HasValue)
        {
            HumanSprites.Era currentEra = GetCurrentEra(GameManager.Instance.CurrentTurn);
            List<HumanSprites> eraAppearances = possibleNpcAppearances.Where(a => a.era == currentEra).ToList();
            if (eraAppearances.Count == 0) return;

            HumanSprites chosenAppearance = eraAppearances[Random.Range(0, eraAppearances.Count)];
            GameObject newNpcObject = Instantiate(npcBasePrefab, groundPos.Value, Quaternion.identity);

            NPCController npcController = newNpcObject.GetComponent<NPCController>();
            if (npcController != null)
            {
                npcController.SetAppearance(chosenAppearance);
                activeNpcs.Add(npcController);
            }

            var sr = newNpcObject.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                float halfHeight = sr.bounds.extents.y;
                newNpcObject.transform.position = new Vector2(groundPos.Value.x, groundPos.Value.y + halfHeight);
            }

            newNpcObject.transform.SetParent(npcContainers[Random.Range(0, npcContainers.Count)]);
        }
    }

    private void UpdateAllNpcAppearances(HumanSprites.Era newEra)
    {
        List<HumanSprites> eraAppearances = possibleNpcAppearances.Where(a => a.era == newEra).ToList();
        if (eraAppearances.Count == 0) return;

        foreach (NPCController npc in activeNpcs)
        {
            if (npc != null)
            {
                npc.SetAppearance(eraAppearances[Random.Range(0, eraAppearances.Count)]);
            }
        }
        activeNpcs.RemoveAll(item => item == null);
    }

    // --- ANIMAL METHODS ---
    private void SpawnAnimal()
    {
        if (animalBasePrefab == null || npcContainers.Count == 0 || possibleAnimalAppearances.Count == 0) return;
        Vector2? groundPos = FindGroundPosition();
        if (groundPos.HasValue)
        {
            AnimalSprites chosenAppearance = possibleAnimalAppearances[Random.Range(0, possibleAnimalAppearances.Count)];
            GameObject newAnimalObject = Instantiate(animalBasePrefab, groundPos.Value, Quaternion.identity);

            var sr = newAnimalObject.GetComponentInChildren<SpriteRenderer>();
            var anim = newAnimalObject.GetComponentInChildren<Animator>();
            if (sr) sr.sprite = chosenAppearance.sprite;
            if (anim) anim.runtimeAnimatorController = chosenAppearance.animatorOverride;

            if (sr != null)
            {
                float halfHeight = sr.bounds.extents.y;
                newAnimalObject.transform.position = new Vector2(groundPos.Value.x, groundPos.Value.y + halfHeight);
            }

            newAnimalObject.transform.SetParent(npcContainers[Random.Range(0, npcContainers.Count)]);
            activeAnimals.Add(newAnimalObject);
        }
    }

    private void DespawnOneAnimal()
    {
        activeAnimals.RemoveAll(item => item == null);
        if (activeAnimals.Count > minimumAnimalCount)
        {
            GameObject animalToDespawn = activeAnimals[0];
            activeAnimals.RemoveAt(0);
            Destroy(animalToDespawn);
        }
    }

    private HumanSprites.Era GetCurrentEra(int turn)
    {
        if (turn <= 40) return HumanSprites.Era.Founding;
        if (turn <= 80) return HumanSprites.Era.Golden;
        return HumanSprites.Era.Modern;
    }

    private Vector2? FindGroundPosition()
    {
        float randomX = Random.Range(minX, maxX);
        Vector2 raycastStart = new Vector2(randomX, spawnY);
        RaycastHit2D hit = Physics2D.Raycast(raycastStart, Vector2.down, 100f, groundLayer);
        return hit.collider != null ? (Vector2?)hit.point : null;
    }
}