using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameResumer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Resume(bool forceReset)
    {
        Manager.forceReset = forceReset;
        SceneManager.LoadSceneAsync("Game");
    }
}
