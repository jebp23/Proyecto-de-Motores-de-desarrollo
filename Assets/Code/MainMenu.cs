using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartLevel(string LevelName)
    {
        SceneManager.LoadScene(LevelName);
    }
   public void Salir()
    {
        Application.Quit();
        Debug.Log("Juego cerrado");
    }
}
