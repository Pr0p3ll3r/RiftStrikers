using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("Tabs")]
    public GameObject tabLogin;
    public GameObject tabRegister;
    public GameObject tabMain;
    public GameObject tabLobbies;
    public GameObject tabCreateLobby;
    public GameObject tabLobby;
    public GameObject tabSettings;
    public GameObject tabAbout;
    public GameObject tabUpgrades;

    [Header("Buttons")]
    [SerializeField] private Button registerButton;
    [SerializeField] private Button findLobbyButton;
    [SerializeField] private Button upgradesButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button aboutButton;
    [SerializeField] private Button quitGameMainMenuButton;
    [SerializeField] private Button quitGameLoginMenuButton;

    private void Awake()
    {
        Instance = this;

        registerButton.onClick.AddListener(() =>
        {
            OpenTab(tabRegister);
        });
        findLobbyButton.onClick.AddListener(() => {
            OpenTab(tabLobbies);
        });
        upgradesButton.onClick.AddListener(() =>
        {
            tabUpgrades.SetActive(true);
        });
        settingsButton.onClick.AddListener(() => {
            OpenTab(tabSettings);
        });
        aboutButton.onClick.AddListener(() => {
            OpenTab(tabAbout);
        });
        quitGameLoginMenuButton.onClick.AddListener(() => {
            QuitGame();
        });
        quitGameMainMenuButton.onClick.AddListener(() => {
            QuitGame();
        });
    }

    private void TabCloseAll()
    {
        tabLogin.SetActive(false);
        tabRegister.SetActive(false);
        tabMain.SetActive(false);
        tabLobbies.SetActive(false);
        tabCreateLobby.SetActive(false);
        tabLobby.SetActive(false);
        tabSettings.SetActive(false);
        tabAbout.SetActive(false);
        tabUpgrades.SetActive(false);
    }

    public void OpenTab(GameObject tab)
    {
        TabCloseAll();
        tab.SetActive(true);
        tab.GetComponent<Animator>().enabled = true;
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}
