using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Controller))]
public class InputPlayer : MonoBehaviour
{
    public float moveSpeed = 5.0f; // キャラクターの移動速度
    public GameObject cameraObject;
    public GameObject normalObject;
    public GameObject reasonObject;


    Character_Status character_Status;
    private Animator animator;

    // 参照するコンポーネント
    private Rigidbody rb;
    private Controller controller; // 作成した Controller クラス

    // Start is called before the first frame update
    void Start()
    {
        cameraObject = transform.GetChild(0).gameObject;
        normalObject = transform.GetChild(1).gameObject;
        reasonObject = transform.GetChild(2).gameObject;

        GameObject My = this.gameObject;
        character_Status = My.GetComponent<Character_Status>();
        //cameraObject = GameObject.Find("Camera");
        animator = GetComponent<Animator>();
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

    private void OnEnable()
    {
        controller.PlayerInput.actions["Attack"].started += OnAttack;
        controller.PlayerInput.actions["ModeChange"].started += OnModeChange;
    }

    private void OnDisable()
    {
        controller.PlayerInput.actions["Attack"].started -= OnAttack;
        controller.PlayerInput.actions["ModeChange"].started -= OnModeChange;
    }

    private void Update()
    {
    }



    // 物理演算は FixedUpdate で行います
    private void FixedUpdate()
    {
        // Controller クラスが正しく取得できているか確認
        if (controller != null && rb != null)
        {
            // Controller クラスが正しく取得できているか確認
            if (controller != null && rb != null)
            {
                // 1. Controller クラスからスティックの入力値を取得
                Vector2 leftStickInput = controller.GetLeftStick();

                // 2. 入力値 (Vector2) を 3D の移動方向 (Vector3) に変換
                Vector3 moveDirection = new Vector3(leftStickInput.x, 0, leftStickInput.y);

                // 3. Rigidbody の速度 (velocity) を変更して移動させる
                Vector3 cameraForward = Vector3.Scale(cameraObject.transform.forward, new Vector3(1, 0, 1)).normalized;
                Vector3 moveForward = cameraForward * leftStickInput.y + cameraObject.transform.right * leftStickInput.x;
                rb.velocity = moveForward * moveSpeed + new Vector3(0, rb.velocity.y, 0);

                if (moveForward != new Vector3(0f, 0f, 0f))
                {
                    normalObject.transform.rotation = Quaternion.LookRotation(moveForward);
                    reasonObject.transform.rotation = Quaternion.LookRotation(moveForward);
                }

                this.UpdateCamera();
            }
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
        //Debug.Log(camera_angle_x);
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


    private void OnAttack(InputAction.CallbackContext context)
    {
        Debug.Log("攻撃");

        switch (character_Status.GetMode())
        {
            case Character_Status.Mode.ANIMAL:
                // 動物モードの攻撃処理
                Debug.Log("動物モードの攻撃");
                animator.SetTrigger("Attack");
                break;

            case Character_Status.Mode.SPSIAL_ANIMAL:
                // スペシャルアニマルモードの攻撃処理
                Debug.Log("スペシャルアニマルモードの攻撃");
                animator.SetTrigger("Reason_Attack");
                break;
        }
    }

    private void OnModeChange(InputAction.CallbackContext context)
    {
        character_Status.GetModeChange();

        Debug.Log("チェンジ");
    }
}