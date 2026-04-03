using UnityEngine;
using UnityEngine.PlayerLoop;
using Update = Unity.VisualScripting.Update;

public class ResumePauseClicker : MonoBehaviour
{
    public GameObject pause;
    public GameObject resume;
    
    public void PresedPause()
    {
        pause.SetActive(false);
        resume.SetActive(true);
    }

    public void PresedResume()
    {
        pause.SetActive(true);
        resume.SetActive(false);
    }
}
