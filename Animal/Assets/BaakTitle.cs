using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BaakTitle : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke("GoTitle", 60f);
    }

    void GoTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }

}
