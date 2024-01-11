using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class GameLauncher : MonoBehaviour
{
    private MenuManager menuManager;

    [Header("Connect")]
    [SerializeField] private Button registerButton;
    [SerializeField] private Button loginButton;

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
    private int maxPlayers = 2;

    [Header("In Lobby")]
    [SerializeField] private GameObject lobbyPlayerPrefab;
    [SerializeField] private Transform playerList;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button startButton;
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI maxPlayersText;
    private int playersReady = 0;

    private void Awake()
    {
        menuManager = GetComponent<MenuManager>();

        loginButton.onClick.AddListener(() => {
            AccountManager.Instance.SignIn();
        });
        registerButton.onClick.AddListener(() => {
            AccountManager.Instance.SignUp();
        });

        createLobbyButton.onClick.AddListener(() => {
            menuManager.OpenTab(menuManager.tabCreateLobby);
            DefaultSettings();
        });
        refreshButton.onClick.AddListener(RefreshButtonClick);

        maxPlayersDropdown.onValueChanged.AddListener(delegate {
            ChangeMaxPlayers(maxPlayersDropdown);
        });
        createButton.onClick.AddListener(CreateLobby);
        readyButton.onClick.AddListener(ReadyOnClick);
        leaveButton.onClick.AddListener(() => {
            LobbyManager.Instance.LeaveLobby();
        });
        startButton.onClick.AddListener(() => {
            LobbyManager.Instance.StartGame();
        });
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

    private void LobbyManager_OnKickedFromLobby(object sender, EventArgs e)
    {
        menuManager.OpenTab(menuManager.tabLobbies);
    }

    private void LobbyManager_OnLeftLobby(object sender, EventArgs e)
    {
        menuManager.OpenTab(menuManager.tabLobbies);
        readyButton.GetComponent<Image>().color = Color.red;
    }

    private void LobbyManager_OnPlayerLeftLobby(object sender, EventArgs e)
    {
        ClearLobby();
        ShowStartButton();
    }

    private void LobbyManager_OnJoinedLobby(object sender, LobbyManager.LobbyEventArgs e)
    {
        menuManager.OpenTab(menuManager.tabLobby);
        ShowStartButton();
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
        playersReady = 0;
        lobbyNameText.text = lobby.Name; 
        maxPlayersText.text = lobby.MaxPlayers.ToString();
        foreach (Unity.Services.Lobbies.Models.Player player in lobby.Players)
        {
            GameObject playerListItem = Instantiate(lobbyPlayerPrefab, playerList);
            LobbyPlayer lobbyPlayer = playerListItem.GetComponent<LobbyPlayer>();

            lobbyPlayer.SetKickPlayerButtonVisible(
                LobbyManager.Instance.IsLobbyHost() &&
                player.Id != AuthenticationService.Instance.PlayerId // Don't allow kick self
            );
            if(player.Data["Ready"].Value == "true")
                playersReady++;
            lobbyPlayer.UpdatePlayer(player);
        }
        if (playersReady == lobby.Players.Count)
            startButton.interactable = true;
        else
            startButton.interactable = false;
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

    private void DefaultSettings()
    {
        lobbyNameInputField.text = "";
        warningTextLobby.text = "";
        maxPlayers = 2;
    }

    private void ChangeMaxPlayers(TMP_Dropdown change)
    {
        maxPlayers = byte.Parse(change.options[change.value].text);
    }

    private void CreateLobby()
    {
        if (string.IsNullOrEmpty(lobbyNameInputField.text))
        {
            warningTextLobby.text = "Set Lobby name!";
            return;
        }

        Debug.Log("Creating Lobby...");

        lobbyName = lobbyNameInputField.text;    
        lobbyNameText.text = lobbyName;
        maxPlayersText.text = maxPlayers.ToString();      
        LobbyManager.Instance.CreateLobby(lobbyName, maxPlayers);
    }

    private void ReadyOnClick()
    {
        if (readyButton.GetComponent<Image>().color == Color.red)
        {
            LobbyManager.Instance.UpdatePlayerReady("true");
            readyButton.GetComponent<Image>().color = Color.green;
        }
        else if (readyButton.GetComponent<Image>().color == Color.green)
        {
            LobbyManager.Instance.UpdatePlayerReady("false");
            readyButton.GetComponent<Image>().color = Color.red;
        }
    }

    private void ShowStartButton()
    {
        if (LobbyManager.Instance.IsLobbyHost())
            startButton.gameObject.SetActive(true);
        else
            startButton.gameObject.SetActive(false);
    }
}