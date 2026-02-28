using System;
using UnityEngine;

public class ProgressBar : MonoBehaviour
{
    public Filehandler fHandler;
    public RectTransform rt;
    public bool write;
    
    // Update is called once per frame
    void Update()
    {
        float newvalue = 0;
        if (write)
        {
            newvalue = Mathf.Lerp(1100f, -1000f, ((float)fHandler.current_frame / (float)(fHandler.fps * fHandler.time)));
        }
        else {
            newvalue = Mathf.Lerp(1100f, -1000f, ((float)fHandler.current_frame / (float)(fHandler.rfps * fHandler.rtime)));
        }
        rt.offsetMax = new Vector2(-newvalue, rt.offsetMax.y);
    }
}
