using System;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class UI_Handler : MonoBehaviour
{
    private GameObject[] MS_Button_Arr = new GameObject[5];
    private GameObject[] SC_Button_Arr = new GameObject[5];
    
    [Header("MASTER BUTTONS")] 
    public GameObject MS_RenderMode;
    public GameObject MS_AddDots;
    public GameObject MS_Delete;
    public GameObject MS_ChangeColor;
    public GameObject MS_ChangeMass;
    
    [Header("SECONDARY BUTTON GROUPS")]
    public GameObject SC_RenderMode_Group;
    public GameObject SC_AddDots_Group;
    public GameObject SC_Delete_Group;
    public GameObject SC_ChangeColor_Group;
    public GameObject SC_ChangeMass_Group;

    private void Start()
    {
        
        MS_Button_Arr[0] = MS_RenderMode;
        MS_Button_Arr[1] = MS_AddDots;
        MS_Button_Arr[2] = MS_Delete;
        MS_Button_Arr[3] = MS_ChangeColor;
        MS_Button_Arr[4] = MS_ChangeMass;
        
        
        SC_Button_Arr[0] = SC_RenderMode_Group;
        SC_Button_Arr[1] = SC_AddDots_Group;
        SC_Button_Arr[2] = SC_Delete_Group;
        SC_Button_Arr[3] = SC_ChangeColor_Group;
        SC_Button_Arr[4] = SC_ChangeMass_Group;
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            for (int i = 0; i < SC_Button_Arr.Length; i++)
            {
                //SC_Button_Arr[i].GetComponent<Image>().color = Color.white;
                SC_Button_Arr[i].SetActive(false);
            }
        }
    }

    public void OnMSButtonClick(int id)
    {
        for (int i = 0; i < SC_Button_Arr.Length; i++)
        {
            //SC_Button_Arr[i].GetComponent<Image>().color = Color.white;
            SC_Button_Arr[i].SetActive(false);
        }
        //SC_Button_Arr[id].GetComponent<Image>().color = Color.green;
        SC_Button_Arr[id].SetActive(true);
    }
}
