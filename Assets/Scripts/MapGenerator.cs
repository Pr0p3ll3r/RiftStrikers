using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.XR;

public enum BiomeType
{
    Forest,
    Desert,
    Winter,
    Beach,
    Swamp
}

[System.Serializable]
public class Biome
{
    public BiomeType biomeType;
    public GameObject[] landPrefabs;
    public GameObject[] naturePrefabs;
}

public class MapGenerator : NetworkBehaviour
{
    public static MapGenerator Instance { get; private set; }

    [SerializeField] private List<Biome> biomes;
    [SerializeField] private GameObject waterCollider;
    [SerializeField] private int natureCount = 10;

    [SerializeField] private int mapSizeX = 10;
    [SerializeField] private int mapSizeZ = 10;
    [SerializeField] private float tileSize = 1f;

    [SerializeField] private float islandRadiusMin = 10f;
    [SerializeField] private float islandRadiusMax = 25f;
    [SerializeField] private float islandDensityMin = 0.7f;
    [SerializeField] private float islandDensityMax = 1f;
    [SerializeField] private GameObject teleportPrefab;

    private float timeForGetToTeleport = 30.0f;
    private float timeForGenerateMap = 3f;
    private NavMeshSurface navMeshSurface;
    private int selectedBiome;
    private List<GameObject> lands;
    private List<GameObject> emptyLands;
    private GameObject teleport;
    private int playersInTeleport;
    private bool teleportSpawned = false;

    private void Awake()
    {
        Instance = this;
        navMeshSurface = GetComponent<NavMeshSurface>();
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        if (IsServer)
        {
            int seed = Random.Range(int.MinValue, int.MaxValue);
            SetSeed(seed);
        }
    }

    private void Update()
    {
        if(!GameManager.Instance.GamePause && teleportSpawned)
        {
            Debug.Log(GameManager.Instance.GamePause);
            timeForGetToTeleport -= Time.deltaTime;
            if(timeForGetToTeleport <= 0)
            {
                DestroyMapRpc();
            }
        }
    }

    [ObserversRpc(BufferLast = true)]
    private void SetSeed(int seed)
    {
        Random.InitState(seed);
    }

    [ObserversRpc(BufferLast = true)]
    public void GenerateMapRpc(BiomeType selectedBiome)
    {
        playersInTeleport = 0;
        teleportSpawned = false;
        int natureCountCurrent = natureCount;
        lands = new List<GameObject>();
        emptyLands = new List<GameObject>();
        foreach (Transform t in transform)
        {
            Destroy(t.gameObject);
        }

        float islandRadius = Random.Range(islandRadiusMin, islandRadiusMax);
        float islandDensity = Random.Range(islandDensityMin, islandDensityMax);

        int totalLandHexagons = 0;
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int z = 0; z < mapSizeZ; z++)
            {
                if (IsIsland(x, z, islandRadius, islandDensity))
                {
                    totalLandHexagons++;
                }
            }
        }

        for (int x = 0; x < mapSizeX; x++)
        {
            for (int z = 0; z < mapSizeZ; z++)
            {
                Vector2 hexPosition = CalculateHexPosition(x, z);
                Vector3 position = new Vector3(hexPosition.x, 0, hexPosition.y);

                if(IsIsland(x, z, islandRadius, islandDensity))
                {
                    if (natureCountCurrent > 0 && Random.value < (float)natureCountCurrent / totalLandHexagons)
                    {
                        GameObject naturePrefab = GetNaturePrefab(GetBiome(selectedBiome).naturePrefabs);
                        GameObject land = Instantiate(naturePrefab, position, Quaternion.identity, transform);
                        lands.Add(land);
                        natureCountCurrent--;
                    }
                    else
                    {
                        GameObject landPrefab = GetLandPrefab(GetBiome(selectedBiome).landPrefabs);
                        GameObject land = Instantiate(landPrefab, position, Quaternion.identity, transform);
                        lands.Add(land);
                        emptyLands.Add(land);
                    }
                }
                else
                {
                    Instantiate(waterCollider, position, Quaternion.identity, transform);
                }
            }
        }

        navMeshSurface.BuildNavMesh();
    }

    private bool IsIsland(int x, int z, float islandRadius, float islandDensity)
    {
        float centerX = mapSizeX / 2;
        float centerZ = mapSizeZ / 2;

        float distanceToCenter = Mathf.Sqrt((x - centerX) * (x - centerX) + (z - centerZ) * (z - centerZ));

        return distanceToCenter <= islandRadius && Random.value < islandDensity;
    }

    [ObserversRpc]
    private void DestroyMapRpc()
    {
        foreach (GameObject land in lands)
        {
            land.SetActive(false);
        }
    }

    public void StartRemoving()
    {
        StartCoroutine(ShakeAndRemove());
    }

    private IEnumerator ShakeAndRemove()
    {
        StartShakingRpc();

        SpawnTeleport();

        while(playersInTeleport != GameManager.Instance.GetLivingPlayers())
        {
            yield return new WaitForSeconds(0.1f);
        }

        StartDarkeningScreenRpc();
    }

    [ObserversRpc]
    private void StartShakingRpc()
    {
        foreach (GameObject land in lands)
        {
            land.GetComponent<Shake>().StartShaking();
        }
    }

    public void PlayersInPortal()
    {
        playersInTeleport++;
    }

    private void SpawnTeleport()
    {
        GameObject randomLand = GetRandomEmptyLand();
        teleport = Instantiate(teleportPrefab, randomLand.transform.position + Vector3.up, Quaternion.identity);
        Spawn(teleport);
        teleportSpawned = true;
    }

    [ObserversRpc]
    private void StartDarkeningScreenRpc()
    {
        StartCoroutine(DarkenScreen());
    }

    private IEnumerator DarkenScreen()
    {
        yield return LevelLoader.Instance.StartCoroutine(LevelLoader.Instance.Crossfade(timeForGenerateMap));
        GameManager.Instance.ChangingBiome = false;
        if (IsServer)
        {        
            teleportSpawned = false;
            playersInTeleport = 0;
            Despawn(teleport);
        }
    }

    private Vector2 CalculateHexPosition(int x, int z)
    {
        float xPos = x * tileSize + ((z % 2 == 1) ? tileSize * 0.5f : 0);
        float zPos = z * tileSize * Mathf.Cos(Mathf.Deg2Rad * 30);  

        return new Vector2(xPos, zPos);
    }
    
    public GameObject GetRandomEmptyLand()
    {
        return emptyLands[Random.Range(0, emptyLands.Count)];
    }

    private GameObject GetLandPrefab(GameObject[] prefabs)
    {
        return prefabs[Random.Range(0, prefabs.Length)];
    }

    private GameObject GetNaturePrefab(GameObject[] prefabs)
    {
        return prefabs[Random.Range(0, prefabs.Length)];
    }

    public Biome GetBiome(BiomeType biomeType)
    {
        return biomes.Find(x => x.biomeType == biomeType);
    }
}
