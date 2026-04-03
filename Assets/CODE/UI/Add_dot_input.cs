using System;
using Unity.VisualScripting;
using UnityEngine;

public class Add_dot_input : MonoBehaviour
{
    public gravity_Csharp gravscript;
    public GameObject centerballObj;
    public string place;
    TMPro.TMP_InputField text;
    float value;

    private void Awake()
    {
        text = GetComponent<TMPro.TMP_InputField>();
    }

    private void Update()
    {
        if (!text.isFocused)
        {
            if (centerballObj.activeSelf)
            {
                switch (place)
                {
                    case "xp":
                        value = gravscript.PositionPresent.x;
                        break;
                    case "yp":
                        value = gravscript.PositionPresent.y;
                        break;
                    case "xv":
                        value = gravscript.VelocityPresent.x;
                        break;
                    case "yv":
                        value = gravscript.VelocityPresent.y;
                        break;
                    case "m":
                        value = gravscript.MassPresent;
                        break;
                }
            }
            else
            {
                Vector2 mousePos = Input.mousePosition;
                mousePos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));
                
                switch (place)
                {
                    case "xp":
                        value = mousePos.x;
                        break;
                    case "yp":
                        value = mousePos.y;
                        break;
                    case "xv":
                        value = 0;
                        break;
                    case "yv":
                        value = 0;
                        break;
                    case "m":
                        value = gravscript.MassPresent;
                        break;
                }
            }


            text.text = value.ToString();
        }
    }

    public void Onchange(string str)
    {
        try
        {
            value = float.Parse(str);

            if (value == null)
                value = 0;
            
            switch (place)
            {
                case "xp":
                    gravscript.PositionPresent.x = value;
                    break;
                case "yp":
                    //gravscript.PositionPresent.y = value;
                    break;
                case "xv":
                    gravscript.VelocityPresent.x = value;
                    break;
                case "yv":
                    gravscript.VelocityPresent.y = value;
                    break;
                case "m":
                    gravscript.MassPresent = value;
                    break;
            }
        }
        catch (Exception e) {}
             
        
    }
}
