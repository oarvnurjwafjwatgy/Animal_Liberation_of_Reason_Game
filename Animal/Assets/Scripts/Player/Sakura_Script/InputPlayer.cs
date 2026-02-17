using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Controller))]
public class InputPlayer : MonoBehaviour
{
    [Header("攻撃の設定")]
    public float attackRange = 2.0f;   // 攻撃が届く距離
    public float attackOffset = 1.0f;  // 攻撃判定を出す位置（自分の中心からどれくらい前か）
    public LayerMask enemyLayer;       // インスペクターで「Player」レイヤーを選択

    public float moveSpeed = 5.0f; // キャラクターの移動速度
    private GameObject cameraObject;
    private GameObject normalObject;
    private GameObject reasonObject;
    private GameObject ghostObject;
    private Quaternion cachedRotate;
    private GameObject collisionObject;


    Character_Status character_Status;
    private Animator animator;

    private bool deathFlag;

    // 参照するコンポーネント
    private Rigidbody rb;
    private Controller controller; // 作成した Controller クラス

    [SerializeField] private GameObject Collision;








    // Start is called before the first frame update
    void Start()
    {
       // cameraObject と ghostObject は土台プレハブに元からあるはずなので取得
    // ただし、既に SetupDynamicReferences で設定されている場合は何もしない
    if (cameraObject == null) cameraObject = transform.GetChild(0).gameObject;
        if (ghostObject == null) ghostObject = transform.GetChild(3).gameObject;

        // normalObject, reasonObject は PlayerManager から渡されるので
        // ここで transform.GetChild で上書きしてはいけない！！（コメントアウト推奨）
        // normalObject = transform.GetChild(1).gameObject; 

        character_Status = GetComponent<Character_Status>();

        // Animatorも渡されているはずなので、nullの場合のみ取得
        if (animator == null) animator = GetComponent<Animator>();

        deathFlag = false;
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
        controller.PlayerInput.actions["CameraReset"].started += OnCameraReset;
    }

    private void OnDisable()
    {
        controller.PlayerInput.actions["Attack"].started -= OnAttack;
        controller.PlayerInput.actions["ModeChange"].started -= OnModeChange;
        controller.PlayerInput.actions["CameraReset"].started -= OnCameraReset;
    }

