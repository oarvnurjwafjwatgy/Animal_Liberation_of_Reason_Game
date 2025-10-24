using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// このスクリプトも Rigidbody と Controller が必要です
// (Controller が Rigidbody を要求しているので、RequireComponent はどちらか片方でも機能しますが、
//  PlayerMovement が直接 Rigidbody を使うため、こちらにも書くのが分かりやすいです)
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Controller))]

public class Player : MonoBehaviour
{
    public float moveSpeed = 5.0f; // キャラクターの移動速度

    // 参照するコンポーネント
    private Rigidbody rb;
    private Controller controller; // 作成した Controller クラス

    private void Awake()
    {
        // 同じゲームオブジェクトにアタッチされているコンポーネントを取得
        TryGetComponent(out rb);
        TryGetComponent(out controller);

        // もし controller が null の場合はエラーログを出しておくと安全です
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
            //    (leftStickInput.y は奥・手前なので Vector3 の z に対応)
            Vector3 moveDirection = new Vector3(leftStickInput.x, 0, leftStickInput.y);

            // 3. Rigidbody の速度 (velocity) を変更して移動させる
            //    (Y軸の速度は現在の重力などによる落下速度を維持)
            rb.velocity = new Vector3(
                moveDirection.x * moveSpeed,
                rb.velocity.y, // Y軸は変更しない
                moveDirection.z * moveSpeed
            );
        }
    }
}