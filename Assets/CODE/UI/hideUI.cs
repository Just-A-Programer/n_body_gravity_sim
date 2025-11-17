using System;
using System.Net.NetworkInformation;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class hideUI : MonoBehaviour
{
    public bool HideUI = true;
    public GameObject canvas;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(Input.KeyCode.h))
        {
            HideUI = !HideUI;
            canvas.SetActive(HideUI);
        }
    }
}
