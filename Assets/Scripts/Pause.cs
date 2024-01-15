using UnityEngine;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    public static Pause Instance {  get; private set; }

    public static bool paused = false;

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject settingsMenu;

    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button returnButton;

    private void Awake()
    {
        Instance = this;
    }

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

    public void TooglePause()
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
        GameManager.Instance.ReturnToMenu();
    }
}