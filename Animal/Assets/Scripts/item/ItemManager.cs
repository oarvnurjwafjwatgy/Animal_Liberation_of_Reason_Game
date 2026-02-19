using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> itemPrefab = new List<GameObject>();  // アイテムのリスト
    [SerializeField] private int maxNutsCount;          // 一度に存在できる木の実の最大数
    [SerializeField] private int maxNutsCreateTime;     // 木の実生成までの最長時間
    [SerializeField] private int minNutsCreateTime;     // 木の実生成までの最短時間
    [SerializeField] private float nutsCreateRadius;    // 木の実を生成する範囲の半径

    private int nutsCount;          // フィールド上にある木の実の数
    private int nutsCreateTime;     // 木の実を生成するまでの時間
    private float nutsCreateTimer;  // 木の実生成タイマー


    // アイテムID
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

        // 木の実の生成までの時間をランダムで求める
        nutsCreateTime = Random.Range(minNutsCreateTime, maxNutsCreateTime + 1);
    }

    // Update is called once per frame
    void Update()
    {
        // 木の実生成タイマーの更新
        this.UpdateNutsTimer();
    }
    
    
    // アイテムの生成
    // item_id  生成するアイテムのID
    // pos      生成位置
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

    // 木の実生成タイマーの更新
    private void UpdateNutsTimer()
    {
        // タイマーの加算
        nutsCreateTimer += Time.deltaTime;

        // タイマーが既定の時間を超えたら、木の実を生成する
        if ((float)nutsCreateTimer > nutsCreateTime)
        {
            this.CreateNuts();

            // タイマーの初期化
            nutsCreateTimer = 0f;
            // 次の生成までの時間をランダムで求める
            nutsCreateTime = Random.Range(minNutsCreateTime, maxNutsCreateTime + 1);
        }
    }

    // 木の実の生成
    private void CreateNuts()
    {
        // 木の実が既定数出ている場合は生成しない
        if (nutsCount >= maxNutsCount)
        {
            Debug.Log("<color=#ACBF73>生成数が上限を超えるため、生成しません</color>");
            return;
        }

        // 既定の範囲内でランダムな位置を決める
        float pos_x = Random.Range(-nutsCreateRadius, nutsCreateRadius);
        float pos_z = Random.Range(-nutsCreateRadius, nutsCreateRadius);
        Vector3 create_pos = new Vector3(pos_x, 3f, pos_z);

        // 木の実の生成
        this.CreateItem(ITEM_ID.NUTS, create_pos);

        Debug.Log("<color=#E5FF99>木の実を生成</color>");
    }

    // フィールド上にある木の実のカウントを減らす
    public void DecreaseNutsCount(int count)
    {
        nutsCount -= count;
    }
}
