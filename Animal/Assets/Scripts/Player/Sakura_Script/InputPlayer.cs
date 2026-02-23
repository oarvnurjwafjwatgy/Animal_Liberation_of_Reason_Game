using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Controller))]



public class InputPlayer : MonoBehaviour
{
    [Header("エフェクトの位置調整")]
    [SerializeField] private Vector3 effectOffset = new Vector3(0f, 0.2f, 1.0f);

    [Header("攻撃の設定")]
    public float attackRange = 2.0f;   // 攻撃が届く距離
    public float attackOffset = 1.0f;  // 攻撃判定を出す位置（自分の中心からどれくらい前か）
    public LayerMask enemyLayer;       // インスペクターで「Player」レイヤーを選択

    public float moveSpeed = 5.0f; // キャラクターの移動速度
    private GameObject cameraObject;
    public GameObject normalObject;
    public GameObject reasonObject;
    public GameObject ghostObject;
    private Quaternion cachedRotate;
    private GameObject collisionObject;


    Character_Status character_Status;
    private Animator animator;

    private bool deathFlag;

    // 参照するコンポーネント
    private Rigidbody rb;
    private Controller controller; // 作成した Controller クラス

    [SerializeField] private GameObject Collision;

    private GameObject EffectManager;
    EffectManager Effect_Manager = null;

    bool MoveFlag = true; // 動かせるか

    bool LiveFlag = true; //生きているか

    Vector3 Position;
    Quaternion Quaternion;
    Vector3 Scale;

    public Vector3 add_pos;
    public Vector3 add_rot;
    public Vector3 add_scale;


    public enum Direction
    {
        Front,
        Right,
        Left,
        Back,
    }

    Direction direction = Direction.Front;


    // Start is called before the first frame update
    void Start()
    {
        EffectManager = GameObject.Find("EffectManager");
        Effect_Manager = EffectManager.GetComponent<EffectManager>();

        // cameraObject と ghostObject は土台プレハブに元からあるはずなので取得
        // ただし、既に SetupDynamicReferences で設定されている場合は何もしない
        if (cameraObject == null) cameraObject = transform.GetChild(0).gameObject;
        if (ghostObject == null) ghostObject = transform.GetChild(1).gameObject;

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
        controller.PlayerInput.actions["Skill"].started += OnSkill;
        controller.PlayerInput.actions["Evation"].started += OnEvation;
    }

    private void OnDisable()
    {
        controller.PlayerInput.actions["Attack"].started -= OnAttack;
        controller.PlayerInput.actions["ModeChange"].started -= OnModeChange;
        controller.PlayerInput.actions["CameraReset"].started -= OnCameraReset;
        controller.PlayerInput.actions["Skill"].started -= OnSkill;
        controller.PlayerInput.actions["Evation"].started -= OnEvation;
    }

    private void Update()
    {
        // 観戦者の上昇下降の処理
        this.GhostUpDown();



        if (character_Status.CurrentHP <= 0)
        {
            animator.SetInteger("State", 2);
            LiveFlag = false;

           

            //Invoke("SetDeath", 3.0f);

             SetDeath();
        }
    }



