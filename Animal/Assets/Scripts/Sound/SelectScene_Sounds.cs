using UnityEngine;

public class SelectScene_Sounds : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        AudioManager.Instance.PlaySEByIndex(17,2); // 選択ナレーション
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
