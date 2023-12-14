using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

[System.Serializable]
public class Island
{
    public BiomeType biomeType;
    public GameObject[] enemies;
    public GameObject bossPrefab;
}

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState currentState = GameState.WaitingForPlayers;

    [SerializeField] private float waitingTimeBeforeStart = 5f;
    [SerializeField] private int islandDuration = 600;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Island[] islands = new Island[5];
    [SerializeField] private float spawnInterval;
    [SerializeField] private int maximumAmount;
    private int clearedIslands = 0;
    private int cycleNumber = 0;

    private Island currentIsland;
    private int enemyAmount;
    private int enemyKilled;
    [SerializeField] private TextMeshProUGUI enemyKilledText;
    private List<Island> availableIslands;
    private float timeToSpawnEnemy;
    private float timeToNextIsland;

    [SyncVar] private float gameTimer;
    [SerializeField] private TextMeshProUGUI timer;

    private int healthMultiplier = 1;
    private float damageMultiplier = 1;
    private float spawnIntervalMultiplier = 1;
    private float maximumAmountMultiplier = 1;
    private bool isBossSpawned;

    [SyncObject]
    public readonly SyncList<PlayerInstance> players = new SyncList<PlayerInstance>();
    private List<Enemy> enemies = new List<Enemy>();

    [SerializeField] private GameObject teleportPrefab;
    [SerializeField] private float timeForGetToTeleport = 30.0f;
    [SerializeField] private float timeToFadeScreen = 2f;
    [SerializeField] private Animator screenTransition;
    private GameObject teleport;
    private int playersInTeleport;
    private bool teleportSpawned = false;
    private MapGenerator MapGenerator => MapGenerator.Instance;
    private GameState previousState;

    private void Awake()
    {
        Instance = this;
        availableIslands = islands.ToList();
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        if (IsServer)
        {
            GenerateMap();
        }
    }

    void Update()
    {
        if (!IsServer) return;

        switch (currentState)
        {
            case GameState.WaitingForPlayers:
                waitingTimeBeforeStart -= Time.deltaTime;
                RpcUpdateGameTimer(waitingTimeBeforeStart);
                if (waitingTimeBeforeStart <= 0)
                {
                    StartGame();
                }
                break;
            case GameState.Fighting:
                UpdateGameTimer();

                if (timeToNextIsland > 0)
                    timeToNextIsland -= Time.deltaTime;
                else
                    SpawnBoss();

                if (timeToSpawnEnemy > 0)
                    timeToSpawnEnemy -= Time.deltaTime;
                else if (enemyAmount < maximumAmount * maximumAmountMultiplier)
                {
                    SpawnEnemy();
                }
                break;
            case GameState.ChangingMap:
                UpdateGameTimer();

                if (playersInTeleport == GetLivingPlayers())
                {
                    playersInTeleport = 0;
                    StartDarkeningScreenRpc();
                }
                else if (teleportSpawned)
                {
                    timeForGetToTeleport -= Time.deltaTime;
                    if (timeForGetToTeleport <= 0)
                    {
                        TimeExpired();
                    }
                }

                if (timeToSpawnEnemy > 0)
                    timeToSpawnEnemy -= Time.deltaTime;
                else if (enemyAmount < maximumAmount * maximumAmountMultiplier)
                {
                    SpawnEnemy();
                }
                break;
        }     
    }

    private void TimeExpired()
    {
        MapGenerator.DestroyMapRpc();
        ClearMap();
        PauseGame(true);
        Player.Instance.TakeDamageServer(1000);
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
        foreach (PlayerInstance player in players)
        {
            player.SpawnPlayer();
        }
        timeToNextIsland = islandDuration;
        UpdateGameTimer();
        currentState = GameState.Fighting;
    }

    public void ClearedIsland()
    {
        Debug.Log("Cleared Island!");
        currentState = GameState.ChangingMap;
        timeToNextIsland = islandDuration;
        clearedIslands++;
        isBossSpawned = false;
        availableIslands.Remove(currentIsland);
        if (!availableIslands.Any())
            NewCycle();     
        StartIslandRemoving();
    }

    private void GenerateMap()
    {
        int randomWave = Random.Range(0, availableIslands.Count);
        currentIsland = availableIslands[randomWave];
        MapGenerator.GenerateMapRpc(currentIsland.biomeType);
        Debug.Log("Generated map!");
    }

    [Server]
    private void SpawnEnemy()
    {
        if (currentIsland == null) return;

        timeToSpawnEnemy = spawnInterval * spawnIntervalMultiplier;
        foreach (GameObject enemyPrefab in currentIsland.enemies)
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
        if (isBossSpawned) return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject bossGO = Instantiate(currentIsland.bossPrefab, spawnPoint.position, Quaternion.identity);
        Enemy boss = bossGO.GetComponent<Enemy>();
        boss.maxHealth *= healthMultiplier;
        boss.damage = (int)(boss.damage * damageMultiplier);
        boss.isBoss = true;
        Spawn(bossGO);
        enemies.Add(boss);
        isBossSpawned = true;
        Debug.Log("Boss spawned!");
    }

    private void NewCycle()
    {
        availableIslands = islands.ToList();
        currentIsland = null;
        cycleNumber++;
        healthMultiplier += 1;
        damageMultiplier += 0.25f;
        spawnIntervalMultiplier += 0.5f;
        maximumAmountMultiplier += 0.5f;
        Debug.Log("New Cycle!");
    }

    private void StartIslandRemoving()
    {
        MapGenerator.StartShakingRpc();
        SpawnTeleport();
    }

    public void PlayersInPortal()
    {
        playersInTeleport++;
    }

    private void SpawnTeleport()
    {
        GameObject randomLand = MapGenerator.GetRandomEmptyLand();
        teleport = Instantiate(teleportPrefab, randomLand.transform.position + Vector3.up, Quaternion.identity);
        Spawn(teleport);
        teleportSpawned = true;
    }

    [ObserversRpc]
    private void StartDarkeningScreenRpc()
    {
        currentState = GameState.Paused;
        StartCoroutine(DarkenScreen());
    }

    private IEnumerator DarkenScreen()
    {
        screenTransition.SetBool("Fade", true);
        yield return new WaitForSeconds(timeToFadeScreen);
        if (IsServer)
        {
            ClearMap();
            GenerateMap();
            foreach (PlayerInstance player in players)
            {
                player.SpawnPlayer(false);
            }
        }
        yield return new WaitForSeconds(timeToFadeScreen);
        screenTransition.SetBool("Fade", false);
        PauseGame(false);
    }

    private void ClearMap()
    {
        teleportSpawned = false;
        playersInTeleport = 0;
        previousState = GameState.Fighting;
        Despawn(teleport);
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = enemies[i];
            if (!enemy.isDead)
            {
                Despawn(enemy.gameObject);
                enemies.RemoveAt(i);
            }
        }
        PickupItem[] pickups = FindObjectsByType<PickupItem>(FindObjectsSortMode.None);
        foreach (PickupItem pickup in pickups)
        {
            Despawn(pickup.gameObject);
        }
    }

    public int GetLivingPlayers()
    {
        return players.Count(x => !x.controlledPlayer.IsDead);
    }

    public void PauseGame(bool paused)
    {
        if (paused)
        {
            previousState = currentState;
            currentState = GameState.Paused;
        }        
        else
            currentState = previousState;                 
        foreach (Enemy enemy in enemies)
        {
            enemy.ChangeAgentStatus(paused);
        }
        Player.Instance.CanControl = !paused;
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
