using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapReducing : MonoBehaviour
{
    [SerializeField]private float mapRadius = 50f;  // マップ(正方形)の半径サイズ
    [SerializeField]private int phaseNum = 4;       // フェーズの数

    private float[] velocity = new float[4];        // 各壁の速度
    private float reducingTimer;    // 縮小タイマー
    private bool reducingFlag;      // 縮小フラグ
    private Vector3 targetPos;      // 収縮最終地点の座標
    [SerializeField] private float finalRadiusRange;  // 収縮最終地点の設定の範囲の半径
    [SerializeField] private GameObject targetObject; // 収縮最終地点オブジェクト
    [SerializeField] private GameObject northWall;    // 北の壁（+z）
    [SerializeField] private GameObject southWall;    // 南の壁（-z）
    [SerializeField] private GameObject eastWall;     // 東の壁（+x）
    [SerializeField] private GameObject westWall;     // 西の壁（+x）
    [SerializeField] private GameObject nePillar;     // 北東の壁
    [SerializeField] private GameObject nwPillar;     // 北西の壁
    [SerializeField] private GameObject sePillar;     // 南東の壁
    [SerializeField] private GameObject swPillar;     // 南西の壁
    int test = 0;                   // テスト用変数

    // Start is called before the first frame update
    void Start()
    {
        reducingTimer = 0;
        reducingFlag = false;
        // 既定半径内で最終収縮地点をランダムに決める
        targetObject.transform.position = new Vector3 (Random.Range(-finalRadiusRange, finalRadiusRange), 0f, Random.Range(-finalRadiusRange, finalRadiusRange));
        targetPos = targetObject.transform.position;

        // 各壁の速度の設定（2f:速度とスケールで半分、30f:範囲移動時間の30秒）
        velocity[0] = (((mapRadius - targetPos.z) / (float)phaseNum) / 2f) / 30f;
        velocity[1] = (((mapRadius + targetPos.z) / (float)phaseNum) / 2f) / 30f;
        velocity[2] = (((mapRadius - targetPos.x) / (float)phaseNum) / 2f) / 30f;
        velocity[3] = (((mapRadius + targetPos.x) / (float)phaseNum) / 2f) / 30f;

        // 壁の位置の初期化
        northWall.transform.position = new Vector3(0f, 0f, mapRadius);
        southWall.transform.position = new Vector3(0f, 0f, -mapRadius);
        eastWall.transform.position = new Vector3(mapRadius, 0f, 0f);
        westWall.transform.position = new Vector3(-mapRadius, 0f, 0f);

        // 柱の位置の初期化
        nePillar.transform.position = new Vector3(mapRadius, 0f, mapRadius);
        nwPillar.transform.position = new Vector3(-mapRadius, 0f, mapRadius);
        sePillar.transform.position = new Vector3(mapRadius, 0f, -mapRadius);
        swPillar.transform.position = new Vector3(-mapRadius, 0f, -mapRadius);

    }

    // Update is called once per frame
    void Update()
    {
        // 時間計測
        reducingTimer += Time.deltaTime;
        if ((int)reducingTimer != test) Debug.Log("time:" + test);
        test = (int)reducingTimer;

        this.SwitchingFlag();
        this.MoveWall();
    }

    private void SwitchingFlag()
    {
        // 4分以上経過している場合は処理しない（1フェーズ1分のため）
        if ((int)reducingTimer > phaseNum * 60) return;

        // 60秒経過したら縮小をストップ
        if (reducingFlag && (int)reducingTimer % 60 == 0)
        {
            reducingFlag = false;
            Debug.Log("縮小を停止");
        }
        // 30秒経過したら縮小をスタート
        if (!reducingFlag && (int)reducingTimer % 60 != 0 && (int)reducingTimer % 30 == 0)
        {
            reducingFlag = true;
            Debug.Log("縮小を開始");
        }
    }

    private void MoveWall()
    {
        // 縮小フラグがfalseの場合は処理しない
        if (!reducingFlag) return;

        // deltaTimeによる速度の算出
        float[] new_velocity = new float[4];
        for (int i = 0; i < 4; i++)
            new_velocity[i] = velocity[i] * Time.deltaTime;

        // 移動
        northWall.transform.position = new Vector3(0f, 0f, northWall.transform.position.z - new_velocity[0]);
        southWall.transform.position = new Vector3(0f, 0f, southWall.transform.position.z + new_velocity[1]);
        eastWall.transform.position = new Vector3(eastWall.transform.position.x - new_velocity[2], 0f, 0f);
        westWall.transform.position = new Vector3(westWall.transform.position.x + new_velocity[3], 0f, 0f);
        // 拡大（補正のため、速度に2をかける）
        northWall.transform.localScale = new Vector3(northWall.transform.localScale.x, northWall.transform.localScale.y, northWall.transform.localScale.z + new_velocity[0] * 2f);
        southWall.transform.localScale = new Vector3(southWall.transform.localScale.x, southWall.transform.localScale.y, southWall.transform.localScale.z + new_velocity[1] * 2f);
        eastWall.transform.localScale = new Vector3(eastWall.transform.localScale.x + new_velocity[2] * 2f, eastWall.transform.localScale.y, eastWall.transform.localScale.z);
        westWall.transform.localScale = new Vector3(westWall.transform.localScale.x + new_velocity[3] * 2f, westWall.transform.localScale.y, westWall.transform.localScale.z);
        // 柱の移動
        nePillar.transform.position = new Vector3(nePillar.transform.position.x - new_velocity[2] * 2f, 0f, nePillar.transform.position.z - new_velocity[0] * 2f);
        nwPillar.transform.position = new Vector3(nwPillar.transform.position.x + new_velocity[3] * 2f, 0f, nwPillar.transform.position.z - new_velocity[0] * 2f);
        sePillar.transform.position = new Vector3(sePillar.transform.position.x - new_velocity[2] * 2f, 0f, sePillar.transform.position.z + new_velocity[1] * 2f);
        swPillar.transform.position = new Vector3(swPillar.transform.position.x + new_velocity[3] * 2f, 0f, swPillar.transform.position.z + new_velocity[1] * 2f);

    }
}
