using FishNet;
using FishNet.Transporting.UTP;
using System.Threading.Tasks;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public async Task<string> CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var utp = (FishyUnityTransport)InstanceFinder.TransportManager.Transport;

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            utp.SetRelayServerData(relayServerData);

            InstanceFinder.ServerManager.StartConnection();

            InstanceFinder.ClientManager.StartConnection();

            return joinCode;
        } 
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }

    public async void JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Joining Relay with " + joinCode);
            var utp = (FishyUnityTransport)InstanceFinder.TransportManager.Transport;

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            utp.SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            InstanceFinder.ClientManager.StartConnection();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
}
