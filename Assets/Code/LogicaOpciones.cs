using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LogicaOpciones : MonoBehaviour   
{
    public bool isPaused = false;
    public ControladorOpciones panelOpciones;
    private EventSystem eventSystem;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        panelOpciones = GameObject.FindGameObjectWithTag("opciones").GetComponent<ControladorOpciones>();
        eventSystem = EventSystem.current;

        if (eventSystem == null)
        {
            Debug.LogWarning("No se encontró un EventSystem en la escena. El menú no podrá recibir input.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        ManejoPausa();
    }

    public void ManejoPausa()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
            {
                MostrarOpciones();
            }
            else
            {
                EsconderOpciones();
            }
        }
    }
    public void MostrarOpciones()
    {
        panelOpciones.optionsScreen.SetActive(true);
        Time.timeScale = 0;
        isPaused = true;

        if (eventSystem != null)
        {
            eventSystem.SetSelectedGameObject(null);
            if (panelOpciones.firstSelectedButton != null)
            {
                eventSystem.SetSelectedGameObject(panelOpciones.firstSelectedButton);
            }
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

        public void EsconderOpciones()
    {
        panelOpciones.optionsScreen.SetActive(false);
        Time.timeScale = 1;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
