using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMessageUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(Hide);
    }

    private void Start()
    {
        LobbyManager.Instance.OnAuthenticateStarted += LobbyManager_OnAuthenticateStarted;
        LobbyManager.Instance.OnCreateLobbyStarted += LobbyManager_OnCreateLobbyStarted;
        LobbyManager.Instance.OnCreateLobbyFailed += LobbyManager_OnCreateLobbyFailed;
        LobbyManager.Instance.OnJoinLobbyStarted += LobbyManager_OnJoinLobbyStarted;
        LobbyManager.Instance.OnJoinLobbyFailed += LobbyManager_OnJoinLobbyFailed;

        Hide();
    }

    private void LobbyManager_OnAuthenticateStarted(object sender, EventArgs e)
    {
        ShowMessage("Connecting...");
    }

    private void LobbyManager_OnJoinLobbyFailed(object sender, EventArgs e)
    {
        ShowMessage("Failed to join Lobby!");
    }

    private void LobbyManager_OnJoinLobbyStarted(object sender, EventArgs e)
    {
        ShowMessage("Joining Lobby...");
    }

    private void LobbyManager_OnCreateLobbyFailed(object sender, EventArgs e)
    {
        ShowMessage("Failed to create Lobby!");
    }

    private void LobbyManager_OnCreateLobbyStarted(object sender, EventArgs e)
    {
        ShowMessage("Creating Lobby...");
    }

    private void ShowMessage(string message)
    {
        Show();
        messageText.text = message;
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
