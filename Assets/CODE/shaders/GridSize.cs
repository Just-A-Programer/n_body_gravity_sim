using UnityEngine;

public class GridSize : MonoBehaviour
{
    [Range(1, 100)]
    public int size;
    private Camera cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        float newSize = size*cam.orthographicSize;
        gameObject.transform.localScale = new Vector3(newSize + Mathf.Floor(cam.transform.position.x), newSize, 1);
        
    }
}
