using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Black_hole_handler : MonoBehaviour
{
    public ComputeShader computeShader;
    public Filehandler fHandler;
    public GameObject BlackHole;
    
    public bool BlackHole_isActive;
    public float BlackHole_Size;
    public float BlackHole_Mass;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B) && (!fHandler.WRITING && !fHandler.READING))
        {
            ToggleBlackHole();
        }
    }

    public void ToggleBlackHole()
    {
        BlackHole_isActive = !BlackHole_isActive;
        BlackHole.SetActive(BlackHole_isActive);
        BlackHole.transform.localScale = new Vector2(BlackHole_Size, BlackHole_Size);
        
        computeShader.SetInt("BlackHole_isActive", Convert.ToInt32(BlackHole_isActive));
        computeShader.SetFloat("BlackHole_Size", BlackHole_Size);
        computeShader.SetFloat("BlackHole_Mass", BlackHole_Mass);
    }
}