    private void Update()
    {
        // 観戦者の上昇下降の処理
        this.GhostUpDown();
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

                GameObject camera = cameraObject;
                if (deathFlag)
                    camera = ghostObject;

                // 3. Rigidbody の速度 (velocity) を変更して移動させる
                Vector3 cameraForward = Vector3.Scale(camera.transform.forward, new Vector3(1, 0, 1)).normalized;
                Vector3 moveForward = cameraForward * leftStickInput.y + camera.transform.right * leftStickInput.x;
                rb.velocity = moveForward * moveSpeed + new Vector3(0, rb.velocity.y, 0);


                // カメラの位置の更新
                if (!deathFlag)
                    this.UpdateCamera();
                else
                    this.UpdateGhostCamera();
            }
        }
    }

    // アニメーションの影響上、プレイヤーの向き更新は LateUpdate で行う
    private void LateUpdate()
    {
        // Controller クラスが正しく取得できているか確認
        if (controller != null && rb != null)
        {
            // Controller クラスからスティックの入力値を取得
            Vector2 leftStickInput = controller.GetLeftStick();

            // 入力値 (Vector2) を 3D の移動方向 (Vector3) に変換
            Vector3 moveDirection = new Vector3(leftStickInput.x, 0, leftStickInput.y);

            GameObject camera = cameraObject;
            if (deathFlag)
                camera = ghostObject;

            // Rigidbody の速度 (velocity) を変更して移動させる
            Vector3 cameraForward = Vector3.Scale(camera.transform.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 moveForward = cameraForward * leftStickInput.y + camera.transform.right * leftStickInput.x;

            // Lスティックが入力されている時は、向きを正面にしその向きを保存する
            if (moveForward != new Vector3(0f, 0f, 0f))
            {
                Quaternion tmp = Quaternion.LookRotation(moveForward);

                normalObject.transform.rotation = tmp;
                reasonObject.transform.rotation = tmp;
                cachedRotate = tmp;
            }
            // 未入力の時は、保存した向きを呼び出し続ける
            else
            {
                normalObject.transform.rotation = cachedRotate;
                reasonObject.transform.rotation = cachedRotate;
            }
        }
    }

    // 通常時のカメラ更新
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
        if (rightStickInput.y > 0.25f && (camera_angle_x > 280f || camera_angle_x >= 0f && camera_angle_x < 180f))
        {
            // 下移動
            cameraObject.transform.RotateAround(this.transform.position, cameraObject.transform.right, -rightStickInput.y * Time.deltaTime * 200f);
        }
        if (rightStickInput.y < -0.25f && (camera_angle_x < 80f || camera_angle_x <= 360f && camera_angle_x > 180f))
        {
            // 上移動
            cameraObject.transform.RotateAround(this.transform.position, cameraObject.transform.right, -rightStickInput.y * Time.deltaTime * 200f);
        }
    }

    // 観戦モード時のカメラ更新
    private void UpdateGhostCamera()
    {
        // ControllerクラスからRスティックの入力値を取得
        Vector2 rightStickInput = controller.GetRightStick();

        // カメラの横移動
        if (rightStickInput.x > 0.25f || rightStickInput.x < -0.25f)
        {
            ghostObject.transform.RotateAround(this.transform.position, Vector3.up, rightStickInput.x * Time.deltaTime * 200f);
        }
        // カメラの縦移動
        float camera_angle_x = ghostObject.transform.localEulerAngles.x;
        //Debug.Log(camera_angle_x);
        if (rightStickInput.y > 0.25f && (camera_angle_x > 280f || camera_angle_x >= 0f && camera_angle_x < 180f))
        {
            // 下移動
            ghostObject.transform.RotateAround(this.transform.position, ghostObject.transform.right, -rightStickInput.y * Time.deltaTime * 200f);
        }
        if (rightStickInput.y < -0.25f && (camera_angle_x < 80f || camera_angle_x <= 360f && camera_angle_x > 180f))
        {
            // 上移動
            ghostObject.transform.RotateAround(this.transform.position, ghostObject.transform.right, -rightStickInput.y * Time.deltaTime * 200f);
        }
    }

    private void GhostUpDown()
    {
        // スペクテイター時以外は処理しない
        if (!deathFlag) return;

        // 下降
        if (controller.PlayerInput.actions["Descent"].IsPressed())
            this.OnDescent();
        // 上昇
        else if (controller.PlayerInput.actions["Jump"].IsPressed())
            this.OnAscending();
        // 上昇も下降もさせない時は、velocity.yを0にする
        else
            this.RemoveUpDown();
    }


    private void OnAttack(InputAction.CallbackContext context)
    {
        Debug.Log("攻撃");
        if (deathFlag) return; // 死亡中は攻撃できない

        switch (character_Status.GetMode())
        {
            case Character_Status.Mode.ANIMAL:
                // 動物モードの攻撃処理
                Debug.Log("動物モードの攻撃");
                animator.SetTrigger("Attack");
                AttackCollider();
                break;

            case Character_Status.Mode.SPSIAL_ANIMAL:
                // スペシャルアニマルモードの攻撃処理
                Debug.Log("スペシャルアニマルモードの攻撃");
                animator.SetTrigger("Reason_Attack");
                AttackCollider();
                break;
        }
    }

    private void OnModeChange(InputAction.CallbackContext context)
    {
        character_Status.GetModeChange();

        Debug.Log("チェンジ");
    }

    private void OnCameraReset(InputAction.CallbackContext context)
    {
        cameraObject.transform.position = normalObject.transform.position + new Vector3(0f, 1f, 0f) + normalObject.transform.forward * -3f;
        cameraObject.transform.rotation = normalObject.transform.rotation;

        Debug.Log("カメラリセット");
    }

    private void OnDescent()
    {
        Debug.Log("下降");
        rb.velocity = new Vector3(rb.velocity.x, -5f, rb.velocity.z);
    }

    private void OnAscending()
    {
        Debug.Log("上昇");
        rb.velocity = new Vector3(rb.velocity.x, 5f, rb.velocity.z);
    }

    private void RemoveUpDown()
    {
        //Debug.Log("上下キャンセル");
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
    }


    public void SetDeath()
    {
        // 死亡フラグをtrueにする
        deathFlag = true;

        // 観戦者用に各アクティブ状態を変更する
        cameraObject.SetActive(false);
        normalObject.SetActive(false);
        reasonObject.SetActive(false);
        ghostObject.SetActive(true);

        // 重力を無効にする
        rb.useGravity = false;

        // 自身と子オブジェクトのレイヤーをGhostにする
        ChangeLayer change_layer = this.GetComponent<ChangeLayer>();
        change_layer.SetLayer();
    }

    private void PerformAttack()
    {
    }


    // アニメーションで攻撃の当たり判定を出す
    public void AttackCollider()
    {

        Vector3 spawnPosition = normalObject.transform.position  + new Vector3(0f,0.5f,0f) +normalObject.transform.right * 3f;

        collisionObject = Instantiate(Collision, spawnPosition, Quaternion.identity, this.gameObject.transform);
    }

    public void ColliderDelete()
    {
        Destroy(collisionObject);
    }


    // 攻撃与えたら
    private void OnTriggerEnter(Collider other)
    {
        GameObject collsionobj = other.gameObject;

        Character_Status damage = collsionobj.GetComponentInParent<Character_Status>();


        switch (collsionobj.tag)
        {
            case "Player1": Debug.Log("1Pダメージ");  damage.TakeDamage(50); ColliderDelete() ; break;
            case "Player2": Debug.Log("2Pダメージ"); damage.TakeDamage(50); ColliderDelete(); break;
            
            
        }

       

    }

    public void SetupDynamicReferences(GameObject normal, GameObject reason)
    {
        this.normalObject = normal;
        this.reasonObject = reason;

        // 生成された動物プレハブについているAnimatorを親にセット
        this.animator = normal.GetComponent<Animator>();

        // 向きの保存
        this.cachedRotate = normal.transform.rotation;

        Debug.Log($"Player {gameObject.name}: モデルの紐付け完了");
    }
}

