using System.Collections;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance;
    [SerializeField] private Animator transition;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public IEnumerator Crossfade(float time)
    {
        transition.SetBool("Fade", true);

        yield return new WaitForSeconds(time);

        transition.SetBool("Fade", false);
    }
}
