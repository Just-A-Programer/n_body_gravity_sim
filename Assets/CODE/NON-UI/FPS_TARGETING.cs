using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class FPS_TARGETING : MonoBehaviour
{
    [Range(0, 120)]
    public int TARGET_FPS = 60;
    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TARGET_FPS;
    }
    void Update()
    {
        if (Application.targetFrameRate != TARGET_FPS)
            Application.targetFrameRate = TARGET_FPS;
    }
}
