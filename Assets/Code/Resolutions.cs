using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NUnit.Framework;

public class Resolutions : MonoBehaviour
{
    public Toggle toggle;

    public TMP_Dropdown resolutionsDropDown;
    Resolution[] resolutions;

    // Start is called before the first frame update
    void Start()
    {
        if (Screen.fullScreen)
        {
            toggle.isOn = true;
        }
        else 
        {
            toggle.isOn = false;
        }

        CheckResolution();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateFullScreen(bool FullScreen)
    {
        Screen.fullScreen = FullScreen;
    }
    public void CheckResolution()
    {
        resolutions = Screen.resolutions;
        resolutionsDropDown.ClearOptions();
        List<string> options = new List<string>();
        int actualResolution = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (Screen.fullScreen && resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                actualResolution = i;
            }
        }

        resolutionsDropDown.AddOptions(options);
        resolutionsDropDown.value = actualResolution;
        resolutionsDropDown.RefreshShownValue();
    }

    public void ChangeResolution(int indexResolution)
    {
        Resolution resolution = resolutions[indexResolution];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}
         
     
