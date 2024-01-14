using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;

public class AccountManager : MonoBehaviour
{
    public static AccountManager Instance { get; private set; }

    public event EventHandler OnSignUpStarted;
    public event EventHandler OnSignedUp;
    public event EventHandler<string> OnSignUpFailed;
    public event EventHandler OnAuthenticateStarted;
    public event EventHandler OnAuthenticated;
    public event EventHandler<string> OnAuthenticateFailed;

    [Header("Profile")]
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [Header("Register")]
    [SerializeField] private TMP_InputField usernameInputFieldRegister;
    [SerializeField] private TMP_InputField passwordInputFieldRegister;
    [SerializeField] private TMP_InputField confirmPasswordInputFieldRegister;
    [Header("Login")]
    [SerializeField] private TMP_InputField usernameInputFieldLogin;
    [SerializeField] private TMP_InputField passwordInputFieldLogin;

    async void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        try
        {
            await UnityServices.InitializeAsync();
            bool isSignedIn = AuthenticationService.Instance.IsSignedIn;
            if (isSignedIn)
            {
                LoadData();
            }
            else
            {
                MenuManager.Instance.OpenTab(MenuManager.Instance.tabLogin);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Signed in! " + AuthenticationService.Instance.PlayerId);
            LoadData();
        };
    }

    private void Start()
    {
        VivoxService.Instance.LoggedIn += OnLoggedIn;
    }

    private void OnDestroy()
    {
        VivoxService.Instance.LoggedIn -= OnLoggedIn;
    }

    public async void SignUp()
    {
        OnSignUpStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            await SignUpWithUsernamePassword(usernameInputFieldRegister.text, passwordInputFieldRegister.text);
        }
        catch (AuthenticationException e)
        {
            Debug.Log(e);
            OnSignUpFailed?.Invoke(this, e.Message);
        }      
    }

    private async Task SignUpWithUsernamePassword(string username, string password)
    {
        if(password != confirmPasswordInputFieldRegister.text)
        {
            OnSignUpFailed?.Invoke(this, "Passwords don't match");
            return;
        }
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            CloudData.PlayerData.Name = username;
            Debug.Log("SignUp is successful.");
            OnSignedUp?.Invoke(this, EventArgs.Empty);
            CloudData.Save();
            usernameText.text = username;
            moneyText.text = $"${CloudData.PlayerData.Money}";
            MenuManager.Instance.OpenTab(MenuManager.Instance.tabMain);
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
            OnSignUpFailed?.Invoke(this, ex.Message);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
            OnSignUpFailed?.Invoke(this, ex.Message);
        }
    }

    public async void SignIn()
    {
        OnAuthenticateStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            await SignInWithUsernamePassword(usernameInputFieldLogin.text, passwordInputFieldLogin.text);
        }
        catch (AuthenticationException e)
        {
            Debug.Log(e);
            OnAuthenticateFailed?.Invoke(this, e.Message);
        }
    }

    private async void LoadData()
    {
        try
        {
            CloudData.PlayerData = await CloudData.RetrieveSpecificData<PlayerData>("PlayerData");
            Debug.Log("Loaded Data");
            usernameText.text = CloudData.PlayerData.Name;
            moneyText.text = $"${CloudData.PlayerData.Money}";
            if (!VivoxService.Instance.IsLoggedIn)
                LoginToVivox();
            else
                MenuManager.Instance.OpenTab(MenuManager.Instance.tabMain);
        }
        catch(CloudSaveException e)
        {
            Debug.Log(e);
        }    
    }

    private async Task SignInWithUsernamePassword(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
            OnAuthenticateFailed?.Invoke(this, ex.Message);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
            OnAuthenticateFailed?.Invoke(this, ex.Message);
        }
    }

    private async void LoginToVivox()
    {
        var loginOptions = new LoginOptions()
        {
            DisplayName = CloudData.PlayerData.Name,
            ParticipantUpdateFrequency = ParticipantPropertyUpdateFrequency.FivePerSecond
        };
        try
        {
            Debug.Log("Connecting to Vivox...");
            await VivoxService.Instance.LoginAsync(loginOptions);        
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }       
    }

    private void OnLoggedIn()
    {
        Debug.Log("Connected to Vivox");
        MenuManager.Instance.OpenTab(MenuManager.Instance.tabMain);
        OnAuthenticated?.Invoke(this, EventArgs.Empty);
    }
}
