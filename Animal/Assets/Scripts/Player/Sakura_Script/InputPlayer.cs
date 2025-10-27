using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Controller))]
public class InputPlayer : MonoBehaviour
{
    public float moveSpeed = 5.0f; // キャラクターの移動速度
    public GameObject cameraObject;

    // 参照するコンポーネント
    private Rigidbody rb;
    private Controller controller; // 作成した Controller クラス

    // Start is called before the first frame update
    void Start()
    {
        cameraObject = transform.GetChild(0).gameObject;
        //cameraObject = GameObject.Find("Camera");
    }

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

            Vector3 cameraForward = Vector3.Scale(cameraObject.transform.forward, new Vector3(1,0,1)).normalized;
            Vector3 moveForward = cameraForward * leftStickInput.y + cameraObject.transform.right * leftStickInput.x;
            rb.velocity = moveForward * moveSpeed + new Vector3(0, rb.velocity.y, 0);

            // 3. Rigidbody の速度 (velocity) を変更して移動させる
            /*rb.velocity = new Vector3(
                moveDirection.x * moveSpeed,
                rb.velocity.y, // Y軸は変更しない
                moveDirection.z * moveSpeed
            );*/

            this.UpdateCamera();
        }
    }

    private void UpdateCamera()
    {
        // ControllerクラスからRスティックの入力値を取得
        Vector2 rightStickInput = controller.GetRightStick();

        // カメラの横移動
        if (rightStickInput.x > 0.25f || rightStickInput.x < -0.25f)
        {
            cameraObject.transform.RotateAround(this.transform.position, Vector3.up, rightStickInput.x * Time.deltaTime * 200f);
        }
        // カメラの縦移動
        float camera_angle_x = cameraObject.transform.localEulerAngles.x;
        Debug.Log(camera_angle_x);
        if (rightStickInput.y > 0.25f && (camera_angle_x < 80f || camera_angle_x <= 360f && camera_angle_x > 180f))
        {
            // 上移動
            cameraObject.transform.RotateAround(this.transform.position, cameraObject.transform.right, rightStickInput.y * Time.deltaTime * 200f);
        }
        if (rightStickInput.y < -0.25f && (camera_angle_x > 280f || camera_angle_x >= 0f && camera_angle_x < 180f))
        {
            // 下移動
            cameraObject.transform.RotateAround(this.transform.position, cameraObject.transform.right, rightStickInput.y * Time.deltaTime * 200f);
        }
    }
}