using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public bool freezeX;
    public bool freezeY;
    public bool freezeZ;


    // Update is called once per frame
    void Update()
    {
        transform.forward = new Vector3(freezeX ? 0 : Camera.main.transform.forward.x, freezeY ? 0 : Camera.main.transform.forward.y, freezeZ ? 0 : Camera.main.transform.forward.z);
    }
}
