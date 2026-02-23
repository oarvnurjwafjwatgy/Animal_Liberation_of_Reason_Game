using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToTitle : MonoBehaviour
{
    // タイトルシーンへ(ボタンから呼ばれる関数)
    public void ToTitleScene()
    {
        // タイトルシーンへ遷移
        SceneManager.LoadScene("TitleScene");
    }
}
