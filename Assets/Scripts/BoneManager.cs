using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneManager : MonoBehaviour
{
    public GameObject joint;
    public GameObject shaft;
    public GameObject endEffector;
    public GameObject balancer;
    public float boneLength;
    public Vector3 endEffectorWorldPos
    {
        get
        {
            return endEffector.transform.position;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        endEffector.transform.localPosition = boneLength * Vector3.up;
        balancer.transform.localPosition = boneLength * Vector3.down;
        shaft.transform.localPosition = boneLength * Vector3.up * .5f;
        shaft.transform.localScale = new Vector3(1, boneLength, 1);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
