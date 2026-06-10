using UnityEngine;

public class SelectScene_Sounds : MonoBehaviour
{
    // 初期
    void Start()
    {
        AudioManager.Instance.PlaySEByIndex(17, 2); // 選択ナレーション
    }
}
