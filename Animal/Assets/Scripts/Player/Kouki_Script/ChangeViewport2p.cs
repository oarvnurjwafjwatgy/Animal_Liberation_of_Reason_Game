using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeViewport2p : MonoBehaviour
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
        var pm_playerCount = playerManager.GetComponent<PlayerManager>().playerCount;

        this.SetViewport(pm_playerCount);
        //Debug.Log(pm_playerCount.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        if (cam != null)
        {
            cam.rect = new Rect(x, y, w, h);
        }
    }

    private void SetViewport(int player_count)
    {
        if (player_count <= 2)
        {
            x = 0.5f;
            y = 0f;
            w = 0.5f;
            h = 1f;
        }
        else
        {
            x = 0.5f;
            y = 0.5f;
            w = 0.5f;
            h = 0.5f;
        }
    }
}
