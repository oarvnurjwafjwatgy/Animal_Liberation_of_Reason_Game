using UnityEngine;

public class P1Process : MonoBehaviour
{
    UIManager ui_manager;
	Character_Status character_Status;

	[SerializeField] GameObject canvas;

	//GameObject Canvas = GameObject.Find("Player_Canvas");

	void Start()
	{
		SetCreateGauge();
		Set_1_Player();
	}

    // Update is called once per frame
    void Update()
    {
        
    }


	//HP�EReason�Q�[�W�����֐�(1P�p)
	private void SetCreateGauge()
	{
		canvas = GameObject.Find("Player_Canvas");
		ui_manager = canvas.GetComponent<UIManager>();

		//1P�p��HP�EReason�Q�[�W�ʒu�擾
		Transform BarPosition = canvas.transform.Find("1PBarPosition");
		Transform ReasonPosition = canvas.transform.Find("1PBarPosition");

		//Reason�Q�[�W�ʒu����&�Q�[�W����
		ReasonPosition.position = new Vector3
		(ReasonPosition.position.x + 25, ReasonPosition.position.y, ReasonPosition.position.z);

		ui_manager.CreateUI(UIManager.UI_ID.GAUGE_REASON, ReasonPosition);


		//HP�Q�[�W�ʒu����&�Q�[�W����
		BarPosition.position = new Vector3
		(BarPosition.position.x, BarPosition.position.y - 10, BarPosition.position.z);

		ui_manager.CreateUI(UIManager.UI_ID.GAUGE_HP, BarPosition);
	}


	//1P�L�����N�^�[�̏��ݒ�֐�
	private void Set_1_Player()
	{
		character_Status = this.GetComponent<Character_Status>();
		GameObject hpGaugeObj = ui_manager.ui_list[1];

		character_Status.hp_object = hpGaugeObj;

		GameObject ReasonGaugeObj = ui_manager.ui_list[0];

		character_Status.reason_object = ReasonGaugeObj;

		//�Q�[�W�������֐��Ăяo��
		if (character_Status !=null)
		{
			character_Status.InitGauges();
		}
	}
}
