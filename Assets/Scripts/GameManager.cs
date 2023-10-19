using FishNet.Object;
using TMPro;
using UnityEngine;

[System.Serializable]
public class Wave
{
    public GameObject[] enemies;
    public float spawnInterval;
    public int maximumAmount;
}

public class GameManager : NetworkBehaviour
{
    [Header("Spawn Management")]
    [SerializeField] private int waveDuration = 60;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Wave[] waves = new Wave[30];
    private int currentWaveNumber = -1;

    private Wave currentWave;
    private int enemyAmount;
    private TextMeshProUGUI enemyKilledText;

    private float timeToSpawn;
    private float timeToNextWave;
    private int enemyKilled;

    private int healthMultiplier = 1;
    private float damageMultiplier = 1;
    private float spawnIntervalMultiplier = 1;
    private float maximumAmountMultiplier = 1;

    void Start()
    {
        enemyKilledText = GameObject.Find("HUD/EnemyKilled/Amount").GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (!IsServer) return;

        if (timeToNextWave > 0)
            timeToNextWave -= Time.deltaTime;
        else
            StartWave();

        if (timeToSpawn > 0)
            timeToSpawn -= Time.deltaTime;
        else if (enemyAmount < currentWave.maximumAmount * maximumAmountMultiplier)
        {
            SpawnEnemy();
        }
    }

    public void StartWave()
    {
        if (currentWaveNumber == waves.Length - 1)
        {
            UpgradeWave();
        }
        else
            currentWaveNumber++;
        currentWave = waves[currentWaveNumber];
        timeToNextWave = waveDuration;
        SpawnEnemy();
    }

    void SpawnEnemy()
    {
        timeToSpawn = currentWave.spawnInterval * spawnIntervalMultiplier;
        foreach (GameObject enemyPrefab in currentWave.enemies)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemyGO = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

            Enemy enemy = enemyGO.GetComponent<Enemy>();
            enemy.maxHealth *= healthMultiplier;
            enemy.damage = (int)(enemy.damage * damageMultiplier);
            Spawn(enemyGO);
        }
    }

    public void ZombieKilled()
    {
        enemyKilled++;
        enemyKilledText.text = enemyKilled.ToString();
    }

    void UpgradeWave()
    {
        currentWaveNumber = 0;

        healthMultiplier += 1;
        damageMultiplier += 0.25f;
        spawnIntervalMultiplier += 0.5f;
        maximumAmountMultiplier += 0.5f;
    }
}
