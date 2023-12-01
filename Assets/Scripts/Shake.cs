using UnityEngine;

public class Shake : MonoBehaviour
{
    private float distance = 0.03f;
    private Vector3 startPos;
    private Vector3 randomPos;
    private bool shaking;

    private void Awake()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        if(shaking)
        {
            randomPos = startPos + (Random.insideUnitSphere * distance);
            transform.position = randomPos;
        }
    }

    public void StartShaking()
    {
        shaking = true;
    }
}
