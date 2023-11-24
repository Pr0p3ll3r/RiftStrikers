using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using System.Linq;
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
    public static GameManager Instance { get; private set; }

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

    [SerializeField] private float timeToStart = 0;
    [SerializeField] private TextMeshProUGUI timer;

    private int healthMultiplier = 1;
    private float damageMultiplier = 1;
    private float spawnIntervalMultiplier = 1;
    private float maximumAmountMultiplier = 1;

    private bool started = false;
    [SyncVar] private float gameTimer;
    public bool GameStarted => started;
    private bool canSpawn = true;
    [SerializeField] private float waitingTimeBeforeStart = 5f;
    [SyncObject]
    public readonly SyncList<Player> players = new SyncList<Player>();
    private List<Enemy> enemies = new List<Enemy>();

    private void Awake()
    {
        Instance = this;
        enemyKilledText = GameObject.Find("HUD/Game/EnemyKilled/Amount").GetComponent<TextMeshProUGUI>();
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
        RpcUpdateGameTimer(gameTimer);
    }

    [Server]
    private void StartWave()
    {
        if (currentWaveNumber == waves.Length - 1)
        {
            UpgradeWave();
        }
        else
            currentWaveNumber++;
        currentWave = waves[currentWaveNumber];
        timeToNextWave = waveDuration;
    }

    [Server]
    private void SpawnEnemy()
    {
        timeToSpawn = currentWave.spawnInterval * spawnIntervalMultiplier;
        foreach (GameObject enemyPrefab in currentWave.enemies)
        {
            Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
            GameObject enemyGO = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

            Enemy enemy = enemyGO.GetComponent<Enemy>();
            enemy.maxHealth *= healthMultiplier;
            enemy.damage = (int)(enemy.damage * damageMultiplier);
            Spawn(enemyGO);
            enemies.Add(enemy);
        }
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

    private void UpgradeWave()
    {
        currentWaveNumber = 0;

        healthMultiplier += 1;
        damageMultiplier += 0.25f;
        spawnIntervalMultiplier += 0.5f;
        maximumAmountMultiplier += 0.5f;
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
