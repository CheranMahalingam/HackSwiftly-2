using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    //public GameObject slide;
    public Text txt;
    public int SceneIndex;
    AsyncOperation operation;
    // Start is called before the first frame update
    void Start()
    {
        operation = SceneManager.LoadSceneAsync(SceneIndex);
        UnityEngine.Debug.Log("scene started loading");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadLevel()
    {

        StartCoroutine(LoadAsynchronously());
        
    }

    IEnumerator LoadAsynchronously ()
    {

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);
            UnityEngine.Debug.Log(progress);
            txt.text = progress.ToString() + "%";

            yield return null;
        }

    }
}
