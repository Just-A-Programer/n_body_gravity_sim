using System;
using UnityEngine;

public class Generation_text : MonoBehaviour
{ 
    public Filehandler fHandler;
    TMPro.TextMeshProUGUI text;
    private float fpsUpdateTimer = 0;
    private string donetext;
    float fps;
    
    private float avg = 0;
    
    
    private void Start()
    {
        text = this.GetComponent<TMPro.TextMeshProUGUI>();
    }

    private void Update()
    {
        if (fHandler.current_frame >= (fHandler.fps * fHandler.time))
        {
            donetext = "Baita!!";
        }
        else
        {
            donetext = "";
        }
        
        avg += ((Time.deltaTime/Time.timeScale) - avg) * 0.03f;
        fps = 1f / avg;
        text.text = 
            Mathf.Floor((float)fHandler.current_frame / (fHandler.fps * fHandler.time) * 10000) / 100 + "% \n" +
            fHandler.current_frame + " / " + fHandler.fps * fHandler.time + "\n" +
            "ETA: " + Mathf.Floor((float)(fHandler.fps * fHandler.time - fHandler.current_frame) / (fps)*100)/100 + "s\n" +
            donetext
            ;
    }
}
