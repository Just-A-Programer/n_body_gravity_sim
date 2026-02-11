using UnityEngine;

public class fastforward : MonoBehaviour
{
    public float TimeScale = 1f;
    public TMPro.TextMeshProUGUI text;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            TimeScale++;
            Time.timeScale = TimeScale;
        }
        if (Input.GetKeyDown(KeyCode.KeypadMinus) && TimeScale > 1)
        {
            TimeScale--;
            Time.timeScale = TimeScale;
        }
        text.text = "Laiko greitis: " + TimeScale;
    }
}
