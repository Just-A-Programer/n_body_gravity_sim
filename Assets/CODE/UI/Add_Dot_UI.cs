using UnityEngine;
using System.Collections;

public class Add_Dot_UI : MonoBehaviour
{
    public gravity_Csharp gravityscript;
    public GameObject centerballObj;
    public GameObject lenghtprefab;
    //public GameObject veltextprefab;
    GameObject[] lenghtobj;
    GameObject[] veltextobj;

    public float sensitivity;
    public float centerballSize;
    public bool active;
    Vector2 center;
    int clickcounter;
    Vector2 currPos;
    Vector2 addvelocity;
    Camera cam;


    void Start()
    {
        cam = Camera.main;
    }

    public void firstpass()
    {
        active = true;
        Vector2 mousePos = Input.mousePosition;

        center = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));

        centerballObj.SetActive(true);
        centerballObj.transform.position = new Vector3(center.x, center.y, 0);
        centerballObj.transform.localScale = new Vector3(cam.orthographicSize*centerballSize, cam.orthographicSize*centerballSize, 0);
        
        gravityscript.PositionPresent = center;
        
        /*lenghtobj[0] = Instantiate(lenghtprefab);
        lenghtobj[0].transform.position = new Vector3(center.x, center.y, 0);
        lenghtobj[0].transform.localScale = new Vector3(cam.orthographicSize*centerballSize, cam.orthographicSize*centerballSize, 0);*/
    }
    void Updatepass()
    {
        Vector2 mousePos = Input.mousePosition;
        currPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
        
        addvelocity = -(center - currPos)/sensitivity;
        
        gravityscript.VelocityPresent = addvelocity;
        
    }
    public void finishingpass()
    {
        centerballObj.SetActive(false);
        clickcounter = 0;
        active = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (active) { 
            Updatepass();

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                clickcounter++;
                if(clickcounter == 2)
                {
                    finishingpass();
                }
            }

            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                finishingpass();
            }


        }
    }
}
