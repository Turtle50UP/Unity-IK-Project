using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneSpawner : MonoBehaviour
{
    public List<GameObject> boneList;
    public GameObject bonePrefab;
    public GameObject lastBone;
    public float bonesLength;
    BoneManager lastbm;
    public float addDelay;
    float delayStart;
    public int boneCount = 0;
    public Vector3 endEffectorPos
    {
        get
        {
            if(lastbm != null)
            {
                return lastbm.endEffectorWorldPos;
            }
            else
            {
                return this.transform.position;
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            bonesLength += .1f;
            delayStart = Time.time;
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            if (bonesLength - .1f > .09f)
            {
                bonesLength -= .1f;
            }
            delayStart = Time.time;
        }
        if (Input.GetKey(KeyCode.F) && Time.time - delayStart > addDelay)
        {
            bonesLength += .1f;
        }
        else if (Input.GetKey(KeyCode.V) && bonesLength - .1f > .09f && Time.time - delayStart > addDelay)
        {
            bonesLength -= .1f;
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            GameObject thisBone = GameObject.Instantiate(bonePrefab);
            BoneManager tbm = thisBone.GetComponent<BoneManager>();
            tbm.boneLength = bonesLength;
            if (lastBone != null)
            {
                thisBone.transform.position = lastbm.endEffectorWorldPos;
            }
            boneList.Add(thisBone);
            lastBone = thisBone;
            lastbm = lastBone.GetComponent<BoneManager>();
            boneCount++;
        }
        if (Input.GetKeyDown(KeyCode.C) && lastBone != null)
        {
            int lbidx = boneList.IndexOf(lastBone);
            lastbm = null;
            boneList.RemoveAt(lbidx);
            GameObject.Destroy(lastBone);
            if(boneList.Count == 0)
            {
                lastBone = null;
            }
            else
            {
                lastBone = boneList[lbidx - 1];
                lastbm = lastBone.GetComponent<BoneManager>();
            }
            boneCount--;
        }
    }
}
