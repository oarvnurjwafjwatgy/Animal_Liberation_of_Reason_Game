using UnityEngine;

public class Character_Status : MonoBehaviour
{
	/******�X�e�[�^�X�ϐ�*************/
	[Header("��{�X�e�[�^�X")]
	[SerializeField] protected int MaxHP = 100;                         // �L�����N�^�[�ő�HP
	[SerializeField] protected int MaxReason = 100;                     // �L�����N�^�[�ő�HP
	[SerializeField] protected int ResonPoint = 100;                    // �����Q�[�W
	[SerializeField] protected int decrease_in_reason_time = 1;         // �����Q�[�W�����_���[�W
	[SerializeField] protected int Attackpower = 10;                    // �L�����N�^�[�U����
	[SerializeField] protected int Defensepower = 20;                   // �L�����N�^�[�h���
	[SerializeField] protected float MoveSpeed = 5.0f;                  // �L�����N�^�[�ړ����x

	private float timer = 0f;

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
	enum Mode
	{
		ANIMAL,         // �G�j���[
		SPSIAL_ANIMAL   // �X�y�V�����G�j���[
	}


	State CharaState; // �L�����N�^�[��ԕϐ�
	Mode CharaMode;   // �L�����N�^�[���[�h�ϐ�

	//������
	private void Start()
	{
		CharaState = State.IDLE;    // ������Ԃ�ҋ@��Ԃɐݒ�
		CharaMode = Mode.ANIMAL;    // �������[�h���G�j���[�ɐݒ�
		CurrentHP = MaxHP;          // ����HP�ɍő�HP����
		CurrentReason = MaxReason;  // ���ݗ����|�C���g�ɍő嗝���|�C���g����
		GetResonPoint();            // �����Q�[�W�擾
		GetAttackPower();           // �U���͎擾
		GetDefensePower();          // �h��͎擾
		GetMoveSpeed();             // �ړ����x�擾
	}

	//�X�V
	void Update()
	{
		GetCurrentHP();
		GetResonPoint();

		if (Input.GetKeyDown(KeyCode.P))
		{
			TakeDamage(40);
		}

		if (Input.GetKeyDown(KeyCode.O))
		{
			switch (CharaMode)
			{
				case Mode.ANIMAL:
					CharaMode = Mode.SPSIAL_ANIMAL;
					Debug.Log("���[�h���X�y�V�����G�j���[�ɕω������B");
					break;

				case Mode.SPSIAL_ANIMAL:
					CharaMode = Mode.ANIMAL;
					Debug.Log("���[�h���G�j���[�ɕω������B");
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
					Mode_SpsialAnimal(decrease_in_reason_time);
				}
				break;
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
			int actualDamage = Mathf.Max(damage - Defensepower, 0);
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
	private int GetCurrentHP()
	{
		return CurrentHP;
	}

	//�����Q�[�W�擾�֐�
	private int GetResonPoint()
	{
		return ResonPoint;
	}

	//�U���͎擾�֐�
	private int GetAttackPower()
	{
		return Attackpower;
	}

	//�h��͎擾�֐�
	private int GetDefensePower()
	{
		return Defensepower;
	}

	//�ړ����x�擾�֐�
	private float GetMoveSpeed()
	{
		return MoveSpeed;
	}


	//���S�����֐�
	protected virtual void Die()
	{
		CharaState = State.DEAD; // ��Ԃ����S��ԂɕύX
		Debug.Log($"{gameObject.name} �͎��S�����B");

		gameObject.SetActive(false); // �L�����N�^�[�I�u�W�F�N�g���A�N�e�B�u��(��)
	}

	protected virtual void Mode_Animal(int damage)
	{
		// �G�j���[���[�h�̎󂯂��_���[�W����

		// �_���[�W�v�Z�i�h��͂��l���j
		int actualDamage = Mathf.Max(damage - Defensepower, 0);
		Debug.Log($"{gameObject.name} ��" + actualDamage + "�̃_���[�W���󂯂��B");
		CurrentHP -= actualDamage;

		Debug.Log("���݂�HP:" + CurrentHP);
	}

	protected virtual void Mode_SpsialAnimal(int damage)
	{
		int num = 0;

		if (CurrentReason >= 0)
		{
			num = decrease_in_reason_time / 10;	 // �����Q�[�W�����ʌv�Z
			CurrentReason -= num;                // �����Q�[�W��������
			Debug.Log("���݂̗����|�C���g:" + CurrentReason);
		}
		else
		{
			CurrentReason = 0;
		}
	}
}
