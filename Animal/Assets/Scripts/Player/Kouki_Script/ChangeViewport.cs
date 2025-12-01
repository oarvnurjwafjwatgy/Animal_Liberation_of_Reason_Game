using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeViewport : MonoBehaviour
{
    public Camera cam;
    public float x = 0f;
    public float y = 0f;
    public float w = 1f;
    public float h = 1f;
    [SerializeField] GameObject playerManager;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();

        playerManager = GameObject.Find("PlayerManager");
        var playerManagerClass = playerManager.GetComponent<PlayerManager>().playerCount;
        Debug.Log(playerManagerClass.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        if (cam != null)
        {
            cam.rect = new Rect(x, y, w, h);
        }
    }
}
