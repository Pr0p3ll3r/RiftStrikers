using TMPro;
using Unity.Services.Vivox;
using UnityEngine;

public class MessageObjectUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;

    public void SetTextMessage(VivoxMessage message)
    {
        messageText.alignment = TextAlignmentOptions.MidlineLeft;
        messageText.text = string.Format($"<color=green>{message.SenderDisplayName} </color>: {message.MessageText}");
    }
}
