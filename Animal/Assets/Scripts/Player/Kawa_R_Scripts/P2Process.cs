using UnityEngine;

public class P2Process : MonoBehaviour
{
	UIManager ui_manager;
	Character_Status character_Status;

	[SerializeField] GameObject canvas;


	void Start()
	{
		SetCreateGauge();
		Set_2_Player();
	}

	// Update is called once per frame
	void Update()
	{

	}


	//HP・Reasonゲージ生成関数(2P用)
	private void SetCreateGauge()
	{
		canvas = GameObject.Find("Player_Canvas");
		ui_manager = canvas.GetComponent<UIManager>();

		//2P用のHP・Reasonゲージ位置取得
		Transform BarPosition = canvas.transform.Find("2PBarPosition");
		Transform ReasonPosition = canvas.transform.Find("2PBarPosition");

		//Reasonゲージ位置調整&ゲージ生成
		ReasonPosition.position = new Vector3
		(ReasonPosition.position.x + 25, ReasonPosition.position.y, ReasonPosition.position.z);

		ui_manager.CreateUI(UIManager.UI_ID.GAUGE_REASON, ReasonPosition);


		//HPゲージ位置調整&ゲージ生成
		BarPosition.position = new Vector3
		(BarPosition.position.x, BarPosition.position.y - 10, BarPosition.position.z);

		ui_manager.CreateUI(UIManager.UI_ID.GAUGE_HP, BarPosition);
	}


	//2Pキャラクターの情報設定関数
	private void Set_2_Player()
	{
		character_Status = this.GetComponent<Character_Status>();
		GameObject hpGaugeObj = ui_manager.ui_list[4];

		character_Status.hp_object = hpGaugeObj;

		GameObject ReasonGaugeObj = ui_manager.ui_list[3];

		character_Status.reason_object = ReasonGaugeObj;

		//ゲージ初期化関数呼び出し
		if (character_Status != null)
		{
			character_Status.InitGauges();
		}
	}
}