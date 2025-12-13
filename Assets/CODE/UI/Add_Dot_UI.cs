using UnityEngine;
using System.Collections;

public class Add_Dot_UI : MonoBehaviour
{
    public gravity_Csharp gravityscript;
    public GameObject centerprefab;
    public GameObject lenghtprefab;
    GameObject centerobj;
    GameObject[] lenghtobj;

    public bool active;
    Vector2 center;
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

        centerobj = Instantiate(centerprefab, new Vector3(center.x, center.y, 0f));
        Debug.Log("first");
    }
    void Updatepass()
    {
        addvelocity = center - currPos;
        gravityscript.VelocityPresent = addvelocity;
        //Debug.Log(gravityscript.VelocityPresent);

    }
    public void finishingpass()
    {
        Debug.Log("finish");

        active = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (active) { 
            Updatepass();

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                finishingpass();
            }

        }
    }
}
