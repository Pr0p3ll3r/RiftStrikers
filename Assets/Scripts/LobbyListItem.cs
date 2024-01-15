using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;

public class LobbyListItem : MonoBehaviour 
{ 
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI playersText;
    [SerializeField] private Button joinLobbyButton;

    private Lobby lobby;

    private void Awake() {
        joinLobbyButton.onClick.AddListener(() => {
            LobbyManager.Instance.JoinLobby(lobby);
        });
    }

    public void UpdateLobby(Lobby lobby) {
        this.lobby = lobby;

        lobbyNameText.text = lobby.Name;
        playersText.text = lobby.Players.Count + "/" + lobby.MaxPlayers;

        if (lobby.Players.Count == lobby.MaxPlayers)
            joinLobbyButton.interactable = false;      
        else
            joinLobbyButton.interactable = true;
    }
}