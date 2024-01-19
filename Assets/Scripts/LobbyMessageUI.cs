using System;
using TMPro;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMessageUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(Hide);
        Show();
    }

    private void Start()
    {
        AccountManager.Instance.OnSignUpStarted += AccountManager_OnSignUpStarted;
        AccountManager.Instance.OnSignUpFailed += AccountManager_OnSignUpFailed;

        AccountManager.Instance.OnAuthenticateStarted += AccountManager_OnAuthenticateStarted;
        AccountManager.Instance.OnAuthenticated += AccountManager_OnAuthenticated;
        AccountManager.Instance.OnAuthenticateFailed += AccountManager_OnAuthenticateFailed;

        LobbyManager.Instance.OnCreateLobbyStarted += LobbyManager_OnCreateLobbyStarted;
        VivoxService.Instance.ChannelJoined += VivoxService_ChannelJoined;
        LobbyManager.Instance.OnCreateLobbyFailed += LobbyManager_OnCreateLobbyFailed;

        LobbyManager.Instance.OnJoinLobbyStarted += LobbyManager_OnJoinLobbyStarted;
        LobbyManager.Instance.OnJoinLobbyFailed += LobbyManager_OnJoinLobbyFailed;

        LobbyManager.Instance.OnGameStarted += LobbyManager_OnGameStarted;

        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnKickedFromLobby;

        AccountManager.Instance.OnVivoxLoginFailed += AccountManager_OnVivoxLoginFailed;
        AccountManager.Instance.OnDataLoadFailed += AccountManager_OnDataLoadFailed;

        Hide();
    }

    private void VivoxService_ChannelJoined(string obj)
    {
        Hide();
    }

    private void LobbyManager_OnGameStarted(object sender, EventArgs e)
    {
        VivoxService.Instance.LeaveChannelAsync(VivoxVoiceManager.LobbyChannelName);
        ShowMessage("Starting game...");
    }

    private void AccountManager_OnSignUpStarted(object sender, EventArgs e)
    {
        ShowMessage("Registration...");
    }

    private void AccountManager_OnSignedUp(object sender, EventArgs e)
    {
        ShowResponseMessage("Registration successful!");
    }

    private void AccountManager_OnSignUpFailed(object sender, string e)
    {
        ShowResponseMessage(e);
    }

    private void AccountManager_OnAuthenticateStarted(object sender, EventArgs e)
    {
        ShowMessage("Connecting...");
    }

    private void AccountManager_OnAuthenticated(object sender, EventArgs e)
    {
        Hide();
    }

    private void AccountManager_OnAuthenticateFailed(object sender, string e)
    {
        ShowResponseMessage(e);
    }

    private void LobbyManager_OnJoinLobbyStarted(object sender, EventArgs e)
    {
        ShowMessage("Joining Lobby...");
    }

    private void LobbyManager_OnJoinLobbyFailed(object sender, EventArgs e)
    {
        ShowResponseMessage("Failed to join Lobby!");
    }

    private void LobbyManager_OnCreateLobbyStarted(object sender, EventArgs e)
    {
        ShowMessage("Creating Lobby...");
    }

    private void LobbyManager_OnCreateLobbyFailed(object sender, EventArgs e)
    {
        ShowResponseMessage("Failed to create Lobby!");
    }

    private void LobbyManager_OnKickedFromLobby(object sender, EventArgs e)
    {
        ShowResponseMessage("You have been kicked from the Lobby!");
    }

    private void AccountManager_OnVivoxLoginFailed(object sender, EventArgs e)
    {
        ShowResponseMessage("Login to Vivox failed!");
    }

    private void AccountManager_OnDataLoadFailed(object sender, EventArgs e)
    {
        ShowResponseMessage("Load Data failed!");
    }

    private void ShowMessage(string message)
    {
        messageText.text = message;
        closeButton.gameObject.SetActive(false);
        Show();       
    }

    private void ShowResponseMessage(string message)
    {
        messageText.text = message;
        closeButton.gameObject.SetActive(true);
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        AccountManager.Instance.OnSignUpStarted -= AccountManager_OnSignUpStarted;
        AccountManager.Instance.OnSignUpFailed -= AccountManager_OnSignUpFailed;

        AccountManager.Instance.OnAuthenticateStarted -= AccountManager_OnAuthenticateStarted;
        AccountManager.Instance.OnAuthenticated -= AccountManager_OnAuthenticated;
        AccountManager.Instance.OnAuthenticateFailed -= AccountManager_OnAuthenticateFailed;

        LobbyManager.Instance.OnCreateLobbyStarted -= LobbyManager_OnCreateLobbyStarted;
        VivoxService.Instance.ChannelJoined -= VivoxService_ChannelJoined;
        LobbyManager.Instance.OnCreateLobbyFailed -= LobbyManager_OnCreateLobbyFailed;

        LobbyManager.Instance.OnJoinLobbyStarted -= LobbyManager_OnJoinLobbyStarted;
        LobbyManager.Instance.OnJoinLobbyFailed -= LobbyManager_OnJoinLobbyFailed;

        LobbyManager.Instance.OnGameStarted -= LobbyManager_OnGameStarted;

        LobbyManager.Instance.OnKickedFromLobby -= LobbyManager_OnKickedFromLobby;
    }
}
