using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
	public enum UI_ID { GAUGE_HP, GAUGE_REASON }
	public List<GameObject> ui_list = new List<GameObject>();
	public Transform canvasParent;

	public void CreateUI(UI_ID ui_id, Transform pos, int pID) // pIDを追加
	{
		GameObject prefab = null;
		if (ui_id == UI_ID.GAUGE_HP) prefab = Resources.Load("Prefab/UI/HP_ber") as GameObject;
		else prefab = Resources.Load("Prefab/UI/Reason_ber") as GameObject;

		if (prefab != null)
		{
			GameObject uiObj = GameObject.Instantiate(prefab);
			uiObj.transform.SetParent(canvasParent, false);
			uiObj.transform.SetAsLastSibling();

			RectTransform rect = uiObj.GetComponent<RectTransform>();
			if (rect != null)
			{
				// 全体のサイズを少し小さくして画面分割に対応
				rect.localScale = new Vector3(0.8f, 0.8f, 1.0f);
				rect.position = pos.position;

				// 理性ゲージ（Reason）ならHPバーの上に少し重ねる
				if (ui_id == UI_ID.GAUGE_REASON)
				{
					rect.anchoredPosition += new Vector2(0f, 19f);
					rect.localScale = new Vector3(0.8f, 0.6f, 1.0f); // 少し細長く
				}
			}
			ui_list.Add(uiObj);
		}
	}
}