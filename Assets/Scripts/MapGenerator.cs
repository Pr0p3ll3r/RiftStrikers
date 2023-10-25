using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class MapGenerator : MonoBehaviour
{
    public GameObject[] islandPrefabs;
    public GameObject waterPrefab;

    public int mapSizeX = 10;
    public int mapSizeY = 10;

    public float islandRadiusMin = 10;
    public float islandRadiusMax = 25;

    private NavMeshSurface navMeshSurface;

    void Start()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
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

        // Losowo wybierz promie� wyspy przy ka�dym uruchomieniu
        float islandRadius = Random.Range(islandRadiusMin, islandRadiusMax);

        // Losowo wybierz g�sto�� wyspy przy ka�dym uruchomieniu
        float islandDensity = Random.Range(0.7f, 0.95f); // Dostosuj zakres wed�ug preferencji

        // Iteruj przez wsp�rz�dne x i y
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                Vector3 hexPosition = CalculateHexPosition(x, y);

                // Je�li punkt znajduje si� wewn�trz obszaru wyspy, utw�rz element
                if (IsInsideIsland(x, y, islandRadius, islandDensity))
                {
                    int random = Random.Range(0, 2);
                    GameObject island = Instantiate(islandPrefabs[random], hexPosition, Quaternion.identity, transform);
                    island.transform.rotation = Quaternion.Euler(90, 0, 0);
                }
                else
                {
                    // W przeciwnym razie utw�rz element wody
                    GameObject water = Instantiate(waterPrefab, hexPosition - new Vector3(0, 0.5f, 0), Quaternion.identity, transform);
                    water.transform.rotation = Quaternion.Euler(90, 0, 0);
                }
            }
        }

        navMeshSurface.BuildNavMesh();
    }

    bool IsInsideIsland(int x, int y, float islandRadius, float islandDensity)
    {
        // Okre�l �rodek mapy
        float centerX = mapSizeX / 2;
        float centerY = mapSizeY / 2;

        // Oblicz odleg�o�� od punktu (x, y) do �rodka mapy
        float distanceToCenter = Mathf.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));

        // Je�li punkt znajduje si� wewn�trz promienia i spe�nia warunek g�sto�ci, zwr�� true
        return distanceToCenter <= islandRadius && Random.value < islandDensity;
    }

    Vector3 CalculateHexPosition(int x, int y)
    {
        float hexWidth = 3.46f;
        float hexHeight = 2.99f;

        float offsetX = x * hexWidth;
        float offsetY = y * hexHeight;

        // Dla nieparzystych wierszy, przesu� co drugi rz�d w prawo
        if (y % 2 != 0)
        {
            offsetX += hexWidth * 0.5f;
        }

        float xPosition = offsetX;
        float zPosition = offsetY;

        return new Vector3(xPosition, 0, zPosition);
    }
}
