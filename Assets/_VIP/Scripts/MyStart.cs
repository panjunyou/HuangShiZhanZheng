using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UIRoot.SetInitParams(new Vector2(1080, 1920));
        UIPage.ShowPageAsync<LogoPage>(); 
    }

    
}
