using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderManager : MonoBehaviour
{
    public void LoadNextScene()
    {
        //StartCoroutine(SceneTransition(SceneManager.GetActiveScene().buildIndex + 1));
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void LoadStartScene()
    {
        SceneManager.LoadScene(0);
    }

    /*private IEnumerator SceneTransition(int sceneIndex)
    {
        yield return new WaitForSeconds(sceneIndex);
        SceneManager.LoadScene(sceneIndex);
    }*/
}