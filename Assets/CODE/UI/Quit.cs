using UnityEngine;
//using UnityEditor;
public class Quit : MonoBehaviour
{
    public void QuitGame()
    {
        Application.Quit();
        //EditorApplication.ExitPlaymode();
    }
}
