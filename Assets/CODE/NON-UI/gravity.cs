using UnityEngine;

public class gravity : MonoBehaviour
{
    public float G = 0.00000000006743f;
    public GameObject myself;
    GameObject[] dots;


    // Update is called once per frame
    void FixedUpdate()
    {
        dots = GameObject.FindGameObjectsWithTag("GRAVABLE_OBJ");
        for (int i = 0; i < dots.Length; i++)
        {
            if (dots[i] != myself)
            {
                float d = Mathf.Sqrt(Mathf.Pow((myself.transform.position.x - dots[i].transform.position.x), 2) + Mathf.Pow((myself.transform.position.y - dots[i].transform.position.y), 2));

                float f = G / d;

                float alpha = Mathf.Atan2((myself.transform.position.y - dots[i].transform.position.y), (myself.transform.position.x - dots[i].transform.position.x));
                Vector2 f_vec = new Vector2(-(Mathf.Cos(alpha) * f), -(Mathf.Sin(alpha) * f));

                myself.GetComponent<Rigidbody2D>().AddForce(f_vec);
            }

        }
    }
}
