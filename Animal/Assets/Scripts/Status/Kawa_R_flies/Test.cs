using UnityEngine;

public class Test : MonoBehaviour
{
    Character_Status status;

    // Start is called before the first frame update
    void Start()
    {
        status = this.GetComponent<Character_Status>();
	}

    // Update is called once per frame
    void Update()
    {
        status.GetDefensePower();
    }
}
