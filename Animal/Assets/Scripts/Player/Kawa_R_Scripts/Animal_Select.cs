using System.Collections;
using UnityEngine;

public class Animal_Select : MonoBehaviour
{
	//ボタンの識別ID
	public int buttonID;
	//動物の名前
	public string animalName;

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnButtonClick()
    {
        Debug.Log("ボタン押された");

        switch (animalName)
        {
            case "LION":
                Debug.Log("ライオンが選択されました");
                break;
            case "OSTRICH":
                Debug.Log("ダチョウが選択されました");
                break;
            case "RHINOCELOS":
                Debug.Log("サイが選択されました");
                break;
            case "RATEL":
                Debug.Log("ラーテルが選択されました");
                break;
        }
	}
}
