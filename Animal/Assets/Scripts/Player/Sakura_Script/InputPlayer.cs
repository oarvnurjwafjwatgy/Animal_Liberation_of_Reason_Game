using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;

/*InputPlayer.cs全体担当者:古澤 桜
  SE関連担当者            :川上 流輝 */

// 動物ごとの設定をインスペクターで表示するためのクラス
[System.Serializable]
public struct AnimalAttackSettings
{
    public Character_Status.CharacterType type;
    public float attackcooldown; // 攻撃クールタイム（秒）
    public float skillCooldown;  // スキルクールタイム
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Controller))]

public class InputPlayer : MonoBehaviour
{
    [Header("クールタイム設定")]
    [SerializeField] private List<AnimalAttackSettings> animalSettings = new List<AnimalAttackSettings>();
    private Dictionary<Character_Status.CharacterType, float> 
    cooldownDict = new Dictionary<Character_Status.CharacterType, float>();
    
    private float lastAttackTime; // 最後に攻撃した時間
    private Vector3 effectOffset = new Vector3(0f, 0.2f, 1.0f);

    [Header("攻撃の設定")]
    public LayerMask enemyLayer;       // インスペクターで「Player」レイヤーを選択

    private GameObject cameraObject;
    public GameObject normalObject;
    public GameObject reasonObject;
    public GameObject ghostObject;
    private Quaternion cachedRotate;
    private GameObject collisionObject;

    private float outOfRangeDamageTimer;      // 範囲外のダメージのタイマー

    Character_Status character_Status;
    private Animator animator;

    private bool deathFlag;

    // 参照するコンポーネント
    private Rigidbody rb;
    private Controller controller; // 作成した Controller クラス

    private const float ghostMoveSpeed = 15f;

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

    GameObject SoundManagerObj;
    SoundManager soundmanager;

    private int ratelSkillStartHP; // スキル開始時のHPを保持

    public Vector3 skillDirection; // スキル発動時の向きを固定するための変数

    private bool isGameFinished = false; // 追加

    private float rhinocerosSkillStartTime = 0; // サイのスキル開始時間を記録

    private float ratelSkillStartTime = 0; // ラーテルの溜め開始時間を記録

    private float SkillStartTime = 0; // サイのスキル開始時間を記録

	private PlayerCameraController cameraController;

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

        SoundManagerObj = GameObject.Find("SoundManager");
        soundmanager = SoundManagerObj.GetComponent<SoundManager>();

        // cameraObject と ghostObject は土台プレハブに元からあるはずなので取得
        // ただし、既に SetupDynamicReferences で設定されている場合は何もしない
        if (cameraObject == null) cameraObject = transform.GetChild(0).gameObject;
        if (ghostObject == null) ghostObject = transform.GetChild(1).gameObject;
        cameraController.Initialize(cameraObject, ghostObject);

        character_Status = GetComponent<Character_Status>();

        // Animatorも渡されているはずなので、nullの場合のみ取得
        if (animator == null) animator = GetComponent<Animator>();

        deathFlag = false;
        outOfRangeDamageTimer = AnimalParam.TIMER_RESET;

