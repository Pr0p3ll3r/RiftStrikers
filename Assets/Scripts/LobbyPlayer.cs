using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private Image readyImage;
    [SerializeField] private Button kickPlayerButton;
    
    private Unity.Services.Lobbies.Models.Player player;

    private void Awake()
    {
        kickPlayerButton.onClick.AddListener(KickPlayer);
    }

    public void SetKickPlayerButtonVisible(bool visible)
    {
        kickPlayerButton.gameObject.SetActive(visible);
    }

    public void UpdatePlayer(Unity.Services.Lobbies.Models.Player player)
    {
        this.player = player;
        nicknameText.text = player.Data["Nickname"].Value;
        
    }

    private void KickPlayer()
    {
        if (player != null)
        {
            LobbyManager.Instance.KickPlayer(player.Id);
        }
    }
}
