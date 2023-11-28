using FishNet.Object;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.InputSystem;

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

    private NavMeshSurface navMeshSurface;
    private int selectedBiome;

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

    [ObserversRpc(BufferLast = true)]
    private void SetSeed(int seed)
    {
        Random.InitState(seed);
    }

    public void GenerateMapServer(BiomeType selectedBiome)
    { 
        GenerateMapRpc(selectedBiome);
    }

    [ObserversRpc(BufferLast = true)]
    public void GenerateMapRpc(BiomeType selectedBiome)
    {
        int natureCountCurrent = natureCount;
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
                        Instantiate(naturePrefab, position, Quaternion.identity, transform);
                        natureCountCurrent--;
                    }
                    else
                    {
                        GameObject landPrefab = GetLandPrefab(GetBiome(selectedBiome).landPrefabs);
                        Instantiate(landPrefab, position, Quaternion.identity, transform);
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

    private Vector2 CalculateHexPosition(int x, int z)
    {
        float xPos = x * tileSize + ((z % 2 == 1) ? tileSize * 0.5f : 0);
        float zPos = z * tileSize * Mathf.Cos(Mathf.Deg2Rad * 30);  

        return new Vector2(xPos, zPos);
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
