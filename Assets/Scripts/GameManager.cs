using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Observing;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[System.Serializable]
public class Wave
{
    public BiomeType biomeType;
    public GameObject[] enemies;
    public GameObject bossPrefab;
}

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private float waitingTimeBeforeStart = 5f;
    [SerializeField] private int waveDuration = 600;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Wave[] waves = new Wave[5];
    [SerializeField] private float spawnInterval;
    [SerializeField] private int maximumAmount;
    private int waveNumber = 0;
    private int cycleNumber = 0;

    private Wave currentWave;
    private int enemyAmount;
    private int enemyKilled;
    [SerializeField] private TextMeshProUGUI enemyKilledText;
    private List<Wave> availableWaves;
    private float timeToSpawn;
    private float timeToNextWave; 

    [SyncVar] private float gameTimer;
    [SerializeField] private TextMeshProUGUI timer;

    private int healthMultiplier = 1;
    private float damageMultiplier = 1;
    private float spawnIntervalMultiplier = 1;
    private float maximumAmountMultiplier = 1;

    private bool started = false; 
    public bool GameStarted => started;
    private bool canSpawn = true;
    private bool changingBiome = false;

    [SyncObject]
    public readonly SyncList<Player> players = new SyncList<Player>();
    private List<Enemy> enemies = new List<Enemy>();

    private void Awake()
    {
        Instance = this;
        availableWaves = waves.ToList(); 
    }

    void Update()
    {
        if (!IsServer) return;
        
        if (!started)
        {
            waitingTimeBeforeStart -= Time.deltaTime;
            RpcUpdateGameTimer(waitingTimeBeforeStart);
            if (waitingTimeBeforeStart <= 0)
            {
                StartGame();
            }
            return;
        }

        if (!canSpawn) return;

        UpdateGameTimer();

        if (!changingBiome) return;

        if (timeToNextWave > 0)
            timeToNextWave -= Time.deltaTime;
        else
            StartWave();

        if (timeToSpawn > 0)
            timeToSpawn -= Time.deltaTime;
        else if (enemyAmount < maximumAmount * maximumAmountMultiplier)
        {
            SpawnEnemy();
        }
    }

    [Server]
    private void UpdateGameTimer()
    {
        gameTimer += Time.deltaTime;
        RpcUpdateGameTimer(gameTimer);
    }

    [Server]
    private void StartGame()
    {
        Debug.Log("Game Start");
        started = true;
        gameTimer = 0f;
        cycleNumber = 0;
        waveNumber = 0;
        RpcUpdateGameTimer(gameTimer);
    }

    [Server]
    private void StartWave()
    {     
        if(!availableWaves.Any())
        {
            NewCycle();
        }
        else
        {
            if(currentWave != null) availableWaves.Remove(currentWave);
            int randomWave = Random.Range(0, availableWaves.Count);
            currentWave = availableWaves[randomWave];
            MapGenerator.Instance.GenerateMapServer(currentWave.biomeType);
            waveNumber++;
            timeToNextWave = waveDuration;
        }    
    }

    [Server]
    private void SpawnEnemy()
    {
        timeToSpawn = spawnInterval * spawnIntervalMultiplier;
        foreach (GameObject enemyPrefab in currentWave.enemies)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemyGO = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

            Enemy enemy = enemyGO.GetComponent<Enemy>();
            enemy.maxHealth *= healthMultiplier;
            enemy.damage = (int)(enemy.damage * damageMultiplier);
            Spawn(enemyGO);
            enemies.Add(enemy);
        }
    }

    [Server]
    private void SpawnBoss()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject bossGO = Instantiate(currentWave.bossPrefab, spawnPoint.position, Quaternion.identity);

        Enemy boss = bossGO.GetComponent<Enemy>();
        boss.maxHealth *= healthMultiplier;
        boss.damage = (int)(boss.damage * damageMultiplier);
        Spawn(bossGO);
        enemies.Add(boss);
    }

    private void NewCycle()
    {
        changingBiome = true;
        availableWaves = waves.ToList();
        currentWave = null;
        waveNumber = 0;
        cycleNumber++;
        healthMultiplier += 1;
        damageMultiplier += 0.25f;
        spawnIntervalMultiplier += 0.5f;
        maximumAmountMultiplier += 0.5f;
        StartWave();

        Debug.Log("New Cycle started!");
    }

    public int GetLivingPlayers()
    {
        return players.Count(x => !x.IsDead);
    }

    public void ChangeEnemiesStatus(bool status)
    {
        canSpawn = status;
        foreach(Enemy enemy in enemies)
        {
            enemy.ChangeAgentStatus(status);
        }
    }

    public void EnemyKilled(Enemy enemy)
    {
        enemyKilled++;
        enemyKilledText.text = enemyKilled.ToString();
        enemies.Remove(enemy);
    }

    [ObserversRpc]
    private void RpcUpdateGameTimer(float newTime)
    {
        string hours = (newTime / 3600).ToString("00");
        float m = newTime % 3600;
        string minutes = (m / 60).ToString("00");
        string seconds = (m % 60).ToString("00");
        timer.text = $"{hours}:{minutes}:{seconds}";
    }
}
