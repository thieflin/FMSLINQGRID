using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Reset : MonoBehaviour
{
    public void Quit()
    {
        Application.Quit();
    }

    public void RestartScene()
    {
    SceneManager.LoadScene(0);

    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartScene();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Quit();
        }
    }
}
