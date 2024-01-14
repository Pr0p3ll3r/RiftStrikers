using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TextChatUI : MonoBehaviour
{
    private IList<KeyValuePair<string, MessageObjectUI>> m_MessageObjPool = new List<KeyValuePair<string, MessageObjectUI>>();
    ScrollRect m_TextChatScrollRect;

    [SerializeField] private GameObject chatContentObj;
    [SerializeField] private GameObject messageObject;
    [SerializeField] private Button sendButton;
    [SerializeField] private TMP_InputField messageInputField;

    private Task FetchMessages = null;
    private DateTime? oldestMessage = null;

    void Start()
    {
        VivoxService.Instance.ChannelJoined += OnChannelJoined;
        VivoxService.Instance.ChannelMessageReceived += OnChannelMessageReceived;

        m_TextChatScrollRect = GetComponent<ScrollRect>();
        if (m_MessageObjPool.Count > 0)
        {
            ClearMessageObjectPool();
        }

        ClearTextField();

        sendButton.onClick.AddListener(SendMessage);
        messageInputField.onEndEdit.AddListener((string text) => { EnterKeyOnTextField(); });
        m_TextChatScrollRect.onValueChanged.AddListener(ScrollRectChange);
    }

    private void ScrollRectChange(Vector2 vector)
    {
        // Scrolled near end and check if we are fetching history already
        if (m_TextChatScrollRect.verticalNormalizedPosition >= 0.95f && FetchMessages != null && (FetchMessages.IsCompleted || FetchMessages.IsFaulted || FetchMessages.IsCanceled))
        {
            m_TextChatScrollRect.normalizedPosition = new Vector2(0, 0.8f);
            FetchMessages = FetchHistory(false);
        }
    }

    private async Task FetchHistory(bool scrollToBottom = false)
    {
        try
        {
            var chatHistoryOptions = new ChatHistoryQueryOptions()
            {
                TimeEnd = oldestMessage
            };
            var historyMessages =
                await VivoxService.Instance.GetChannelTextMessageHistoryAsync(VivoxVoiceManager.LobbyChannelName, 10,
                    chatHistoryOptions);
            var reversedMessages = historyMessages.Reverse();
            foreach (var historyMessage in reversedMessages)
            {
                AddMessageToChat(historyMessage, true, scrollToBottom);
            }

            // Update the oldest message ReceivedTime if it exists to help the next fetch get the next batch of history
            oldestMessage = historyMessages.FirstOrDefault()?.ReceivedTime;
        }
        catch (TaskCanceledException e)
        {
            Debug.Log($"Chat history request was canceled, likely because of a logout or the data is no longer needed: {e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Tried to fetch chat history and failed with error: {e.Message}");
        }
    }

    void OnDestroy()
    {
        VivoxService.Instance.ChannelJoined -= OnChannelJoined;
        VivoxService.Instance.ChannelMessageReceived -= OnChannelMessageReceived;

        sendButton.onClick.RemoveAllListeners();
        messageInputField.onEndEdit.RemoveAllListeners();
        m_TextChatScrollRect.onValueChanged.RemoveAllListeners();
    }

    void ClearMessageObjectPool()
    {
        foreach (KeyValuePair<string, MessageObjectUI> keyValuePair in m_MessageObjPool)
        {
            Destroy(keyValuePair.Value.gameObject);
        }
        m_MessageObjPool.Clear();
    }

    void ClearTextField()
    {
        messageInputField.text = string.Empty;
        messageInputField.Select();
        messageInputField.ActivateInputField();
    }

    void EnterKeyOnTextField()
    {
        if (!Keyboard.current[Key.Enter].wasPressedThisFrame)
        {
            return;
        }
        SendMessage();
    }

    void SendMessage()
    {
        if (string.IsNullOrEmpty(messageInputField.text))
        {
            return;
        }

        VivoxService.Instance.SendChannelTextMessageAsync(VivoxVoiceManager.LobbyChannelName, messageInputField.text);
        ClearTextField();
    }

    IEnumerator SendScrollRectToBottom()
    {
        yield return new WaitForEndOfFrame();

        // We need to wait for the end of the frame for this to be updated, otherwise it happens too quickly.
        m_TextChatScrollRect.normalizedPosition = new Vector2(0, 0);

        yield return null;
    }

    void OnChannelJoined(string channelName)
    {
        if (m_MessageObjPool.Count > 0)
        {
            ClearMessageObjectPool();
        }
        ClearTextField();
        FetchMessages = FetchHistory(true);
    }

    void OnChannelMessageReceived(VivoxMessage message)
    {
        AddMessageToChat(message, false, true);
    }

    void AddMessageToChat(VivoxMessage message, bool isHistory = false, bool scrollToBottom = false)
    {
        var newMessageObj = Instantiate(messageObject, chatContentObj.transform);
        var newMessageTextObject = newMessageObj.GetComponent<MessageObjectUI>();
        if (isHistory)
        {
            m_MessageObjPool.Insert(0, new KeyValuePair<string, MessageObjectUI>(message.MessageId, newMessageTextObject));
            newMessageObj.transform.SetSiblingIndex(0);
        }
        else
        {
            m_MessageObjPool.Add(new KeyValuePair<string, MessageObjectUI>(message.MessageId, newMessageTextObject));
        }

        newMessageTextObject.SetTextMessage(message);
        if (scrollToBottom)
        {
            StartCoroutine(SendScrollRectToBottom());
        }
    }
}