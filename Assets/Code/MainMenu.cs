using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
   
    [SerializeField] GameObject background;
    [SerializeField] GameObject gameTitle;
    [SerializeField] GameObject btnPlay;
    [SerializeField] GameObject btnOptions;
    [SerializeField] GameObject btnExit;
    [SerializeField] GameObject optionsCanvasRoot;
    [SerializeField] GameObject optionsPanel;
    [SerializeField] Canvas mainMenuCanvas;
    [SerializeField] Canvas optionsCanvas;
    [SerializeField] bool bringOptionsToFront = true;

    void Awake()
    {
        if (!mainMenuCanvas) mainMenuCanvas = GetComponentInParent<Canvas>();
        if (!optionsCanvas && optionsCanvasRoot)
            optionsCanvas = optionsCanvasRoot.GetComponentInChildren<Canvas>(true);


        if (!optionsPanel && optionsCanvasRoot)
            optionsPanel = FindDeep(optionsCanvasRoot.transform, "OptionsMenu")?.gameObject;


        SafeSetActive(optionsPanel, false);
        SafeSetActive(optionsCanvasRoot, false);


        if (bringOptionsToFront && optionsCanvas)
        {
            optionsCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            optionsCanvas.overrideSorting = true;
            int baseOrder = mainMenuCanvas ? mainMenuCanvas.sortingOrder : 0;
            optionsCanvas.sortingOrder = baseOrder + 10;
        }


        SafeSetActive(background, true);
        SafeSetActive(gameTitle, true);
        SafeSetActive(btnPlay, true);
        SafeSetActive(btnOptions, true);
        SafeSetActive(btnExit, true);
    }


    public void StartLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }


    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Juego cerrado");
    }

   
    public void OpenOptions()
    {    
        SafeSetActive(background, true);
        SafeSetActive(gameTitle, true);

        SafeSetActive(btnPlay, false);
        SafeSetActive(btnOptions, false);
        SafeSetActive(btnExit, false);

        SafeSetActive(optionsCanvasRoot, true);
        SafeSetActive(optionsPanel, true);
    }


    public void CloseOptions()
    {
        SafeSetActive(optionsPanel, false);
        SafeSetActive(optionsCanvasRoot, false);

        SafeSetActive(btnPlay, true);
        SafeSetActive(btnOptions, true);
        SafeSetActive(btnExit, true);

        SafeSetActive(background, true);
        SafeSetActive(gameTitle, true);
    }

    
    private void SafeSetActive(GameObject go, bool on)
    {
        if (go && go.activeSelf != on) go.SetActive(on);
    }

    private Transform FindDeep(Transform root, string name)
    {
        if (root.name == name) return root;
        foreach (Transform c in root)
        {
            var r = FindDeep(c, name);
            if (r) return r;
        }
        return null;
    }
}