    // 物理演算は FixedUpdate で行います
    private void FixedUpdate()
    {

        // Controller クラスが正しく取得できているか確認
        if ((controller != null && rb != null) && MoveFlag == true && LiveFlag == true)
        {
            // 1. Controller クラスからスティックの入力値を取得
            Vector2 leftStickInput = controller.GetLeftStick();

            if (leftStickInput.x > 0.1)
            {
                direction = Direction.Right;
            }
            else if (leftStickInput.x < -0.1)
            {
                direction = Direction.Left;
            }
            else if (leftStickInput.y < -0.1)
            {
                direction = Direction.Back;
            }
            else
            {
                direction = Direction.Front;
            }


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

    // アニメーションの影響上、プレイヤーの向き更新は LateUpdate で行う
    private void LateUpdate()
    {
        // Controller クラスが正しく取得できているか確認
        if ((controller != null && rb != null) && MoveFlag == true && LiveFlag == true)
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

            if (leftStickInput.magnitude > 0.1f)
            {
                animator.SetInteger("State", 1);
            }
            else
            {
                animator.SetInteger("State", 0);
            }




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
        if(LiveFlag ==  true)
        { 

            MoveFlag = false;

            // 現在のモデル（通常か強化か）を取得
            GameObject activeModel = (character_Status.GetMode() == Character_Status.Mode.SPSIAL_ANIMAL) ? reasonObject : normalObject;

            // 【新機能】動物ごとの最適座標を計算して取得
            Vector3 effectPosition = GetEffectSpawnPosition(activeModel);

            switch (character_Status.CharaAnim)
            {
                case Character_Status.CharacterType.LION:
                    Position = new Vector3(effectPosition.x, effectPosition.y, effectPosition.z);
                    Quaternion = activeModel.transform.rotation * Quaternion.Euler(0f, 0f, 0f);
                    Scale = new Vector3(1f, 1f, 1f);

                    break;

                case Character_Status.CharacterType.OSTRICH:

                    // 1. モデルの「右・上・前」の方向ベクトルを取得
                    Vector3 right = activeModel.transform.right;
                    Vector3 up = activeModel.transform.up;
                    Vector3 forward = activeModel.transform.forward;

                    // 2. この形なら add_pos.x を変えると「常にキャラの右/左」に動きます！
                    Position = new Vector3(
                        effectPosition.x + (right.x * -0.5f) + (up.x *  0) + (forward.x * 0),
                        effectPosition.y + (right.y * -0.5f) + (up.y * 0) + (forward.y * 0),
                        effectPosition.z + (right.z * -0.5f) + (up.z * 0) + (forward.z * 0)
                    );

                    Quaternion = activeModel.transform.rotation * Quaternion.Euler(20f, 0, 0);
                    Scale = new Vector3(0.3f, 0.3f, 0.3f);

                    break;

                    //Vector3 offset = new Vector3(other.gameObject.transform.position.x - 0.7f, other.gameObject.transform.position.y, other.gameObject.transform.position.z - 1.0f);


                case Character_Status.CharacterType.RHINOCELOS:

                    // 1. モデルの「右・上・前」の方向ベクトルを取得
                    Vector3 RHright = activeModel.transform.right;
                    Vector3 RHup = activeModel.transform.up;
                    Vector3 RHforward = activeModel.transform.forward;

                    // 2. この形なら add_pos.x を変えると「常にキャラの右/左」に動きます！
                    Position = new Vector3(
                        effectPosition.x + (RHright.x * 0) + (RHup.x * 0) + (RHforward.x * 0),
                        effectPosition.y + (RHright.y * 0) + (RHup.y * 0) + (RHforward.y * 0),
                        effectPosition.z + (RHright.z * 0) + (RHup.z * 0) + (RHforward.z * 0)
                    );

                    Quaternion = activeModel.transform.rotation * Quaternion.Euler(0, 0, 0);
                    Scale = new Vector3(0.3f, 0.3f, 0.3f);

                    break;

                case Character_Status.CharacterType.RATEL:
                    // 1. モデルの「右・上・前」の方向ベクトルを取得
                    Vector3 RAright = activeModel.transform.right;
                    Vector3 RAup = activeModel.transform.up;
                    Vector3 RAforward = activeModel.transform.forward;

                    // 2. この形なら add_pos.x を変えると「常にキャラの右/左」に動きます！
                    Position = new Vector3(
                        effectPosition.x + (RAright.x * add_pos.x) + (RAup.x * add_pos.y) + (RAforward.x * add_pos.z),
                        effectPosition.y + (RAright.y * add_pos.x) + (RAup.y * add_pos.y) + (RAforward.y * add_pos.z),
                        effectPosition.z + (RAright.z * add_pos.x) + (RAup.z * add_pos.y) + (RAforward.z * add_pos.z)
                    );

                    Quaternion = activeModel.transform.rotation * Quaternion.Euler(add_rot.x, add_rot.y, add_rot.z);
                    Scale = new Vector3(add_scale.x, add_scale.y, add_scale.z);

                    break;


            }

            // エフェクト再生
            Effect_Manager.PlayEffect(normalObject.name, 0, Position, Quaternion, Scale,this.transform);

            // 当たり判定生成
            AttackCollider();

            // アニメーション処理（既存のまま）
            animator.SetTrigger("Attack");
            if (deathFlag) return;
        }
    }

    private void OnModeChange(InputAction.CallbackContext context)
    {
        if (LiveFlag == true)
        {

            Effect_Manager.PlayEffect("Common", 0, this.transform.position, this.transform.rotation, new Vector3(2.0f, 2.0f, 2.0f), this.transform);
            character_Status.GetModeChange();
            Enhancement();

            Debug.Log("チェンジ");
        }

    }

    private void OnCameraReset(InputAction.CallbackContext context)
    {
        cameraObject.transform.position = normalObject.transform.position + new Vector3(0f, 1f, 0f) + normalObject.transform.forward * -3f;
        cameraObject.transform.rotation = normalObject.transform.rotation;

        Debug.Log("カメラリセット");
    }

    private void OnSkill(InputAction.CallbackContext context)
    {
        if (LiveFlag == true)
        {
            MoveFlag = false;

            // 現在のモデル（通常か強化か）を取得
            GameObject activeModel = (character_Status.GetMode() == Character_Status.Mode.SPSIAL_ANIMAL) ? reasonObject : normalObject;

            // 【新機能】動物ごとの最適座標を計算して取得
            Vector3 effectPosition = GetEffectSpawnPosition(activeModel);

            animator.SetTrigger("Skill");

            // エフェクト再生
            Effect_Manager.PlayEffect(normalObject.name, 1, effectPosition, activeModel.transform.rotation, new Vector3(1f, 1f, 1f));


            Debug.Log("スキル発動");
        }

           
    }

    private void OnEvation(InputAction.CallbackContext context)
    {
        if(LiveFlag == true)
        {
            switch (direction)
            {
                case Direction.Right: animator.SetTrigger("RightStep"); break;
                case Direction.Left: animator.SetTrigger("LeftStep"); break;
                case Direction.Back: animator.SetTrigger("BackStep"); break;
            }



            Debug.Log("回避");
        }

       
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
       if(deathFlag == false)
        {
            //// 死亡フラグをtrueにする
            //deathFlag = true;

            //// 観戦者用に各アクティブ状態を変更する
            //cameraObject.SetActive(false);
            //normalObject.SetActive(false);
            //reasonObject.SetActive(false);
            //ghostObject.SetActive(true);

            //// 重力を無効にする
            //rb.useGravity = false;

            //// 自身と子オブジェクトのレイヤーをGhostにする
            //ChangeLayer change_layer = this.GetComponent<ChangeLayer>();
            //change_layer.SetLayer();

            Effect_Manager.PlayEffect("Common", 4, this.gameObject.transform.position, this.gameObject.transform.rotation, new Vector3(1f, 1f, 1f));

            deathFlag = true;
            MoveFlag = false;
        }
    }



    // アニメーションで攻撃の当たり判定を出す
    public Vector3 AttackCollider()
    {
        // 1. 現在アクティブなモデル（通常時か強化時か）を取得する
        GameObject activeModel = (character_Status.GetMode() == Character_Status.Mode.SPSIAL_ANIMAL) ? reasonObject : normalObject;

        // 2. 出現位置の計算
        // activeModel.transform.forward : モデルが向いている正面方向
        // attackOffset : インスペクターで設定できる「前方にどれくらい離すか」の距離
        Vector3 spawnPosition = activeModel.transform.position + new Vector3(0f, 0.5f, 0f) + activeModel.transform.forward * attackOffset;

        // 3. 当たり判定の生成
        // Quaternion.identity ではなく activeModel.transform.rotation を渡すことで、向きを合わせます
        collisionObject = Instantiate(Collision, spawnPosition, activeModel.transform.rotation, this.gameObject.transform);

        return collisionObject.transform.position;
    }

    public void ColliderDelete()
    {
        Destroy(collisionObject);
    }



    public void SetupDynamicReferences(GameObject normal, GameObject reason)
    {
        this.normalObject = normal;
        this.reasonObject = reason;

        // 1. まずは「Animator」コンポーネントそのものを取得する
        this.animator = normal.GetComponentInChildren<Animator>();

        if (this.animator != null)
        {
            // 2. もしプレハブのAnimatorにあらかじめOverrideControllerが設定されていれば、
            // それが自動的に runtimeAnimatorController として扱われます。
            // なので、ここで特別な代入をしなくても、animator.SetTrigger("Attack") は動きます。
            Debug.Log($"{normal.name} の Animator を紐付けました。");
        }
        else
        {
            Debug.LogWarning($"{normal.name} に Animator が見つかりません。");
        }

        this.cachedRotate = normal.transform.rotation;
    }



    public void Enhancement()
    {
        Character_Status.Mode currentMode = character_Status.GetMode();

        Debug.Log(currentMode);

        if (currentMode == Character_Status.Mode.SPSIAL_ANIMAL)
        {
            // --- 通常 → 強化（理性モード）への切り替え ---
            normalObject.SetActive(false);
            reasonObject.SetActive(true);

            // Animator を強化モデルのものに差し替える
            animator = reasonObject.GetComponentInChildren<Animator>();
            Debug.Log("強化モデルに切り替わりました");
        }
        else
        {
            // --- 強化 → 通常への切り替え ---
            reasonObject.SetActive(false);
            normalObject.SetActive(true);

            // Animator を通常モデルのものに差し替える
            animator = normalObject.GetComponentInChildren<Animator>();
            Debug.Log("通常モデルに戻りました");
        }
    }



    public void MoveFlagFalse()
    {
        MoveFlag = true;
    }

    public void DeleteCollision()
    {
        Destroy(collisionObject);
    }



    /// <summary>
    /// 現在の動物名に合わせて、エフェクトの発生位置を計算する
    /// </summary>
    private Vector3 GetEffectSpawnPosition(GameObject activeModel)
    {
        // 基本のオフセット（インスペクターで設定した値）
        Vector3 baseOffset = effectOffset;

        // 動物ごとに微調整が必要な場合、ここで baseOffset を上書き・加算する
        // normalObject.name には "(Clone)" がついている場合があるため Contains で判定

        string animalName = normalObject.name;

        if (animalName.Contains("Lion(Clone)"))
        {
            // ライオン用の微調整（例：もう少し低く、もう少し前になど）
            // baseOffset += new Vector3(0f, -0.1f, 0.5f);
        }
        else if (animalName.Contains("Ostrich(Clone)"))
        {
            // ダチョウ用の微調整
             baseOffset += new Vector3(0f, 0.2f, 0f);
        }
        else if (animalName.Contains("Rhinoceros(Clone)"))
        {
            // サイ用の微調整（体が大きいのでもっと前になど）
            // baseOffset += new Vector3(0f, 0f, 1.0f);
        }
        else if (animalName.Contains("Ratel(Clone)"))
        {
            // ラーテル用の微調整（小さいのでもっと低くなど）
            // baseOffset += new Vector3(0f, -0.2f, 0f);
        }

        // 最終的な座標を計算して返す
        return activeModel.transform.position
               + activeModel.transform.forward * baseOffset.z
               + activeModel.transform.up * baseOffset.y
               + activeModel.transform.right * baseOffset.x;
    }
}

