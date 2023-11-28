using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using System.Collections.Generic;

public class SettingsMenu : MonoBehaviour
{
	[Header("MIXER")]
	[SerializeField] private AudioMixer masterMixer;

	[Header("Panels")]
	[SerializeField] private GameObject panelVideo;
	[SerializeField] private GameObject panelGame;
	[SerializeField] private GameObject panelAudio;
    [SerializeField] private GameObject panelControls;

    [Header("GAME SETTINGS")]
	[SerializeField] private Toggle showFpsToggle;
    [SerializeField] private Toggle autoAimToggle;

    [Header("VIDEO SETTINGS")]
	[SerializeField] private Toggle fullscreenToggle;
	[SerializeField] private Toggle vsyncToggle;
	[SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown qualityDropdown;

    private Resolution[] resolutions;
	private List<string> Options = new List<string>();

	[Header("AUDIO SETTINGS")]
	[SerializeField] private Slider musicSlider;
	[SerializeField] private Slider sfxSlider;
	[SerializeField] private Slider uiSlider;

    public void Start()
	{
		resolutions = Screen.resolutions;

		int currentResolutionIndex = 0;
		int x = 0;

		int lw = -1;
		int lh = -1;

		foreach (var res in resolutions)
		{
			if (lw != res.width || lh != res.height)
			{
				string option = res.width + " x " + res.height;
				Options.Add(option);
				lw = res.width;
				lh = res.height;

				if (lw == Screen.currentResolution.width && lh == Screen.currentResolution.height)
				{
					currentResolutionIndex = x;
				}

				x++;
			}
		}

		resolutionDropdown.ClearOptions();
		resolutionDropdown.AddOptions(Options);
		resolutionDropdown.value = currentResolutionIndex;

        #region PlayerPrefs

        if (PlayerPrefs.HasKey("Resolution"))
		{
			int resolutionIndex = PlayerPrefs.GetInt("Resolution");
			string resolution = Options[resolutionIndex];
			int ind = resolution.IndexOf('x');
			int width = int.Parse(resolution.Substring(0, ind - 1));
			int height = int.Parse(resolution.Substring(ind + 1));
			Screen.SetResolution(width, height, Screen.fullScreen);
			resolutionDropdown.value = resolutionIndex;
		}

        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 0) == 1;
        fullscreenToggle.isOn = isFullscreen;

        float musicVolume = PlayerPrefs.GetFloat("Music", 0.5f);
        masterMixer.SetFloat("musicVolume", Mathf.Log10(musicVolume) * 20);
        musicSlider.value = musicVolume;

        float sfxVolume = PlayerPrefs.GetFloat("SFX", 0.5f);
        masterMixer.SetFloat("sfxVolume", Mathf.Log10(sfxVolume) * 20);
        sfxSlider.value = sfxVolume;

        float uiVolume = PlayerPrefs.GetFloat("UI", 0.5f);
        masterMixer.SetFloat("uiVolume", Mathf.Log10(uiVolume) * 20);
        uiSlider.value = uiVolume;

        bool showFPS = PlayerPrefs.GetInt("FPS", 0) == 1;
        showFpsToggle.isOn = showFPS;

        bool isVSync = PlayerPrefs.GetInt("VSync", 1) == 1;
        vsyncToggle.isOn = isVSync;
        QualitySettings.vSyncCount = isVSync ? 1 : 0;

        bool autoaim = PlayerPrefs.GetInt("AutoAim", 1) == 1;
        autoAimToggle.isOn = autoaim;

        #endregion
    }

    #region Panels
    public void OpenGamePanel()
	{
		CloseAll();
        panelGame.SetActive(true);
	}

    public void OpenVideoPanel()
	{
        CloseAll();
        panelVideo.SetActive(true);
    }

    public void OpenAudioPanel()
	{
		CloseAll();
        panelAudio.SetActive(true);
	}

    public void OpenControlsPanel()
    {
		CloseAll();
        panelControls.SetActive(true);
    }

	private void CloseAll()
	{
        panelVideo.SetActive(false);
        panelGame.SetActive(false);
        panelAudio.SetActive(false);
		panelControls.SetActive(false);
    }
    #endregion

    #region PanelGame

    public void ShowFPS(bool showFPS)
	{
        PlayerPrefs.SetInt("FPS", showFPS ? 1 : 0);
    }

    public void AutoAim(bool isOn)
    {
        PlayerPrefs.SetInt("AutoAim", isOn ? 1 : 0);
		if (Player.Instance)
		{
			Player.Instance.AutoAim = isOn;
        }
    }

    #endregion

    #region PanelVideo

    public void FullScreen(bool isFullscreen)
	{
        Screen.fullScreen = isFullscreen;

        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
	}

	public void SetResolution(int resolutionIndex)
	{
		string resolution = Options[resolutionIndex];
		int ind = resolution.IndexOf('x');
		int width = int.Parse(resolution.Substring(0, ind - 1));
		int height = int.Parse(resolution.Substring(ind + 1));
		Screen.SetResolution(width, height, Screen.fullScreen);
		PlayerPrefs.SetInt("Resolution", resolutionIndex);
	}

	public void Vsync(bool isVsync)
	{
        QualitySettings.vSyncCount = isVsync ? 1 : 0;

        PlayerPrefs.SetInt("VSync", isVsync ? 1 : 0);
    }

	#endregion

	#region PanelAudio

	public void SetMusicVolume(float volume)
    {
        masterMixer.SetFloat("musicVolume", Mathf.Log10(volume) * 20);
		PlayerPrefs.SetFloat("Music", volume);
    }

    public void SetSFXVolume(float volume)
    {
		masterMixer.SetFloat("sfxVolume", Mathf.Log10(volume) * 20);
		PlayerPrefs.SetFloat("SFX", volume);
    }

    public void SetUIVolume(float volume)
    {
		masterMixer.SetFloat("uiVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("UI", volume);
    }

    #endregion
}
