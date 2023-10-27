using FishNet.Object;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameLauncher : MonoBehaviour
{
    private MenuManager menuManager;

    [Header("Connect")]
    [SerializeField] private TMP_InputField usernameInputField;
    [SerializeField] private TextMeshProUGUI progressStatus;
    [SerializeField] private Button authenticateButton;

    [Header("Profile")]
    private string username;
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI expText;

    [Header("Lobby List")]
    [SerializeField] private GameObject lobbyListItemPrefab;
    [SerializeField] private Transform lobbyList;
    [SerializeField] private TextMeshProUGUI status;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button createLobbyButton;

    [Header("Lobby Settings")]
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private TMP_Dropdown maxPlayersDropdown;
    [SerializeField] private TextMeshProUGUI warningTextLobby;
    [SerializeField] private Button createButton;

    private string lobbyName;
    private int maxPlayers;

    [Header("In Lobby")]
    [SerializeField] private GameObject lobbyPlayerPrefab;
    [SerializeField] private Transform playerList;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private GameObject startButton;
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI maxPlayersText;

    public string GetPlayerName()
    {
        return username;
    }

    private void Awake()
    {
        menuManager = GetComponent<MenuManager>();
        menuManager.OpenTab(menuManager.tabConnect);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        startButton.SetActive(false);

        authenticateButton.onClick.AddListener(Authenticate);

        createLobbyButton.onClick.AddListener(() => {
            menuManager.OpenTab(menuManager.tabCreateLobby);
            DefaultSettings();
        });
        refreshButton.onClick.AddListener(RefreshButtonClick);

        maxPlayersDropdown.onValueChanged.AddListener(delegate {
            ChangeMaxPlayers(maxPlayersDropdown);
        });
        createButton.onClick.AddListener(CreateLobby);

        leaveButton.onClick.AddListener(() => {
            LobbyManager.Instance.LeaveLobby();
        });
    }

    private void Authenticate()
    {
        if (string.IsNullOrEmpty(usernameInputField.text))
        {
            progressStatus.text = "Set username!";
            return;
        }
        username = usernameInputField.text;
        usernameText.text = username;
        LobbyManager.Instance.Authenticate(GetPlayerName());
    }

    private void Start()
    {
        LobbyManager.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
        LobbyManager.Instance.OnJoinedLobby += LobbyManager_OnJoinedLobby;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnKickedFromLobby;
        LobbyManager.Instance.OnJoinedLobby += UpdateLobby_Event;
        LobbyManager.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnPlayerLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnPlayerLeftLobby;
    }

    private void LobbyManager_OnKickedFromLobby(object sender, LobbyManager.LobbyEventArgs e)
    {
        menuManager.OpenTab(menuManager.tabLobbies);
    }

    private void LobbyManager_OnLeftLobby(object sender, EventArgs e)
    {
        menuManager.OpenTab(menuManager.tabLobbies);
    }

    private void LobbyManager_OnPlayerLeftLobby(object sender, EventArgs e)
    {
        ClearLobby();
    }

    private void LobbyManager_OnJoinedLobby(object sender, LobbyManager.LobbyEventArgs e)
    {
        menuManager.OpenTab(menuManager.tabLobby);
    }

    private void LobbyManager_OnLobbyListChanged(object sender, LobbyManager.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach (Transform child in this.lobbyList)
        {
            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList)
        {
            GameObject lobbyListGO = Instantiate(lobbyListItemPrefab, this.lobbyList);
            LobbyListItem lobbyListItem = lobbyListGO.GetComponent<LobbyListItem>();
            lobbyListItem.UpdateLobby(lobby);
        }
    }

    private void UpdateLobby_Event(object sender, LobbyManager.LobbyEventArgs e)
    {
        UpdateLobby();
    }

    private void UpdateLobby()
    {
        UpdateLobby(LobbyManager.Instance.GetJoinedLobby());
    }

    private void UpdateLobby(Lobby lobby)
    {
        ClearLobby();

        foreach (Unity.Services.Lobbies.Models.Player player in lobby.Players)
        {
            GameObject playerListItem = Instantiate(lobbyPlayerPrefab, playerList);
            LobbyPlayer lobbyPlayer = playerListItem.GetComponent<LobbyPlayer>();

            lobbyPlayer.SetKickPlayerButtonVisible(
                LobbyManager.Instance.IsLobbyHost() &&
                player.Id != AuthenticationService.Instance.PlayerId // Don't allow kick self
            );

            lobbyPlayer.UpdatePlayer(player);
        }
    }

    private void ClearLobby()
    {
        foreach (Transform child in playerList)
        {
            Destroy(child.gameObject);
        }
    }

    private void RefreshButtonClick()
    {
        LobbyManager.Instance.RefreshLobbyList();
    }

    private void StartTimer()
    {
        startButton.GetComponent<Button>().interactable = false;
        readyButton.interactable = false;
        //chatManager.StartCounting();
    }

    #region LobbySettings

    private void DefaultSettings()
    {
        lobbyNameInputField.text = "";
        warningTextLobby.text = "";
        maxPlayers = 1;
    }

    private void ChangeMaxPlayers(TMP_Dropdown change)
    {
        maxPlayers = byte.Parse(change.options[change.value].text);
    }

    #endregion

    #region Buttons

    private void LogOut()
    {      
        menuManager.OpenTab(menuManager.tabConnect);
        usernameText.text = "Username";
        levelText.text = "Level 1";
        expText.text = "0/0";
    }

    private void CreateLobby()
    {
        if (string.IsNullOrEmpty(lobbyNameInputField.text))
        {
            warningTextLobby.text = "Set Lobby name!";
            return;
        }

        Debug.Log("Creating Lobby");
        warningTextLobby.text = "Creating lobby";

        lobbyName = lobbyNameInputField.text;

        lobbyNameText.text = lobbyName;
        maxPlayersText.text = maxPlayers.ToString();      
        LobbyManager.Instance.CreateLobby(lobbyName, maxPlayers);
    }

    private void ReadyOnClick()
    {
        if (readyButton.GetComponent<Image>().color == Color.red)
        {
            readyButton.GetComponent<Image>().color = Color.green;
            
            leaveButton.interactable = false;
        }
        else if (readyButton.GetComponent<Image>().color == Color.green)
        {
            readyButton.GetComponent<Image>().color = Color.red;
            
            leaveButton.interactable = true;
        }
    }

    private void Btn_StartGame()
    {
        
    }

    #endregion
}