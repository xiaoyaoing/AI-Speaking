using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuAdminSprite : MonoBehaviour
{
    // Start is called before the first frame update
    private float timer = 0.0f;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 5.0)
        {
            SceneManager.LoadScene("Conference Room");
        }
    }
    
    public void GameStart()
    {
        SceneManager.LoadScene("Conference Room");
    }

    public void GameQuit()
    {
        Application.Quit();
    }
}
