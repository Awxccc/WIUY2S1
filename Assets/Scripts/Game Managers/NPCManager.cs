using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    // === HUMAN NPC SETTINGS ===
    [Header("Human NPC Settings")]
    public GameObject npcBasePrefab;
    public List<NPCAppearance> possibleNpcAppearances = new List<NPCAppearance>();
    public List<Transform> npcContainers = new List<Transform>();

    // === ANIMAL SETTINGS ===
    [Header("Animal Settings")]
    [Tooltip("The base prefab for animals. It MUST have the NPCController script attached.")]
    public GameObject animalBasePrefab;
    [Tooltip("A list of all possible animal appearances.")]
    public List<AnimalAppearance> possibleAnimalAppearances;
    [Tooltip("The number of animals to spawn at the start of the game.")]
    public int initialAnimalCount = 20;
    [Tooltip("The minimum number of animals to leave when despawning.")]
    public int minimumAnimalCount = 3;

    // === GENERAL SETTINGS ===
    [Header("General Spawning Settings")]
    public LayerMask groundLayer;
    public int maxSpawnAttempts = 20;
    public float minX = -50f;
    public float maxX = 50f;
    public float spawnY = 30f;

    // --- Private variables for tracking ---
    private List<NPCController> activeNpcs = new List<NPCController>();
    private List<GameObject> activeAnimals = new List<GameObject>();

    // NPC Tracking
    private int currentPopulation = 0;
    private NPCAppearance.Era lastCheckedEra;

    // Animal Tracking
    private bool animalDespawningActive = false;
    private int lastTurnAnimalDespawned = -1;

    // Initialization Flags
    private bool initialNpcSpawnDone = false;
    private bool initialAnimalSpawnDone = false;
    private bool isInitialized = false;

    void Update()
    {
        if (GameManager.Instance == null) return;

        // --- INITIALIZATION ---
        // Both initial spawns must wait for the ground to be active.
        if (!initialNpcSpawnDone || !initialAnimalSpawnDone)
        {
            if (FindGroundPosition() != null)
            {
                if (!initialNpcSpawnDone) TryInitialNpcSpawn();
                if (!initialAnimalSpawnDone) SpawnInitialAnimals();
            }
        }

        if (!isInitialized) return; // Wait for initialization to complete

        int currentTurn = GameManager.Instance.CurrentTurn;

        // --- HUMAN NPC LOGIC ---
        // 1. Check for population increases
        if (GameManager.Instance.Population > currentPopulation)
        {
            int populationDifference = GameManager.Instance.Population - currentPopulation;
            for (int i = 0; i < populationDifference; i++) SpawnNpc();
            currentPopulation = GameManager.Instance.Population;
        }

        // 2. Check for era change to update NPC appearances
        NPCAppearance.Era currentEra = GetCurrentEra(currentTurn);
        if (currentEra != lastCheckedEra)
        {
            UpdateAllNpcAppearances(currentEra);
            lastCheckedEra = currentEra;
        }

        // --- ANIMAL LOGIC ---
        // 1. Check if we should start despawning animals
        if (!animalDespawningActive && currentEra == NPCAppearance.Era.Modern)
        {
            Debug.Log("Modern Era reached. Animal population will now decline.");
            animalDespawningActive = true;
        }

        // 2. If despawning is active, remove one animal per new turn
        if (animalDespawningActive && currentTurn > lastTurnAnimalDespawned)
        {
            lastTurnAnimalDespawned = currentTurn;
            DespawnOneAnimal();
        }
    }

    // --- INITIAL SPAWNERS ---
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
        Debug.Log($"Spawning {initialAnimalCount} initial animals.");
        for (int i = 0; i < initialAnimalCount; i++) SpawnAnimal();
        initialAnimalSpawnDone = true;
        CheckInitialization();
    }

    private void CheckInitialization()
    {
        if (initialNpcSpawnDone && initialAnimalSpawnDone)
        {
            isInitialized = true;
            Debug.Log("NPC and Animal Managers are initialized.");
        }
    }

    // --- HUMAN NPC METHODS ---
    private void SpawnNpc()
    {
        if (npcBasePrefab == null || npcContainers.Count == 0 || possibleNpcAppearances.Count == 0) return;
        Vector2? groundPos = FindGroundPosition();
        if (groundPos.HasValue)
        {
            NPCAppearance.Era currentEra = GetCurrentEra(GameManager.Instance.CurrentTurn);
            List<NPCAppearance> eraAppearances = possibleNpcAppearances.Where(a => a.era == currentEra).ToList();
            if (eraAppearances.Count == 0) return;

            NPCAppearance chosenAppearance = eraAppearances[Random.Range(0, eraAppearances.Count)];
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

    private void UpdateAllNpcAppearances(NPCAppearance.Era newEra)
    {
        List<NPCAppearance> eraAppearances = possibleNpcAppearances.Where(a => a.era == newEra).ToList();
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
            AnimalAppearance chosenAppearance = possibleAnimalAppearances[Random.Range(0, possibleAnimalAppearances.Count)];
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

    // --- HELPER METHODS ---
    private NPCAppearance.Era GetCurrentEra(int turn)
    {
        if (turn <= 40) return NPCAppearance.Era.Founding;
        if (turn <= 80) return NPCAppearance.Era.Golden;
        return NPCAppearance.Era.Modern;
    }

    private Vector2? FindGroundPosition()
    {
        float randomX = Random.Range(minX, maxX);
        Vector2 raycastStart = new Vector2(randomX, spawnY);
        RaycastHit2D hit = Physics2D.Raycast(raycastStart, Vector2.down, 100f, groundLayer);
        return hit.collider != null ? (Vector2?)hit.point : null;
    }
}