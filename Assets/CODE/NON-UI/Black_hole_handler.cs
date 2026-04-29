using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Black_hole_handler : MonoBehaviour
{
    public ComputeShader computeShader;
    public gravity_Csharp gravscript;
    public Filehandler fHandler;
    public GameObject BHMeniu;
    public GameObject BlackHole;
    
    public bool BlackHole_isActive;
    public float BlackHole_Size;
    public float BlackHole_Mass;

    private const float true_G = 0.00000000006674f;

    public void SetMass(string str)
    {
        if (float.TryParse(str, out float newMass) && newMass != 0)
        {
            BlackHole_Mass = newMass;
            UpdateBH();
        }
    }

    public void SetRadius(string str)
    {
        if (float.TryParse(str, out float newSize) && newSize != 0)
        {
            BlackHole_Size = newSize;
            UpdateBH();
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B) && (!fHandler.WRITING && !fHandler.READING))
        {
            ToggleBlackHole();
        }
    }

    public void UpdateBH()
    {
        BlackHole.transform.localScale = new Vector2(2*BlackHole_Size, 2*BlackHole_Size);
        
        computeShader.SetInt("BlackHole_isActive", Convert.ToInt32(BlackHole_isActive));
        computeShader.SetFloat("BlackHole_Size", BlackHole_Size);
        computeShader.SetFloat("BlackHole_Mass", BlackHole_Mass);
    }
    public void ToggleBlackHole()
    {
        BlackHole_isActive = !BlackHole_isActive;
        BlackHole.SetActive(BlackHole_isActive);
        //BlackHole_Size = (gravscript.G/true_G) * ((2 * gravscript.G * BlackHole_Mass) / 89875517874000000);
        BlackHole.transform.localScale = new Vector2(2*BlackHole_Size, 2*BlackHole_Size);
        
        computeShader.SetInt("BlackHole_isActive", Convert.ToInt32(BlackHole_isActive));
        computeShader.SetFloat("BlackHole_Size", BlackHole_Size);
        computeShader.SetFloat("BlackHole_Mass", BlackHole_Mass);
    }
}
