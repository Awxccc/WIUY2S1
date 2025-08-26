using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public GameObject npcBasePrefab;
    public List<HumanSprites> possibleNpcAppearances = new();
    public List<Transform> npcContainers = new();
    public List<Transform> animalContainers = new();

    public GameObject animalBasePrefab;
    public List<AnimalSprites> possibleAnimalAppearances;
    public int initialAnimalCount = 20;
    public int minimumAnimalCount = 3;

    public LayerMask groundLayer;
    public int maxSpawnAttempts = 20;
    public float minX = -50f;
    public float maxX = 50f;
    public float spawnY = 30f;

    private List<NPCController> activeNpcs = new();
    private List<GameObject> activeAnimals = new();

    private int currentPopulation = 0;
    private HumanSprites.Era lastCheckedEra;

    private bool animalDespawningActive = false;
    private int lastTurnAnimalDespawned = -1;

    private bool initialNpcSpawnDone = false;
    private bool initialAnimalSpawnDone = false;
    private bool isInitialized = false;
    private Dictionary<Transform, bool> animalContainerStates = new();

    void Start()
    {
        foreach (var container in animalContainers)
        {
            animalContainerStates[container] = container.gameObject.activeInHierarchy;
        }
    }


    void Update()
    {
        if (GameManager.Instance == null) return;

        if (!isInitialized)
        {
            if (FindGroundPosition() != null)
            {
                if (!initialNpcSpawnDone) TryInitialNpcSpawn();
                if (!initialAnimalSpawnDone) SpawnInitialAnimals();
            }
        }

        if (!isInitialized) return;

        int currentTurn = GameManager.Instance.CurrentTurn;

        //Update NPC based on Era
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

        ManageAnimalPopulation();

        currentPopulation = GameManager.Instance.Population;
        ManageHumanNpcPopulation();
    }

    private void TryInitialNpcSpawn()
    {
        currentPopulation = GameManager.Instance.Population;
        ManageHumanNpcPopulation();
        lastCheckedEra = GetCurrentEra(GameManager.Instance.CurrentTurn);
        initialNpcSpawnDone = true;
        CheckInitialization();
    }

    private void SpawnInitialAnimals()
    {
        var activeContainers = animalContainers.Where(c => c.gameObject.activeInHierarchy).ToList();
        if (activeContainers.Count == 0)
        {
            initialAnimalSpawnDone = true;
            CheckInitialization();
            return;
        }

        int containerIndex = 0;
        for (int i = 0; i < initialAnimalCount; i++)
        {
            SpawnAnimal(activeContainers[containerIndex]);
            containerIndex = (containerIndex + 1) % activeContainers.Count;
        }

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

    private void ManageHumanNpcPopulation()
    {
        foreach (var container in npcContainers)
        {
            var npcsInContainer = container.GetComponentsInChildren<NPCController>().ToList();

            if (container.gameObject.activeInHierarchy)
            {
                int desiredNpcCount = Mathf.CeilToInt(currentPopulation / 4.0f);

                while (npcsInContainer.Count < desiredNpcCount)
                {
                    SpawnNpc(container);
                    npcsInContainer = container.GetComponentsInChildren<NPCController>().ToList();
                }

                // If we have more than desired, despawn the excess
                while (npcsInContainer.Count > desiredNpcCount)
                {
                    var npcToDespawn = npcsInContainer.Last();
                    activeNpcs.Remove(npcToDespawn);
                    Destroy(npcToDespawn.gameObject);
                    npcsInContainer.Remove(npcToDespawn);
                }
            }
            else
            {
                foreach (var npc in npcsInContainer)
                {
                    activeNpcs.Remove(npc);
                    Destroy(npc.gameObject);
                }
            }
        }
    }

    private void SpawnNpc(Transform parentContainer)
    {
        if (npcBasePrefab == null || possibleNpcAppearances.Count == 0) return;
        Vector2? groundPos = FindGroundPosition();
        if (groundPos.HasValue)
        {
            HumanSprites.Era currentEra = GetCurrentEra(GameManager.Instance.CurrentTurn);
            List<HumanSprites> eraAppearances = possibleNpcAppearances.Where(a => a.era == currentEra).ToList();
            if (eraAppearances.Count == 0) return;

            HumanSprites chosenAppearance = eraAppearances[Random.Range(0, eraAppearances.Count)];
            GameObject newNpcObject = Instantiate(npcBasePrefab, groundPos.Value, Quaternion.identity, parentContainer);

            if (newNpcObject.TryGetComponent<NPCController>(out var npcController))
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

    //ANIMAL METHODS

    private void ManageAnimalPopulation()
    {
        activeAnimals.RemoveAll(item => item == null);

        foreach (var container in animalContainers)
        {
            bool wasActive = animalContainerStates.ContainsKey(container) && animalContainerStates[container];
            bool isActive = container.gameObject.activeInHierarchy;

            if (wasActive && !isActive)
            {
                //Container become inactive and despawn animals
                var animalsInContainer = container.GetComponentsInChildren<Transform>()
                    .Where(t => t.parent == container && activeAnimals.Contains(t.gameObject))
                    .Select(t => t.gameObject)
                    .ToList();

                foreach (var animal in animalsInContainer)
                {
                    activeAnimals.Remove(animal);
                    Destroy(animal);
                }
            }
            else if (!wasActive && isActive)
            {
                //Container become active and respawn animals
                int animalsToSpawn = initialAnimalCount / animalContainers.Count(c => c.gameObject.activeInHierarchy);
                for (int i = 0; i < animalsToSpawn; i++)
                {
                    SpawnAnimal(container);
                }
            }

            animalContainerStates[container] = isActive;
        }

        if (animalDespawningActive && GameManager.Instance.CurrentTurn > lastTurnAnimalDespawned && activeAnimals.Count > minimumAnimalCount)
        {
            lastTurnAnimalDespawned = GameManager.Instance.CurrentTurn;
            DespawnOneAnimal();
        }
    }


    private void SpawnAnimal(Transform parentContainer)
    {
        if (animalBasePrefab == null || possibleAnimalAppearances.Count == 0) return;
        Vector2? groundPos = FindGroundPosition();
        if (groundPos.HasValue)
        {
            AnimalSprites chosenAppearance = possibleAnimalAppearances[Random.Range(0, possibleAnimalAppearances.Count)];

            GameObject newAnimalObject = Instantiate(animalBasePrefab, groundPos.Value, Quaternion.identity, parentContainer);

            var sr = newAnimalObject.GetComponentInChildren<SpriteRenderer>();
            var anim = newAnimalObject.GetComponentInChildren<Animator>();
            if (sr) sr.sprite = chosenAppearance.sprite;
            if (anim) anim.runtimeAnimatorController = chosenAppearance.animatorOverride;

            if (sr != null)
            {
                float halfHeight = sr.bounds.extents.y;
                newAnimalObject.transform.position = new Vector2(groundPos.Value.x, groundPos.Value.y + halfHeight);
            }

            activeAnimals.Add(newAnimalObject);
        }
    }

    private void DespawnOneAnimal()
    {
        activeAnimals.RemoveAll(item => item == null);
        if (activeAnimals.Count > minimumAnimalCount)
        {
            GameObject animalToDespawn = activeAnimals[Random.Range(0, activeAnimals.Count)];
            activeAnimals.Remove(animalToDespawn);
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
        Vector2 raycastStart = new(randomX, spawnY);
        RaycastHit2D hit = Physics2D.Raycast(raycastStart, Vector2.down, 100f, groundLayer);
        return hit.collider != null ? (Vector2?)hit.point : null;
    }
}