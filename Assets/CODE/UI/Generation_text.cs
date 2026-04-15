using System;
using UnityEngine;

public class Generation_text : MonoBehaviour
{ 
    public Filehandler fHandler;
    public GameObject StopButton;
    public GameObject PlayButton;
    TMPro.TextMeshProUGUI text;
    string donetext;
    float fps;
    float avg = 0;
    float speed = 0;
    
    float Time_start;
    float Time_end;
    float Time_elapsed;
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
        if (donetext == "") { Time_elapsed = Time.time - Time_start;}
        if (fHandler.current_frame >= (fHandler.fps * fHandler.time))
        {
            donetext = "Baigta!! \n" /*+ Mathf.Round((((fHandler.fps * fHandler.time) / Time_elapsed))*1000)/1000 + " K/s"*/;
            Time_end = Time_elapsed;
            StopButton.SetActive(true);
            PlayButton.SetActive(true);
        }
        else
        {
            donetext = "";
            StopButton.SetActive(false);
            PlayButton.SetActive(false);
        }
        
        
        fps = (float)fHandler.current_frame / Time_elapsed;


        if (donetext != "") { speed = 0;}
        if (donetext == "" && fHandler.current_frame % 10 == 0)
        {
            speed = Mathf.Round((1f / Time.deltaTime) * 100) / 100;
        }
        
        text.text = 
            Mathf.Floor((float)fHandler.current_frame / (fHandler.fps * fHandler.time) * 10000) / 100 + "% \n" +           //percentage
            fHandler.current_frame + " / " + fHandler.fps * fHandler.time + "\n" +                                           //ratio
            speed + " K/s \n" +
            "Likęs laikas: " + Mathf.Floor((float)(fHandler.fps * fHandler.time - fHandler.current_frame) / (fps)*100)/100 + "s\n" +//ETA
            donetext                                                                                                         //isDone
            ;
    }
}
