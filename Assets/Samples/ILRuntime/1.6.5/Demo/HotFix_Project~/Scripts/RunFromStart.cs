using UnityEngine;
using UnityEngine.SceneManagement;

public  class RunFromStart:MonoBehaviour
{
    private void Awake()
    {
#if UNITY_EDITOR
        if (GameObject.Find("UIRoot")==null)
        {
            SceneManager.LoadScene("Start");
            return;
        }
#endif
    }
}
