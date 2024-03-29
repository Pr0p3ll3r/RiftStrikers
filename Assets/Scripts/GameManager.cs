using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    [SerializeField] private int islandDuration = 300;
    private Transform[] spawnPoints = new Transform[4];
    [SerializeField] private Island[] islands = new Island[5];
    [SerializeField] private float spawnInterval;
    [SerializeField] private int maximumAmount;
    private int clearedIslands = 0;

    private Island currentIsland;
    private int enemyAmount;
    private int enemyKilled;
    [SerializeField] private TextMeshProUGUI enemyKilledText;
    private List<Island> availableIslands;
    private float timeToSpawnEnemy;
    private float timeToNextIsland;

    private float gameTimer;
    [SerializeField] private TextMeshProUGUI gameTimerText;
    [HideInInspector] public int money;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private GameObject gameoverScreen;
    [SerializeField] private TextMeshProUGUI timeSurvivedText;
    [SerializeField] private TextMeshProUGUI moneyEarnedText;
    [SerializeField] private TextMeshProUGUI levelReachedText;
    [SerializeField] private TextMeshProUGUI enemiesDefeatedText;
    [SerializeField] private TextMeshProUGUI islandsClearedText;
    [SerializeField] private Button returnToMenuButton;
    private int healthMultiplier = 1;
    private float damageMultiplier = 1;
    private float spawnIntervalMultiplier = 1;
    private float maximumAmountMultiplier = 1;
    private bool isBossSpawned;

    [SyncObject] public readonly SyncList<PlayerInstance> players = new SyncList<PlayerInstance>();
    [SyncObject] public readonly SyncList<Enemy> enemies = new SyncList<Enemy>();

    [SerializeField] private TextMeshProUGUI nextIslandTimerText;
    [SerializeField] private GameObject getPortalText;
    [SerializeField] private GameObject enemyPortalPrefab;
    [SerializeField] private GameObject playerPortalPrefab;
    private GameObject spawnedPlayerPortal;
    private GameObject[] spawnedEnemiesPortal = new GameObject[4];
    [SerializeField] private float timeForGetToPortal = 30.0f;
    [SyncVar] [HideInInspector] private float timerGetToPortal;
    [SerializeField] private float timeToFadeScreen = 1f;
    [SerializeField] private Animator screenTransition;
    private int playersInPortal;
    private bool islandBeingDestroyed = false;
    private MapGenerator MapGenerator => MapGenerator.Instance;
    private GameState previousState;

    private void Awake()
    {
        Instance = this;
        availableIslands = islands.ToList();
        returnToMenuButton.onClick.AddListener(ReturnToMenu);
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        if (IsServer)
        {
            GenerateMap();
        }      
    }

    public override void OnStartServer()
    {
        base.OnStartClient();
        gameTimer = 0;
        waitingTimeBeforeStart = 5f;
        currentState = GameState.WaitingForPlayers;
    }

    void Update()
    {
        if (!IsServer) return;

        switch (currentState)
        {
            case GameState.WaitingForPlayers:
                waitingTimeBeforeStart -= Time.deltaTime;
                UpdateGameTimerRpc(waitingTimeBeforeStart + 1);
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

                if (playersInPortal == GetLivingPlayers())
                {
                    currentState = GameState.Paused;
                    StartDarkeningScreenRpc();
                }
                else if (islandBeingDestroyed)
                {
                    timerGetToPortal -= Time.deltaTime;
                    if (timerGetToPortal <= 0)
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
        foreach (PlayerInstance player in players)
        {
            if (player.controlledPlayer.gameObject.activeSelf)
                Despawn(player.controlledPlayer.gameObject);
        }
        if (playersInPortal == 0)
        {
            MapGenerator.DestroyMapRpc();
            ClearMap();
            SetGameOverScreen();
        }
        else
        {
            currentState = GameState.Paused;
            StartDarkeningScreenRpc();
        }
    }

    [ObserversRpc]
    private void UpdateGameTimerRpc(float newTime)
    {
        gameTimerText.text = FormatTimer(newTime);
        nextIslandTimerText.text = FormatTimer(timerGetToPortal + 1);
    }

    private string FormatTimer(float newTime)
    {
        if (newTime < 0)
            newTime = 0;
        int hours = Mathf.FloorToInt(newTime / 3600);
        int minutes = Mathf.FloorToInt((newTime % 3600) / 60);
        int seconds = Mathf.FloorToInt(newTime % 60);
        if (hours > 0)
            return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        else
            return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void UpdateGameTimer()
    {
        gameTimer += Time.deltaTime;
        UpdateGameTimerRpc(gameTimer);
    }

    private void StartGame()
    {
        Debug.Log("Game Start");
        foreach (PlayerInstance player in players)
        {
            player.SpawnPlayer(spawnedPlayerPortal.transform);
        }
        timerGetToPortal = timeForGetToPortal;
        timeToNextIsland = islandDuration;
        UpdateGameTimer();
        getPortalText.SetActive(false);
        currentState = GameState.Fighting;
    }

    private void GenerateMap()
    {
        int seed = Random.Range(int.MinValue, int.MaxValue);
        int randomIsland = Random.Range(0, availableIslands.Count);
        currentIsland = availableIslands[randomIsland];
        MapGenerator.GenerateMapRpc(seed, currentIsland.biomeType);
        Debug.Log("Generated map!");
    }

    private void SpawnEnemy()
    {
        if (currentIsland == null) return;

        timeToSpawnEnemy = spawnInterval * spawnIntervalMultiplier;
        foreach (GameObject enemyPrefab in currentIsland.enemies)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemyGO = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            Enemy enemy = enemyGO.GetComponent<Enemy>();
            enemy.CurrentMaxHealth *= healthMultiplier;
            enemy.CurrentDamage *= damageMultiplier;
            Spawn(enemyGO);
            enemies.Add(enemy);
        }
    }

    private void SpawnBoss()
    {
        if (isBossSpawned) return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject bossGO = Instantiate(currentIsland.bossPrefab, spawnPoint.position, Quaternion.identity);
        Enemy boss = bossGO.GetComponent<Enemy>();
        boss.CurrentMaxHealth *= healthMultiplier;
        boss.CurrentDamage *= damageMultiplier;
        Spawn(bossGO);
        enemies.Add(boss);
        isBossSpawned = true;
        Debug.Log("Boss spawned!");
    }

    [ObserversRpc]
    public void ClearedIsland()
    {
        Debug.Log("Cleared Island!");
        currentState = GameState.ChangingMap;
        timeToNextIsland = islandDuration;
        clearedIslands++;
        isBossSpawned = false;
        availableIslands.Remove(currentIsland);
        StartIslandRemoving();
    }

    private void NewCycle()
    {
        availableIslands = islands.ToList();
        healthMultiplier += 1;
        damageMultiplier += 0.25f;
        spawnIntervalMultiplier -= 0.5f;
        spawnIntervalMultiplier = Mathf.Max(spawnIntervalMultiplier, 0.5f);
        maximumAmountMultiplier += 0.5f;
        Debug.Log("New Cycle!");
    }

    private void StartIslandRemoving()
    {
        MapGenerator.StartShaking();
        getPortalText.SetActive(true);
        if (IsServer)
            EnablePortal();
    }

    public void PlayersInPortal()
    {
        playersInPortal++;
    }

    public void SpawnPortals()
    {
        GameObject randomLand = MapGenerator.GetRandomEmptyLand();
        spawnedPlayerPortal = Instantiate(playerPortalPrefab, randomLand.transform.position + Vector3.up, Quaternion.identity);
        Spawn(spawnedPlayerPortal);
        for (int i = 0; i < 4; i++)
        {
            randomLand = MapGenerator.GetRandomEmptyLand();
            GameObject spawnedEnemyPortal = Instantiate(enemyPortalPrefab, randomLand.transform.position + Vector3.up, Quaternion.identity);
            Spawn(spawnedEnemyPortal);
            spawnedEnemiesPortal[i] = spawnedEnemyPortal;
            spawnPoints[i] = spawnedEnemyPortal.transform;
        }
    }

    private void EnablePortal()
    {      
        spawnedPlayerPortal.GetComponent<Portal>().canBeUsed = true;
        islandBeingDestroyed = true;
    }

    [ObserversRpc]
    private void StartDarkeningScreenRpc()
    {
        if (GetLivingPlayers() == 0) return;

        currentState = GameState.Paused;
        getPortalText.SetActive(false);
        timerGetToPortal = timeForGetToPortal;
        Player.Instance.DisableVignette();
        StartCoroutine(DarkenScreen());
    }

    private IEnumerator DarkenScreen()
    {
        screenTransition.SetBool("Fade", true);
        yield return new WaitForSeconds(timeToFadeScreen);
        if (IsServer)
        {
            if (!availableIslands.Any())
                NewCycle();
            ClearMap();
            GenerateMap();
        }
        yield return new WaitForSeconds(timeToFadeScreen);
        if (IsServer)
        {
            foreach (PlayerInstance player in players)
            {
                player.SpawnPlayer(spawnedPlayerPortal.transform, false);
            }
        }
        yield return new WaitForSeconds(timeToFadeScreen);
        playersInPortal = 0;
        LevelSystem.Instance.SpawnAllItems();
        screenTransition.SetBool("Fade", false);
        PauseGame(false);
    }

    private void ClearMap()
    {
        islandBeingDestroyed = false;
        previousState = GameState.Fighting;
        Despawn(spawnedPlayerPortal);
        foreach (GameObject portal in spawnedEnemiesPortal)
        {
            Despawn(portal);
        }
        foreach (PlayerInstance player in players)
        {
            if (player.controlledPlayer)
                Despawn(player.controlledPlayer.gameObject);
        }
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in enemies)
        {
            Despawn(enemy.gameObject);
        }
        PickupItem[] pickups = FindObjectsByType<PickupItem>(FindObjectsSortMode.None);
        foreach (PickupItem pickup in pickups)
        {
            Despawn(pickup.gameObject);
        }
    }

    public void GameOver()
    {
        if (GetLivingPlayers() > 0) return;
        SetGameOverScreen();
    }

    [ObserversRpc]
    public void SetGameOverScreen()
    {
        PauseGame(true);
        timeSurvivedText.text = gameTimerText.text;
        enemiesDefeatedText.text = enemyKilledText.text;
        moneyEarnedText.text = moneyText.text;
        levelReachedText.text = LevelSystem.Instance.GetCurrentLevel().ToString();
        islandsClearedText.text = clearedIslands.ToString();
        gameoverScreen.SetActive(true);
        CloudData.PlayerData.Money += money;
        CloudData.Save();
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadGlobalScenes(new SceneLoadData("Menu"));
        SceneManager.UnloadGlobalScenes(new SceneUnloadData("Game"));
        NetworkManager.ClientManager.StopConnection();
        if (IsServer)
            NetworkManager.ServerManager.StopConnection(false);
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
    public void AddMoneyRpc(int amount)
    {
        money += amount;
        moneyText.text = $"Money: ${money}";
    }

    public GameObject GetClosestEnemy(Vector3 pos, float range, List<Enemy> enemyList = null)
    {
        List<Enemy> tempList = enemyList ?? enemies.ToList();      

        GameObject closestEnemy = null;
        float minimumDistance = Mathf.Infinity;

        foreach (Enemy enemy in tempList)
        {
            if (enemy.IsClientInitialized && !enemy.IsDead)
            {
                float distanceToEnemy = Vector3.Distance(pos, enemy.transform.position);

                if (distanceToEnemy <= range && distanceToEnemy < minimumDistance)
                {
                    closestEnemy = enemy.gameObject;
                    minimumDistance = distanceToEnemy;
                }
            }
        }

        return closestEnemy;
    }

    public Enemy[] GetEnemiesInRange(Vector3 pos, float range)
    {
        List<Enemy> tempList = new List<Enemy>();

        foreach (Enemy enemy in enemies)
        {
            if (!enemy.IsDead)
            {
                float distanceToEnemy = Vector3.Distance(pos, enemy.transform.position);

                if (distanceToEnemy <= range)
                {
                    tempList.Add(enemy);
                }
            }
        }

        return tempList.ToArray();
    }
}
