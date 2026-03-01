using System;
using UnityEngine;

public class Generation_text : MonoBehaviour
{ 
    public Filehandler fHandler;
    public GameObject PlayButton;
    TMPro.TextMeshProUGUI text;
    string donetext;
    float fps;
    float avg = 0;

    float Time_start;
    float Time_end;
    
    private void Start()
    {
        text = this.GetComponent<TMPro.TextMeshProUGUI>();
    }

    private void Awake()
    {
        Time_start = Time.time;
    }

    private void Update()
    {
        float Time_elapsed = Time.time - Time_start;
        if (fHandler.current_frame >= (fHandler.fps * fHandler.time))
        {
            donetext = "Baigta!!";
            Time_end = Time_elapsed;
            PlayButton.SetActive(true);
        }
        else
        {
            donetext = "";
            PlayButton.SetActive(false);
        }
        
        
        fps = (float)fHandler.current_frame / Time_elapsed;
        text.text = 
            Mathf.Floor((float)fHandler.current_frame / (fHandler.fps * fHandler.time) * 10000) / 100 + "% \n" +
            fHandler.current_frame + " / " + fHandler.fps * fHandler.time + "\n" +
            "ETA: " + Mathf.Floor((float)(fHandler.fps * fHandler.time - fHandler.current_frame) / (fps)*100)/100 + "s\n" +
            donetext
            ;
    }
}
