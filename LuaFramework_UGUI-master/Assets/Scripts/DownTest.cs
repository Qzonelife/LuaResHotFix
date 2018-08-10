using UnityEngine;
using System.Collections;
using System.Text;

public class DownTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
	    Debug.Log("start down");
	    StartCoroutine(DDD());
	}

    IEnumerator DDD()
    {
        WWW www = new WWW("ftp://192.168.10.218/test.txt");
        
        yield return www;
        if (www.isDone)
        {
            Debug.Log(www.error);
            Debug.Log(www.text);
        }
    }

    // Update is called once per frame
	void Update () {
	
	}

}
