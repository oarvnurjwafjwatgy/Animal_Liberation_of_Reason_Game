using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nuts : MonoBehaviour
{
    private float deleteTimer;

    // Start is called before the first frame update
    void Start()
    {
        deleteTimer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        // 消滅タイマーの更新
        this.UpdateDeleteTimer();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            //// 効果をここに… ////

            // ItemManagerの木の実の数を減らす
            GameObject.Find("ItemManager").GetComponent<ItemManager>().DecreaseNutsCount(1);
            // オブジェクトの削除
            Destroy(this.gameObject);
        }
    }

    // 消滅タイマーの更新
    private void UpdateDeleteTimer()
    {
        // タイマー計測
        deleteTimer += Time.deltaTime;

        // 10秒経過したら自動で削除する
        if (deleteTimer > 10f)
        {
            // ItemManagerの木の実の数を減らす
            GameObject.Find("ItemManager").GetComponent<ItemManager>().DecreaseNutsCount(1);
            // オブジェクトの削除
            Destroy(this.gameObject);
        }
    }

}
