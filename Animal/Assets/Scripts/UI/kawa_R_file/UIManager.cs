using System.Collections.Generic;
using UnityEngine;



public class UIManager : MonoBehaviour
{
    public enum UI_ID
    {
        GAUGE_HP,
        GAUGE_REASON
    }

    public List<GameObject> ui_list = new List<GameObject>();
    public Transform canvasParent;

	Transform pos;

    void Start()
    {

        GameObject Canvas = GameObject.Find("Player_Canvas");

        Transform HPBarPosition = Canvas.transform.Find("1PBarPosition");



        canvasParent = GetComponent<Transform>();
        canvasParent.transform.localPosition = Vector3.zero;
        //CreateUI(UI_ID.GAUGE_HP, HPBarPosition);

    }

    public void CreateUI(UI_ID ui_id,Transform pos)
    {
        switch (ui_id)
        {
            case UI_ID.GAUGE_HP:
                GameObject ListObjects_HP = GameObject.Instantiate
                (Resources.Load("Prefab/UI/HP_ber")) as GameObject;

                ListObjects_HP.transform.SetParent(canvasParent, false);
                ListObjects_HP.transform.localPosition = pos.position;
                ListObjects_HP.transform.localScale = Vector3.one;
                ListObjects_HP.transform.rotation = Quaternion.identity;
                ui_list.Add(ListObjects_HP);   //リストに追加

                break;


            case UI_ID.GAUGE_REASON:
                GameObject ListObjects_Reason = GameObject.Instantiate
                   (Resources.Load("Prefab/UI/Reason_ber")) as GameObject;

                ListObjects_Reason.transform.SetParent(canvasParent, false);
                ListObjects_Reason.transform.localPosition = pos.position;
                ListObjects_Reason.transform.localScale = Vector3.one;
                ListObjects_Reason.transform.rotation = Quaternion.identity;
                ui_list.Add(ListObjects_Reason);   //リストに追加
                break;
        }
	}
}
