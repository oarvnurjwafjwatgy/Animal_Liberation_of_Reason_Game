using UnityEngine;
using UnityEngine.UI;

public class Character_Status : MonoBehaviour
{
	/******�X�e�[�^�X�ϐ�*************/
	[Header("��{�X�e�[�^�X")]
	[SerializeField] protected int MaxHP = 400;                         // �L�����N�^�[�ő�HP
	[SerializeField] protected int MaxReason = 100;                     // �L�����N�^�[�����ő�HP
	[SerializeField] protected int ReasonPoint = 100;                   // �����Q�[�W
	[SerializeField] protected int AttackPower = 10;                    // �L�����N�^�[�U����
	[SerializeField] protected int DefensePower = 20;                   // �L�����N�^�[�h���
	[SerializeField] protected float MoveSpeed = 5.0f;                  // �L�����N�^�[�ړ����x

	[Header("�����Q�[�W������̌����ݒ�")]
	[SerializeField] protected int Decrease_in_reason_time = 1;         // �����Q�[�W�����_���[�W


	[Header("���������ԃX�e�[�^�X")]
	[SerializeField] protected int ReasonHP = 200;                      // �L�����N�^�[����������ő�HP
	[SerializeField] protected int ReasonAttackPower = 50;              // �L�����N�^�[����������U����
	[SerializeField] protected int ReasonDefensePower = 60;             // �L�����N�^�[����������U����
	[SerializeField] protected float ReasonMoveSpeed = 1.0f;            // �L�����N�^�[�ړ����x

	private Slider hp_gauge;			   //HP�Q�[�WUI�X���C�_�[�Q�Ɨp�ϐ�
	private Slider reason_gauge;        //HP�Q�[�WUI�X���C�_�[�Q�Ɨp�ϐ�
	private Animator animator;		   //�A�j���[�^�[�Q�Ɨp�ϐ�

	private float timer = 0f;       //�����Q�[�W�����p�^�C�}�[

	public GameObject hp_object;
	public GameObject reason_object;

	public int CurrentHP { get; protected set; }    // �L�����N�^�[����HP(�O���ǂݎ��A�����ύX��)
	public int CurrentReason { get; protected set; }    // �L�����N�^�[���ݗ���HP(�O���ǂݎ��A�����ύX��)

	/**********���*******************/
	enum State
	{
		IDLE,       // �ҋ@���
		MOVE,       // �ړ����
		ATTAKING,   // �U�����
		DEAD        // ���S���
	}

	/**********���[�h*******************/
	public enum Mode
	{
		ANIMAL,         // �G�j���[
		SPSIAL_ANIMAL   // �X�y�V�����G�j���[
	}


	State CharaState; // �L�����N�^�[��ԕϐ�
	Mode CharaMode;   // �L�����N�^�[���[�h�ϐ�

	//������
	private void Start()
	{
		//HP�Q�[�W�̃I�u�W�F�N�g��T���Ď����I�Ɏ擾������B
		//hp_object = GameObject.Find("HP_ber");

		//�����Q�[�W�̃I�u�W�F�N�g��T���Ď����I�Ɏ擾������B
		//reason_object = GameObject.Find("Reason_ber");

		CharaState = State.IDLE;    // ������Ԃ�ҋ@��Ԃɐݒ�
		CharaMode = Mode.ANIMAL;    // �������[�h���G�j���[�ɐݒ�
		CurrentHP = MaxHP;          // ����HP�ɍő�HP����
		CurrentReason = MaxReason;  // ���ݗ����|�C���g�ɍő嗝���|�C���g����
		GetResonPoint();            // �����Q�[�W�擾
		GetAttackPower();           // �U���͎擾
		GetDefensePower();          // �h��͎擾
		GetMoveSpeed();             // �ړ����x�擾

		animator = GetComponent<Animator>();
	}

	//�X�V
	void Update()
	{
		GetCurrentHP();
		GetResonPoint();

		// HP�Q�[�W�̌��ݒl���X�V
		if (hp_gauge != null && reason_gauge != null)
		{
			hp_gauge.value = CurrentHP;
			reason_gauge.value = CurrentReason;
		}

		if (Input.GetKeyDown(KeyCode.P))
		{
			TakeDamage(40);
		}

		if (Input.GetKeyDown(KeyCode.O))
		{
			GetModeChange();
		}

		if(Input.GetKeyDown(KeyCode.K))
		{
			switch (CharaMode)
			{
				case Mode.ANIMAL:
					
					break;
				case Mode.SPSIAL_ANIMAL:

					break;
			}
			
		}

		switch (CharaMode)
		{
			case Mode.ANIMAL:
				// �G�j���[���[�h�̏���
				break;
			// �X�y�V�����G�j���[���[�h�̗����Q�[�W���������֐��Ăяo��
			case Mode.SPSIAL_ANIMAL:

				timer += Time.deltaTime;
				if (timer >= 1f)
				{
					Mode_SpsialAnimal();
					timer = 0f;
				}
				break;
		}
	}

