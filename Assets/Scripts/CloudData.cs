using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using UnityEngine;

public static class CloudData
{
    public static PlayerData PlayerData = new PlayerData();

    public static async void Save()
    {
        await SaveAsync();
    }

    public static async Task SaveAsync()
    {
        var data = new Dictionary<string, object> { { "PlayerData", PlayerData } };
        try
        {
            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
        }
        catch (CloudSaveValidationException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveRateLimitedException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveException e)
        {
            Debug.LogError(e);
        }   
    }

    public static async Task<T> RetrieveSpecificData<T>(string key)
    {
        try
        {
            var results = await CloudSaveService.Instance.Data.Player.LoadAsync(
                new HashSet<string> { key }
            );

            if (results.TryGetValue(key, out var item))
            {
                return item.Value.GetAs<T>();
            }
            else
            {
                Debug.Log($"There is no such key as {key}!");
            }
        }
        catch (CloudSaveValidationException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveRateLimitedException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveException e)
        {
            Debug.LogError(e);
        }

        return default;
    }
}