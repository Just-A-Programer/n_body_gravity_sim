using UnityEngine;

public class camera : MonoBehaviour
{
    public gravity_Csharp grav_Script;
    
    public float movement_speed;
    public float sprint_multiplayer;
    public float scroll_sensitivity;

    private float zoomMultiplier = 4f;
    private float velocity = 0f;
    private float smoothTime = 0.25f;

    GameObject cam;
    float zoom;
    bool isfullscreen;

    Vector2 mov_directions;
    int issprinting = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = Camera.main.gameObject;
        zoom = Camera.main.orthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift)) { issprinting = 1; }
        else { issprinting = 0; }

        if (Input.GetKey(KeyCode.D)) { cam.transform.position = cam.transform.position + new Vector3(movement_speed + (movement_speed * issprinting * (sprint_multiplayer - 1)), 0, 0) * Time.deltaTime * zoomMultiplier; }    
        if (Input.GetKey(KeyCode.A)) { cam.transform.position = cam.transform.position - new Vector3(movement_speed + (movement_speed * issprinting * (sprint_multiplayer - 1)), 0, 0) * Time.deltaTime * zoomMultiplier; }
        if (Input.GetKey(KeyCode.W)) { cam.transform.position = cam.transform.position + new Vector3(0, movement_speed + (movement_speed * issprinting * (sprint_multiplayer - 1)), 0) * Time.deltaTime * zoomMultiplier; }
        if (Input.GetKey(KeyCode.S)) { cam.transform.position = cam.transform.position - new Vector3(0, movement_speed + (movement_speed * issprinting * (sprint_multiplayer - 1)), 0) * Time.deltaTime * zoomMultiplier; }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (grav_Script.mouse_MODE == -1 || Input.GetKey(KeyCode.LeftControl)) { zoom -= Mathf.Pow(scroll * zoomMultiplier * sprint_multiplayer, 2) * Mathf.Sign(scroll); }
        Camera.main.orthographicSize = Mathf.SmoothDamp(Camera.main.orthographicSize, zoom, ref velocity, smoothTime);

        if (Input.GetKeyDown(KeyCode.F11))
        {
            isfullscreen = !isfullscreen;
            Screen.fullScreen = isfullscreen;
        }
    }
}