	public void InitGauges()
	{
		// HP�Q�[�W�X���C�_�[�R���|�[�l���g�擾
		if (hp_object != null)
		{
			hp_gauge = hp_object.GetComponent<Slider>();

			// HP�Q�[�W�̃I�u�W�F�N�g�ɍő�l�ƌ��ݒl��ݒ�
			hp_gauge.maxValue = MaxHP;
			hp_gauge.value = CurrentHP;
		}

		// �����Q�[�W�X���C�_�[�R���|�[�l���g�擾
		if (reason_object != null)
		{
			reason_gauge = reason_object.GetComponent<Slider>();

			// �����Q�[�W�̃I�u�W�F�N�g�ɍő�l�ƌ��ݒl��ݒ�
			reason_gauge.maxValue = MaxReason;
			reason_gauge.value = CurrentReason;
		}
	}


	//���S�����֐�&�_���[�W�����֐�
	protected virtual void TakeDamage(int damage)
	{
		// ���[�h���Ƃ̃_���[�W��������
		if (CharaMode == Mode.ANIMAL)
		{
			Mode_Animal(damage);  // �G�j���[���[�h�̃_���[�W�����֐��Ăяo��
		}
		else if (CharaMode == Mode.SPSIAL_ANIMAL)
		{
			// �_���[�W�v�Z�i�h��͂��l���j
			int actualDamage = Mathf.Max(damage - DefensePower, 0);
			CurrentReason -= actualDamage; // �����Q�[�W��������
		}

		// HP��0�ȉ��ɂȂ����ꍇ�̏���
		if (CurrentHP <= 0)
		{
			CurrentHP = 0;
			Die();  // ���S�����֐��Ăяo��
		}
	}

	//����HP�擾�֐�
	public int GetCurrentHP()
	{
		return CurrentHP;
	}

	//�����Q�[�W�擾�֐�
	public int GetResonPoint()
	{
		return ReasonPoint;
	}

	//�U���͎擾�֐�
	public int GetAttackPower()
	{
		return AttackPower;
	}

	//�h��͎擾�֐�
	public int GetDefensePower()
	{
		return DefensePower;
	}

	//�ړ����x�擾�֐�
	public float GetMoveSpeed()
	{
		return MoveSpeed;
	}

	public Mode GetMode()
	{
		return CharaMode;
	}

	//���[�h���؂�ւ����ɌĂяo���֐�
	public void GetModeChange()
	{
		switch (CharaMode)
		{
			case Mode.ANIMAL:
				CharaMode = Mode.SPSIAL_ANIMAL;
				animator.SetBool("Reason", true);
				Debug.Log("���[�h���X�y�V�����G�j���[�ɕω������B");
				break;

			case Mode.SPSIAL_ANIMAL:
				DefaultGetStatus();
				animator.SetBool("Reason", false);
				break;
		}
	}

	//�؂�ւ���Animal�X�e�[�^�X�擾�֐�
	public void DefaultGetStatus()
	{
		CharaMode = Mode.ANIMAL;
		Debug.Log("���[�h���G�j���[�ɕω������B");

	}

	//���S�����֐�
	protected virtual void Die()
	{
		hp_gauge.value = 0;
		CharaState = State.DEAD; // ��Ԃ����S��ԂɕύX
		Debug.Log($"{gameObject.name} �͎��S�����B");

		gameObject.SetActive(false); // �L�����N�^�[�I�u�W�F�N�g���A�N�e�B�u��(��)
	}

	protected virtual void Mode_Animal(int damage)
	{
		// �G�j���[���[�h�̎󂯂��_���[�W����

		// �_���[�W�v�Z�i�h��͂��l���j
		int actualDamage = Mathf.Max(damage - DefensePower, 0);
		Debug.Log($"{gameObject.name} ��" + actualDamage + "�̃_���[�W���󂯂��B");
		CurrentHP -= actualDamage;

		Debug.Log("���݂�HP:" + CurrentHP);
	}

	//�X�y�V�����G�j���[���[�h�����Q�[�W���������֐�
	protected virtual void Mode_SpsialAnimal()
	{
		int num = 0;

		//����������0���傫���Ȃ痝���Q�[�W������������
		if (CurrentReason > 0)
		{
			num = Decrease_in_reason_time;		 // �����Q�[�W�����ʌv�Z
			CurrentReason -= num;                // �����Q�[�W��������
			Debug.Log("���݂̗����|�C���g:" + CurrentReason);
		}
		//����������0�ȉ��Ȃ痝���Q�[�W��0�ɂ��Ď��S�������s��
		else
		{
			CurrentReason = 0;
			Die();
		}
	}
}
