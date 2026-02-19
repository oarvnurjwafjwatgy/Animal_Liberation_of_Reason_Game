using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nuts : MonoBehaviour
{
    private float deleteTimer;

    // Start is called before the first frame update
    void Start()
    {
        // タイマーの初期化
        deleteTimer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        // 削除タイマーの更新
        this.UpdateDeleteTimer();
    }

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーに当たったら、効果を発動
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            //// ここに効果 ////

            // ItemManagerの生成した木の実の数を減らす
            GameObject.Find("ItemManager").GetComponent<ItemManager>().DecreaseNutsCount(1);
            // 自身を削除する
            Destroy(this.gameObject);
        }
    }

    // 削除タイマーの更新
    private void UpdateDeleteTimer()
    {
        // タイマーの加算
        deleteTimer += Time.deltaTime;

        // 10秒経過したら、自動で消える
        if (deleteTimer > 10f)
        {
            // ItemManagerの生成した木の実の数を減らす
            GameObject.Find("ItemManager").GetComponent<ItemManager>().DecreaseNutsCount(1);
            // 自身を削除する
            Destroy(this.gameObject);
        }
    }
}
