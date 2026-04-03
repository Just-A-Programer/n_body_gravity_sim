using UnityEngine;
using System;

public class TimeStamp : MonoBehaviour
{
    TMPro.TextMeshProUGUI outtext;
    public Filehandler fHandler;
    int rfps;
    int rtime;
    uint cframe;
    uint totalseconds;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        outtext = this.GetComponent<TMPro.TextMeshProUGUI>();
        rfps = fHandler.rfps;
        rtime = fHandler.rtime;

        totalseconds = (uint)(rtime);
    }

    // Update is called once per frame
    void Update()
    {
        cframe = fHandler.current_frame;
        
        float cseconds = (float)cframe / rfps;
        
        outtext.text = Mathf.Floor((float)cseconds / 60) + ":" + (Mathf.Floor((float)cseconds % 60)).ToString("00")
                       + " / " + 
                       Mathf.Floor((float)totalseconds / 60) + ":" + (Mathf.Floor((float)totalseconds % 60)).ToString("00");
    }
}
