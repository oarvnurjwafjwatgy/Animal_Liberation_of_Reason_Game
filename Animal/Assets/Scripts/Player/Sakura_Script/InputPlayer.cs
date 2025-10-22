using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Controller))]
public class InputPlayer : MonoBehaviour
{
    public float moveSpeed = 5.0f; // キャラクターの移動速度

    // ▼▼▼ 【ここから追加】 ▼▼▼
    [Header("コンポーネント参照")]
    [Tooltip("インスペクターで、このプレハブの子オブジェクトの Camera をドラッグ＆ドロップしてください")]
    [SerializeField]
    private Camera playerCameraComponent; // 自分のカメラをインスペクターから登録する
    // ▲▲▲ 【ここまで追加】 ▲▲▲

    // 参照するコンポーネント
    private Rigidbody rb;
    private Controller controller; // 作成した Controller クラス

    private void Awake()
    {
        // 同じゲームオブジェクトにアタッチされているコンポーネントを取得
        TryGetComponent(out rb);
        TryGetComponent(out controller);

        if (controller == null)
        {
            Debug.LogError("Controller コンポーネントが見つかりません！");
        }
    }

    // 物理演算は FixedUpdate で行います
    private void FixedUpdate()
    {
        // Controller クラスが正しく取得できているか確認
        if (controller != null && rb != null)
        {
            // 1. Controller クラスからスティックの入力値を取得
            Vector2 leftStickInput = controller.GetLeftStick();

            // 2. 入力値 (Vector2) を 3D の移動方向 (Vector3) に変換
            Vector3 moveDirection = new Vector3(leftStickInput.x, 0, leftStickInput.y);

            // 3. Rigidbody の速度 (velocity) を変更して移動させる
            rb.velocity = new Vector3(
                moveDirection.x * moveSpeed,
                rb.velocity.y, // Y軸は変更しない
                moveDirection.z * moveSpeed
            );
        }
    }

    // ▼▼▼ 【ここから追加】 ▼▼▼
    /// <summary>
    /// 【重要】登録されている自分のカメラを PlayerManager に渡す
    /// </summary>
    public Camera GetPlayerCamera()
    {
        return playerCameraComponent;
    }
    // ▲▲▲ 【ここまで追加】 ▲▲▲
}