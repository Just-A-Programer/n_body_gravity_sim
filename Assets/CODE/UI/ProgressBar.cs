using System;
using UnityEngine;

public class ProgressBar : MonoBehaviour
{
    public Filehandler fHandler;
    public RectTransform rt;

    // Update is called once per frame
    void Update()
    {
        float newvalue = Mathf.Lerp(1100f,-1000f,((float)fHandler.current_frame/(float)(fHandler.fps*fHandler.time)));
        
        rt.offsetMax = new Vector2(-newvalue, rt.offsetMax.y);
    }
}