        // 辞書の初期化（リストから検索しやすい辞書形式に変換）
        foreach (var setting in animalSettings)
        {
            if (!cooldownDict.ContainsKey(setting.type))
                cooldownDict.Add(setting.type, setting.attackcooldown);
        }
    }

    private void Awake()
    {
        // 同じゲームオブジェクトにアタッチされているコンポーネントを取得
        TryGetComponent(out rb);
        TryGetComponent(out controller);
        if (controller == null) Debug.LogError("Controller コンポーネントが見つかりません！");

        //新しいカメラコントローラーを取得：無ければ追加
        cameraController = GetComponent<PlayerCameraController>();
        if(cameraController==null) cameraController = gameObject.AddComponent<PlayerCameraController>();
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
        if (character_Status.CurrentHP <= 0 || character_Status.CurrentReason <= 0)
        {
            animator.SetInteger("State", 2);
            LiveFlag = false;
        }

        // ラーテルがスキル発動中(1)の時だけHPチェック
        if (character_Status.CharaAnim == Character_Status.CharacterType.RATEL &&
            animator.GetInteger("RatelSkill") == 1)
        {
            if (character_Status.CurrentHP < ratelSkillStartHP)
            {
                Debug.Log("ダメージを受けたのでラーテルのスキルを解除します");
                animator.SetInteger("RatelSkill", 2);
                MoveFlag = true;
            }
        }

        // 奈落落下チェック
        this.CheckAbyss();
    }

    // 物理演算は FixedUpdate で行います
    private void FixedUpdate()
    {
        if (deathFlag)
        {
            this.UpdateGhostMove();
            cameraController.UpdateGhostCamera(controller.GetRightStick(), transform,ref cachedRotate);return;
        }

        if (controller == null || rb == null || LiveFlag == false) return;

        // --- 【サイのスキル直進ロジック】 ---
        if (character_Status.CharaAnim == Character_Status.CharacterType.RHINOCELOS
        && animator.GetBool("RhinocerosSkill"))
        {
            // スティック入力に関係なく直進
            rb.velocity = skillDirection * character_Status.CurrentMoveSpeed + new Vector3(0, rb.velocity.y, 0);

            // スキル中でもカメラ操作（Rスティック）だけは可能にする
            cameraController.UpdateCamera(controller.GetRightStick(), transform); return;
        }

        // --- 通常の移動処理 ---
        if (MoveFlag)
        {
            Vector2 leftStickInput = controller.GetLeftStick();

            GameObject camera = deathFlag ? ghostObject : cameraObject;
            Vector3 cameraForward = Vector3.Scale(camera.transform.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 moveForward = cameraForward * leftStickInput.y + camera.transform.right * leftStickInput.x;

            rb.velocity = moveForward * character_Status.CurrentMoveSpeed + new Vector3(0, rb.velocity.y, 0);

            // 通常時のカメラ更新
            cameraController.UpdateCamera(controller.GetRightStick(), transform);
        }
    }

    // アニメーションの影響上、プレイヤーの向き更新は LateUpdate で行う
    private void LateUpdate()
    {
        if (controller == null || rb == null || LiveFlag == false || isGameFinished) return;

        // --- 【サイのスキル中：向きの強制固定】 ---
        if (character_Status.CharaAnim == Character_Status.CharacterType.RHINOCELOS && animator.GetBool("RhinocerosSkill"))
        {
            Quaternion skillRotation = Quaternion.LookRotation(skillDirection);
            normalObject.transform.rotation = skillRotation;
            reasonObject.transform.rotation = skillRotation;
            return; // スキル中はここで終了して、スティックによる回転計算をさせない
        }

        if (MoveFlag)
        {
            Vector2 leftStickInput = controller.GetLeftStick();
            GameObject camera = deathFlag ? ghostObject : cameraObject;
            Vector3 cameraForward = Vector3.Scale(camera.transform.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 moveForward = cameraForward * leftStickInput.y + camera.transform.right * leftStickInput.x;

            if (leftStickInput.magnitude > 0.1f) animator.SetInteger("State", 1);
            else animator.SetInteger("State", 0);

            if (moveForward != Vector3.zero)
            {
                Quaternion tmp = Quaternion.LookRotation(moveForward);
                normalObject.transform.rotation = tmp;
                reasonObject.transform.rotation = tmp;
                cachedRotate = tmp;
            }
            else
            {
                normalObject.transform.rotation = cachedRotate;
                reasonObject.transform.rotation = cachedRotate;
            }
        }

    }

    // 観戦モード時の移動
    private void UpdateGhostMove()
    {
        // Controller クラスが正しく取得できているか確認
        if (controller != null && rb != null)
        {
            // Controller クラスからスティックの入力値を取得
            Vector2 leftStickInput = controller.GetLeftStick();

            // 2. 入力値 (Vector2) を 3D の移動方向 (Vector3) に変換
            Vector3 moveDirection = new Vector3(leftStickInput.x, 0, leftStickInput.y);

            GameObject camera = ghostObject.transform.GetChild(0).gameObject;

            // 3. Rigidbody の速度 (velocity) を変更して移動させる
            Vector3 cameraForward = Vector3.Scale(camera.transform.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 moveForward = cameraForward * leftStickInput.y + camera.transform.right * leftStickInput.x;
            rb.velocity = moveForward * ghostMoveSpeed + new Vector3(0, rb.velocity.y, 0);
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
        if (LiveFlag)
        {
            if (IsAnySkillActive())
            {
                Debug.Log("スキル発動中は通常攻撃を出せません"); return;
            }

            // ラーテルの場合、アニメーションパラメータが0（Idle）なら強制的にMoveFlagを戻す
            if (character_Status.CharaAnim == Character_Status.CharacterType.RATEL)
                if (animator.GetInteger("RatelSkill") == 0) MoveFlag = true;

            // --- 追加：クールタイムの判定 ---
            Character_Status.CharacterType currentType = character_Status.CharaAnim;

            bool isSpecial = (character_Status.GetMode() == Character_Status.Mode.SPSIAL_ANIMAL);

            float cooldown = 0.0f;

            switch (currentType)
            {
                case Character_Status.CharacterType.LION: cooldown = 2.5f; break;
                case Character_Status.CharacterType.OSTRICH: cooldown = 0.3f; break;
                case Character_Status.CharacterType.RHINOCELOS: cooldown = 1.5f; break;
                case Character_Status.CharacterType.RATEL: cooldown = 1f; break;
            }


            if (cooldownDict.ContainsKey(currentType))
            {
                var settings = animalSettings.Find(s => s.type == currentType);
                // 強化中ならスキルクールタイムを半分にする、といった調整も可能
                cooldown = isSpecial ? settings.skillCooldown * 0.5f : settings.skillCooldown;
            }

            if (cooldownDict.ContainsKey(currentType))
            {
                cooldown = cooldownDict[currentType];
            }

            // 現在の時間 - 最後に攻撃した時間 が クールタイムより短い場合は何もしない
            if (Time.time - lastAttackTime < cooldown)
            {
                Debug.Log($"{currentType} はまだ攻撃できません。残：{cooldown - (Time.time - lastAttackTime):F2}秒");
                return;
            }

            // 攻撃成功：最後に攻撃した時間を更新
            lastAttackTime = Time.time;
            MoveFlag = false;

            // 現在のモデル（通常か強化か）を取得
            GameObject activeModel = GetActiveModel();

            // 【新機能】動物ごとの最適座標を計算して取得
            Vector3 effectPosition = GetEffectSpawnPosition(activeModel);

            switch (character_Status.CharaAnim)
            {
                case Character_Status.CharacterType.LION:
                    SetPlaySE(2);

                    // 1. モデルの「右・上・前」の方向ベクトルを取得
                    Vector3 LIright = activeModel.transform.right;
                    Vector3 LIup = activeModel.transform.up;
                    Vector3 LIforward = activeModel.transform.forward;

                    Position = GetOffsetPosition(
                    activeModel,
                    effectPosition,
                    0f,
                    0.43f,
                    0f);

                    Quaternion = activeModel.transform.rotation * Quaternion.Euler(0, 0, 0);
                    Scale = new Vector3(0.4f, 0.4f, 0.4f);

                    break;

                case Character_Status.CharacterType.OSTRICH:
                    SetPlaySE(3);
                    Position = GetOffsetPosition(
                    activeModel,
                    effectPosition,
                    -0.5f,
                    0f,
                    0f);

                    Quaternion = activeModel.transform.rotation * Quaternion.Euler(20f, 0, 0);
                    Scale = new Vector3(0.3f, 0.3f, 0.3f);
                    break;

                //Vector3 offset = new Vector3(other.gameObject.transform.position.x - 0.7f, other.gameObject.transform.position.y, other.gameObject.transform.position.z - 1.0f);

                case Character_Status.CharacterType.RHINOCELOS:
                    SetPlaySE(4);
                    // 1. モデルの「右・上・前」の方向ベクトルを取得
                    Vector3 RHright = activeModel.transform.right;
                    Vector3 RHup = activeModel.transform.up;
                    Vector3 RHforward = activeModel.transform.forward;
                    Position = effectPosition;

                    Quaternion = activeModel.transform.rotation * Quaternion.Euler(0, 0, 0);
                    Scale = new Vector3(0.3f, 0.3f, 0.3f);
                    break;

                case Character_Status.CharacterType.RATEL:
                    SetPlaySE(5);
                    // 1. モデルの「右・上・前」の方向ベクトルを取得
                    Vector3 RAright = activeModel.transform.right;
                    Vector3 RAup = activeModel.transform.up;
                    Vector3 RAforward = activeModel.transform.forward;
                    Position = GetOffsetPosition(
                    activeModel,
                    effectPosition,
                    0.6f,
                    0.5f,
                    0f);

                    Quaternion = activeModel.transform.rotation * Quaternion.Euler(0, 0, 30);
                    Scale = new Vector3(0.8f, 0.8f, 0.8f);
                    // エフェクト再生
                    Effect_Manager.PlayEffect(normalObject.name, 0, Position, Quaternion, Scale, this.transform);
                    Quaternion = activeModel.transform.rotation * Quaternion.Euler(0, 0, -30);
                    break;
            }

            if (character_Status.CharaAnim != Character_Status.CharacterType.LION)
                Effect_Manager.PlayEffect(normalObject.name, 0, Position, Quaternion, Scale, this.transform);// エフェクト再生
            else Effect_Manager.PlayEffect(normalObject.name, 0, Position, Quaternion, Scale);

            AttackCollider();            // 当たり判定生成
            animator.SetTrigger("Attack");// アニメーション処理
            if (deathFlag) return;
        }
    }

    private Vector3 GetOffsetPosition(
    GameObject model,
    Vector3 basePos,
    float right,
    float up,
    float forward)
    {
        return basePos
            + model.transform.right * right
            + model.transform.up * up
            + model.transform.forward * forward;
    }

    private void OnModeChange(InputAction.CallbackContext context)
    {
        if (LiveFlag == true)
        {
            if (IsAnySkillActive() || IsAnyAttackActive())
            {
                Debug.Log("スキル発動中はモードチェンジできません！");
                return;
            }
            else Debug.Log("モードチェンジ可能");

            Effect_Manager.PlayEffect("Common", 0, this.transform.position,
            this.transform.rotation, new Vector3(2.0f, 2.0f, 2.0f), this.transform);

            switch (character_Status.CharaAnim)
            {
                case Character_Status.CharacterType.LION: SetPlaySE(8); break;
                case Character_Status.CharacterType.OSTRICH: SetPlaySE(9); break;
                case Character_Status.CharacterType.RHINOCELOS: SetPlaySE(10); break;
                case Character_Status.CharacterType.RATEL: SetPlaySE(11); break;
            }
            character_Status.GetModeChange();
            Enhancement();
            Debug.Log("チェンジ");
        }

    }

    //カメラリセット
    private void OnCameraReset(InputAction.CallbackContext context)
    { cameraController.ResetCamera(GetActiveModel()); }

    private GameObject GetActiveModel()
    {
        return character_Status.GetMode()
        == Character_Status.Mode.SPSIAL_ANIMAL
        ? reasonObject
        : normalObject;
    }

    private void OnSkill(InputAction.CallbackContext context)
    {
        if (!LiveFlag) return;
        // Character_Status.CharacterType currentType = character_Status.CharaAnim;
        //GameObject activeModel = GetActiveModel();
        var currentType = character_Status.CharaAnim;
        var activeModel = GetActiveModel();
        var effectPosition = GetEffectSpawnPosition(activeModel);

        // サイだけ先に処理
        if (currentType == Character_Status.CharacterType.RHINOCELOS)
        {
            Debug.Log("サイスキル入口");
            if (Time.time - rhinocerosSkillStartTime < 1.0f) return;
            HandleRhinoSkill(activeModel, effectPosition);
            character_Status.Skill();
            return;
        }

		var skillBase = GetComponent<Animal_Skill_TraitBase>();
		if (skillBase != null && skillBase.SkillCooldownTimer > 0f)
		{
			Debug.Log($"{currentType} のスキルはクールタイム中です。残り: {skillBase.SkillCooldownTimer:F2}秒");
			return; // クールタイム中なら、SEもアニメーションも走らせずにここで終了
		}

		switch (currentType)
        {
            case Character_Status.CharacterType.RATEL:
                var currentRatelSkill = animator.GetInteger("RatelSkill");
                if (currentRatelSkill == 1) // 溜め中 -> 攻撃
                {
					//if (Time.time - ratelSkillStartTime < 1.0f) return;
					AudioManager.Instance.PlaySEByIndex(5, 1.5f);
					animator.SetInteger("RatelSkill", 2);
                    // ★ 攻撃アニメーションが終わる頃に、すべてのフラグを「0」に戻す
                    StartCoroutine(ResetRatelSkillState(0.8f));
                    Invoke("AttackCollider", 0.5f);
                    MoveFlag = true;
                }
                else if (currentRatelSkill == 0) // 待機中(0) から 溜め開始(1) へ
                {
                    Effect_Manager.PlayEffect(normalObject.name, 1,
                    effectPosition, activeModel.transform.rotation, Vector3.one, this.transform);
                    animator.SetInteger("RatelSkill", 1);
                    ratelSkillStartTime = Time.time; // 開始時間を記録
                    AudioManager.Instance.PlaySEByIndex(5, 1.5f);
                    MoveFlag = false;
                    ratelSkillStartHP = character_Status.CurrentHP;
                }
                break;

            case Character_Status.CharacterType.LION:
                //animator.SetTrigger("Skill");
                PlaySkillTrigger(6);
                SetEffect(1); SetEffect(2); SetEffect(3);
                StartCoroutine(StopLionEffectAfterDelay(5.0f));
                break;

            case Character_Status.CharacterType.OSTRICH:
                PlaySkillTrigger(7);
                AttackCollider();
                SetEffect(1);
                break;
        }

        //lastSkillTime = Time.time;
        character_Status.Skill();
    }

    // ライオンやダチョウの重複処理を統一させる＆SE決定
    private void PlaySkillTrigger(int seIndex)
    {
        animator.SetTrigger("Skill");
        AudioManager.Instance.PlaySEByIndex(seIndex, 1.5f);
    }


    private void HandleLionSkill() { }
    private void HandleOstrichSkill() { }
    private void HandleRhinoSkill(
    GameObject activeModel,
    Vector3 effectPosition)
    {
        bool isRhinocerosActive = animator.GetBool("RhinocerosSkill");

        if (isRhinocerosActive)
        {
            animator.SetBool("RhinocerosSkill", false);
            MoveFlag = true;
            Effect_Manager.StopLoopEffect(transform);
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            Debug.Log("サイ：スキル解除");
        }
        else
        {
            animator.SetBool("RhinocerosSkill", true);
            skillDirection = activeModel.transform.forward;
            MoveFlag = false;
            rhinocerosSkillStartTime = Time.time;
            AudioManager.Instance.PlaySEByIndex(14, 1.5f);

            Effect_Manager.PlayEffect(
                normalObject.name,
                1,
                effectPosition + new Vector3(0, -0.2f, 0),
                activeModel.transform.rotation,
                Vector3.one,
                transform,
                true);

            AttackCollider();
        }
    }

    private void HandleRatelSkill() { }

    private IEnumerator ResetRatelSkillState(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (animator != null) animator.SetInteger("RatelSkill", 0);
        MoveFlag = true;
    }


    private void OnEvation(InputAction.CallbackContext context)
    {
        if (LiveFlag == true)
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

    private void RemoveUpDown() { rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); }

    public void SetDeath()
    {
        if (deathFlag == false)
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

            Effect_Manager.PlayEffect("Common", 4, this.gameObject.transform.position, this.gameObject.transform.rotation, new Vector3(1f, 1f, 1f));

            Invoke("DeleteEffect", 2.0f);

            deathFlag = true;
            MoveFlag = false;
        }
    }

    // アニメーションで攻撃の当たり判定を出す
    public Vector3 AttackCollider()
    {
        // 現在アクティブなモデルを取得する
        GameObject activeModel = GetActiveModel();

        // 2. 出現位置の計算
        // activeModel.transform.forward : モデルが向いている正面方向
        // attackOffset : インスペクターで設定できる「前方にどれくらい離すか」の距離
        Vector3 spawnPosition = activeModel.transform.position + new Vector3(0f, 0.5f, 0f) + activeModel.transform.forward * CharacterData.ATTACK_OFFSET;

        // 3. 当たり判定の生成
        // Quaternion.identity ではなく activeModel.transform.rotation を渡すことで、向きを合わせます
        collisionObject = Instantiate(Collision, spawnPosition, activeModel.transform.rotation, this.gameObject.transform);

        return collisionObject.transform.position;
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
        else { Debug.LogWarning($"{normal.name} に Animator が見つかりません。"); }
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

    public void MoveFlagFalse(){ MoveFlag = true; }

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

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "DamageZone")
        {
            outOfRangeDamageTimer += Time.deltaTime;

            if (outOfRangeDamageTimer > 2f)
            {
                // ダメージ処理
                if (character_Status != null)
                    character_Status.OutOfRangeDamage();

                outOfRangeDamageTimer = 0f;  // タイマーリセット
            }
        }
        else outOfRangeDamageTimer = 0f;    // 範囲内に戻ったらタイマーリセット
    }

    // 奈落に落ちたかのチェック
    private void CheckAbyss()
    {
        // Y座標が-50より上にいるなら処理しない
        if (this.transform.position.y > -50f) return;

        // 奈落死
        if (character_Status != null)
            character_Status.DieAbyss();
    }

    // 5秒待ってからエフェクトを止める魔法の関数
    private IEnumerator StopLionEffectAfterDelay(float delay)
    {
        // 指定された秒数（5秒）待つ
        yield return new WaitForSeconds(delay);

        // エフェクトを止める
        if (Effect_Manager != null)
        {
            Effect_Manager.StopLoopEffect(this.transform);
            Debug.Log("ライオンのループエフェクトを停止しました");
        }
    }

    public bool IsRhinocerosSkillActive()
    {
        if (animator == null) return false;
        return animator.GetBool("RhinocerosSkill");
    }

    public bool GetGameSetFlg(){ return isGameFinished; }

    public void Win()
    {
        if (isGameFinished) return; // 二重呼び出し防止
        isGameFinished = true;
        MoveFlag = false;
        rb.velocity = Vector3.zero; // 物理移動も止める
        Debug.Log("ゲーム終了");
        ResetAnimatorParameters();
        Effect_Manager.StopLoopEffect(this.gameObject.transform);

        animator.SetInteger("State", 3);

        LiveFlag = false;

        character_Status.ItemHeal(character_Status.CurrentHP, character_Status.CurrentReason);

        // ★追加：カメラの方を向かせる処理
        if (cameraObject != null)
        {
            // 1. カメラと自分の水平な位置関係を計算（高さYは無視して、足元で向きを合わせる）
            Vector3 targetPos = cameraObject.transform.position;
            targetPos.y = this.transform.position.y; // プレイヤーの高さに合わせる（仰け反り防止）

            Vector3 directionToCamera = targetPos - this.transform.position;

            if (directionToCamera != Vector3.zero)
            {
                // 2. カメラの方を向く回転値を作成
                Quaternion lookAtCameraRotation = Quaternion.LookRotation(directionToCamera);

                // 3. アクティブなモデルを回転させる
                normalObject.transform.rotation = lookAtCameraRotation;
                reasonObject.transform.rotation = lookAtCameraRotation;

                // cachedRotateも更新しておくと、その後の挙動が安定します
                cachedRotate = lookAtCameraRotation;
            }
        }

        // エフェクト演出（既存の処理）
        switch (character_Status.CharaAnim)
        {
            case Character_Status.CharacterType.LION:
                Effect_Manager.PlayEffect(normalObject.name, 4, this.gameObject.transform.position, this.gameObject.transform.rotation, new Vector3(1f, 1f, 1f), this.gameObject.transform);
                break;
            case Character_Status.CharacterType.OSTRICH:
                Effect_Manager.PlayEffect(normalObject.name, 2, this.gameObject.transform.position, this.gameObject.transform.rotation, new Vector3(1f, 1f, 1f), this.gameObject.transform);
                break;
            case Character_Status.CharacterType.RHINOCELOS:
                Effect_Manager.PlayEffect(normalObject.name, 2, this.gameObject.transform.position, this.gameObject.transform.rotation, new Vector3(1f, 1f, 1f), this.gameObject.transform);
                break;
            case Character_Status.CharacterType.RATEL:
                Effect_Manager.PlayEffect(normalObject.name, 3, this.gameObject.transform.position, this.gameObject.transform.rotation, new Vector3(1f, 1f, 1f), this.gameObject.transform);
                break;
        }
    }

    private bool IsAnySkillActive()
    {
        if (animator == null) return false;

        // 1. ラーテルとサイの既存の判定（パラメータベース）
        if (character_Status.CharaAnim == Character_Status.CharacterType.RATEL)
        {
            int ratelState = animator.GetInteger("RatelSkill");
            if (ratelState == 1 || ratelState == 2) return true;
        }

        if (character_Status.CharaAnim == Character_Status.CharacterType.RHINOCELOS)
        {
            if (animator.GetBool("RhinocerosSkill")) return true;
        }

        // 2. 【重要】ライオン・ダチョウなどの「モーション中」判定
        // 現在のベースレイヤー(0番目)のアニメーション情報を取得
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // モーション名に "Skill" が含まれている、かつ 再生が終わっていない(1.0未満)ならスキル中とみなす
        // ※Animator上のステート名が "Skill" であることを確認してください
        if (stateInfo.IsName("Skill"))
        {
            if (stateInfo.normalizedTime < 1.0f) { return true; }
        }
        return false;
    }

    private bool IsAnyAttackActive()
    {
        if (animator == null) return false;

        // 現在のベースレイヤーのアニメーション情報を取得
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // 再生中のアニメーション名が "Attack" かどうか、かつ再生が完了していないかチェック
        // ※Animator上でのステート名が "Attack" である必要があります
        if (stateInfo.IsName("Attack"))
        {
            // normalizedTime が 1.0f 未満であれば再生中とみなす
            if (stateInfo.normalizedTime < 1.0f) { return true; }
        }
        return false;
    }

    public void ResetRatelSkillParam()
    {
        if (character_Status.CharaAnim == Character_Status.CharacterType.RATEL)
        {
            animator.SetInteger("RatelSkill", 0);
            animator.SetInteger("State", 0);
            MoveFlag = true;
        }
        else Debug.Log("ラーテルじゃない");
    }

    public void ResetAnimatorParameters()
    {
        if (animator == null) return;

        // 数値系 (Int, Float) のリセット
        animator.SetInteger("State", 0);      // 0: Idle
        animator.SetInteger("RatelSkill", 0); // ラーテルの溜め状態解除

        // トリガー系 (Trigger) のリセット
        // ※発動待ちのトリガーがある場合に備えてリセットします
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Skill");
        animator.ResetTrigger("LeftStep");
        animator.ResetTrigger("RightStep");
        animator.ResetTrigger("BackStep");

        // 真偽値系 (Bool) のリセット
        animator.SetBool("RhinocerosSkill", false); // サイの突進解除
        MoveFlag = true;// 動けなくなっている場合はフラグも戻す
        Debug.Log("アニメーターのパラメータを初期化しました。");
    }

    //SEをセット
    private void SetPlaySE(int index) { AudioManager.Instance.PlaySEByIndex(index, 1.5f); }

    //エフェクトをセット
    private void SetEffect(int id)
    {
        Effect_Manager.PlayEffect(normalObject.name, id, this.gameObject.transform.position, this.gameObject.transform.rotation, new Vector3(1, 1, 1), this.transform);
    }
}

