using FishNet.Object;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private GameObject[] islandPrefabs;

    [SerializeField] private int mapSizeX = 10;
    [SerializeField] private int mapSizeZ = 10;
    [SerializeField] private float tileSize = 1f;

    [SerializeField] private float islandRadiusMin = 10f;
    [SerializeField] private float islandRadiusMax = 25f;
    [SerializeField] private float islandDensityMin = 0.7f;
    [SerializeField] private float islandDensityMax = 1f;

    private NavMeshSurface navMeshSurface;

    void Start()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
        int seed = Random.Range(int.MinValue, int.MaxValue);
        InitializeWorld(seed);
    }

    //public override void OnStartClient()
    //{
    //    base.OnStartClient();

    //    if(IsServer)
    //    {
    //        int seed = Random.Range(int.MinValue, int.MaxValue);
    //        InitializeWorld(seed);
    //    }
    //}

   // [ObserversRpc]
    private void InitializeWorld(int seed)
    {
        Random.InitState(seed);

        GenerateMap();
    }

    private void Update()
    {
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            GenerateMap();
        }
    }

    void GenerateMap()
    {
        foreach (Transform t in transform)
        {
            Destroy(t.gameObject);
        }

        float islandRadius = Random.Range(islandRadiusMin, islandRadiusMax);
        float islandDensity = Random.Range(islandDensityMin, islandDensityMax);

        for (int x = 0; x < mapSizeX; x++)
        {
            for (int z = 0; z < mapSizeZ; z++)
            {
                Vector2 hexPosition = CalculateHexPosition(x, z);
                Vector3 position = new Vector3(hexPosition.x, 0, hexPosition.y);

                bool isWater = !IsIsland(x, z, islandRadius, islandDensity);
                if (isWater) continue;

                int random = Random.Range(0, 2);
                Instantiate(islandPrefabs[random], position, Quaternion.identity, transform);
            }
        }

        navMeshSurface.BuildNavMesh();
    }

    bool IsIsland(int x, int z, float islandRadius, float islandDensity)
    {
        float centerX = mapSizeX / 2;
        float centerZ = mapSizeZ / 2;

        float distanceToCenter = Mathf.Sqrt((x - centerX) * (x - centerX) + (z - centerZ) * (z - centerZ));

        return distanceToCenter <= islandRadius && Random.value < islandDensity;
    }

    Vector2 CalculateHexPosition(int x, int z)
    {
        float xPos = x * tileSize + ((z % 2 == 1) ? tileSize * 0.5f : 0);
        float zPos = z * tileSize * Mathf.Cos(Mathf.Deg2Rad * 30);  

        return new Vector2(xPos, zPos);
    }
}
