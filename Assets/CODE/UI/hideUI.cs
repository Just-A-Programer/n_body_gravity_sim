using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class hideUI : MonoBehaviour
{
    public bool HideUI = true;
    public GameObject canvas;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            HideUI = !HideUI;
            canvas.SetActive(HideUI);
            Cursor.visible = HideUI;
        }
    }
}
