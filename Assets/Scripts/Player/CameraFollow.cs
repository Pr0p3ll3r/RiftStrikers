using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset;

    public Transform player;

    public void SetPlayer(Transform _player)
    {
        player = _player;
    }

    private void LateUpdate()
    {
        if(player != null)
            transform.position = player.position + offset;
    }
}
