using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> itemPrefab = new List<GameObject>();  // アイテムプレハブのリスト
    [SerializeField] private int maxNutsCount;          // フィールド上に生成できる木の実の最大個数
    [SerializeField] private int maxNutsCreateTime;     // 生成する木の実の間隔の最大値
    [SerializeField] private int minNutsCreateTime;     // 生成する木の実の間隔の最小値
    [SerializeField] private float nutsCreateRadius;    // 木の実の生成する範囲(半径)

    public int nutsCount;          // 現在フィールド上にある木の実の数
    private int nutsCreateTime;     // 木の実の生成までの時間
    private float nutsCreateTimer;  // 木の実の生成タイマー

    // アイテムのID
    public enum ITEM_ID
    { 
        NUTS,       // 木の実

        DUMMY,
    }

    // Start is called before the first frame update
    void Start()
    {
        // 変数初期化
        nutsCount = 0;
        nutsCreateTimer = 0f;

        // 木の実の最初の生成までの時間を決める
        nutsCreateTime = Random.Range(minNutsCreateTime,maxNutsCreateTime + 1);
    }

    // Update is called once per frame
    void Update()
    {
        // 木の実生成タイマーの更新
        UpdateNutsTimer();
    }

    /*
     *  アイテムの生成
     *  
     *  item_id アイテムのID
     *  pos     生成位置
     */
    public void CreateItem(ITEM_ID item_id, Vector3 pos)
    {

        switch (item_id)
        {
            case ITEM_ID.NUTS:

                Instantiate(itemPrefab[(int)item_id], pos, Quaternion.identity);
                nutsCount++;
                break;
        }
    }

    /*
     *  木の実生成タイマーの更新
     */
    private void UpdateNutsTimer()
    {
        // 時間計測
        nutsCreateTimer += Time.deltaTime;

        // 指定時間を超えたら生成する
        if ((float)nutsCreateTimer > nutsCreateTime)
        {
            this.CreateNuts();

            // タイマーを0に戻す
            nutsCreateTimer = 0f;
            // 次の生成までの時間を決める
            nutsCreateTime = Random.Range(minNutsCreateTime, maxNutsCreateTime + 1);
        }
    }

    /*
     *  木の実の生成
     */
    private void CreateNuts()
    {
        // 上限まで出ている場合は生成しない
        if (nutsCount >= maxNutsCount)
        {
            Debug.Log("<color=#ACBF73>上限のため、木の実が生成できませんでした</color>");
            return;
        }

        // 生成する位置をランダムに決定する
        float pos_x = Random.Range(-nutsCreateRadius, nutsCreateRadius);
        float pos_z = Random.Range(-nutsCreateRadius, nutsCreateRadius);
        Vector3 create_pos = new Vector3(pos_x, 3f, pos_z);

        // 木の実を生成する
        this.CreateItem(ITEM_ID.NUTS, create_pos);

        Debug.Log("<color=#E5FF99>木の実の生成</color>");
    }

    public void DecreaseNutsCount(int count)
    {
        nutsCount -= count;
    }

}
