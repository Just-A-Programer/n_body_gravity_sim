using UnityEngine;

public class DebugUI : MonoBehaviour
{
    public gravity_Csharp script;


    TMPro.TextMeshProUGUI text;
    string textstart;
    public int ID;

    private void Start()
    {
        text = gameObject.GetComponent<TMPro.TextMeshProUGUI>();
        textstart = text.text;
    }
    // Update is called once per frame
    void Update()
    {
        if (ID == 0) text.text = textstart + (script.dotCount - script.freeSpace).ToString();
        else if (ID == 1) text.text = textstart + script.RENDER_MODE.ToString();
        else if (ID == 2) text.text = textstart + script.MassPresent.ToString();
    }
}
