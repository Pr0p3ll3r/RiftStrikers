using UnityEngine;
using UnityEngine.UI;

public class FPSDisplay : MonoBehaviour
{
    private float deltaTime = 0.0f;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        if(PlayerPrefs.GetInt("FPS") == 1)
        {
            int w = Screen.width, h = Screen.height;

            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(0, 0, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 100;
            style.normal.textColor = Color.green;
            float fps = 1.0f / deltaTime;
            string text = string.Format("{0:0.} fps", fps);
            GUI.Label(rect, text, style);
        }      
    }
}