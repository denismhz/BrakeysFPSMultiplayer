
using UnityEngine;

public class Util {

    public static void SetLayerRecursively(GameObject _obj, int _newLayer)
    {
        if(_obj == null)
        {
            return;
        }

        _obj.layer = _newLayer;

        foreach(Transform _child in _obj.transform)
        {
            if(_child == null)
            {
                continue;
            }
            SetLayerRecursively(_child.gameObject, _newLayer);
        }
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
