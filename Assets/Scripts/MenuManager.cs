using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("Tabs")]
    public GameObject tabConnect;
    public GameObject tabMain;
    public GameObject tabLobbies;
    public GameObject tabCreateLobby;
    public GameObject tabLobby;
    public GameObject tabOptions;
    public GameObject tabAbout;

    [Header("Loading")]
    public GameObject tabLoading;
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("MainTab")]
    [SerializeField] private Button findLobbyButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button aboutButton;
    [SerializeField] private Button quitGameMainMenuButton;
    [SerializeField] private Button quitGameConnectMenuButton;

    private void Awake()
    {
        Instance = this;

        findLobbyButton.onClick.AddListener(() => {
            OpenTab(tabLobbies);
        });
        optionsButton.onClick.AddListener(() => {
            OpenTab(tabOptions);
        });
        aboutButton.onClick.AddListener(() => {
            OpenTab(tabAbout);
        });
        quitGameMainMenuButton.onClick.AddListener(() => {
            QuitGame();
        });
        quitGameConnectMenuButton.onClick.AddListener(() => {
            QuitGame();
        });
    }

    private void Start()
    {
        OpenTab(tabConnect);
    }

    private void TabCloseAll()
    {
        tabConnect.SetActive(false);
        tabMain.SetActive(false);
        tabLobbies.SetActive(false);
        tabCreateLobby.SetActive(false);
        tabLobby.SetActive(false);
        tabLoading.SetActive(false);
        tabOptions.SetActive(false);
        tabAbout.SetActive(false);
    }

    public void OpenTab(GameObject tab)
    {
        TabCloseAll();
        tab.SetActive(true);
        tab.GetComponent<Animator>().enabled = true;
    }

    public void LoadingBox(string text)
    {
        TabCloseAll();
        loadingText.text = text;
        tabLoading.SetActive(true);
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}
