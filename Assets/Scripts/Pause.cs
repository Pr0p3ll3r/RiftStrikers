using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    private bool paused = false;

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject settingsMenu;

    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button returnButton;

    private void Start()
    {
        resumeButton.onClick.AddListener(() => {
            TooglePause();
        });
        settingsButton.onClick.AddListener(() => {
            Settings();
        });
        exitButton.onClick.AddListener(() => {
            ReturnToMenu();
        });
        returnButton.onClick.AddListener(() => {
            Return();
        });
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TooglePause();
        }
    }

    private void TooglePause()
    {
        paused = !paused;
   
        transform.GetChild(0).gameObject.SetActive(paused);
        pauseMenu.SetActive(paused);
        settingsMenu.SetActive(false);
    }

    private void Settings()
    {
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    private void Return()
    {
        transform.GetChild(0).gameObject.SetActive(paused);
        pauseMenu.SetActive(true);
        settingsMenu.SetActive(false);
    }

    private void ReturnToMenu()
    {
        TooglePause();
        LobbyManager.Instance.LeaveLobby();
    }
}