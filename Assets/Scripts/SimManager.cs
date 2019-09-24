using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimManager : MonoBehaviour
{
    public GameObject boneSpawner;
    BoneSpawner bs;
    public GameObject boneZone;
    Vector3 xyProject;
    Vector3 yzProject;
    Vector3 xzProject;
    public float epsilon = 0.0001f;
    public bool slowMode = false;
    public int simMode = 1;
    Vector3 stepHomeLoc;
    Vector3 targetLoc
    {
        get
        {
            return boneZone.transform.position;
        }
    }
    public bool simStarted = false;
    int bonePos = -1;
    public float timeDelay;
    float slowModeTime;
    public float delE;
    int fabrikPos = -1;
    bool isBackprop = true;
    float adjustFactor;
    // Start is called before the first frame update
    void Start()
    {
        bs = boneSpawner.GetComponent<BoneSpawner>();
        xyProject = new Vector3(1, 1, 0);
        yzProject = new Vector3(0, 1, 1);
        xzProject = new Vector3(1, 0, 1);
        stepHomeLoc = Vector3.zero;
    }

    void ResetSlowMode()
    {
        bonePos = bs.boneList.Count - 1;
        fabrikPos = bs.boneList.Count - 1;
        isBackprop = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            simStarted = !simStarted;
            if (simStarted)
            {
                Debug.Log("Sim has started");
            }
            else
            {
                Debug.Log("Sim has stopped");
            }
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            slowMode = !slowMode;
            if (slowMode)
            {
                slowModeTime = Time.time;
                ResetSlowMode();
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            simMode = 1;
            ResetSlowMode();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            simMode = 2;
            ResetSlowMode();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            simMode = 3;
            ResetSlowMode();
        }
        if (simStarted)
        {
            switch (simMode)
            {
                case 1:
                    DoCCD();
                    break;
                case 2:
                    DoJT();
                    break;
                case 3:
                    DoFABRIK();
                    break;
                default:
                    DoCCD();
                    break;
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            timeDelay *= .1f;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            timeDelay *= 10f;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            delE *= .1f;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            delE *= 10f;
        }
    }

    List<Vector3> GetRotationAxes()
    {
        List<Vector3> res = new List<Vector3>();
        for (int i = 0; i < bs.boneList.Count; i++)
        {
            GameObject curBone = bs.boneList[i];
            BoneManager cbbm = curBone.GetComponent<BoneManager>();
            Vector3 axisToSnap = targetLoc - curBone.transform.position;
            Vector3 eeAxis = bs.endEffectorPos - curBone.transform.position;
            res.Add(Vector3.Cross(eeAxis, axisToSnap).normalized);
        }
        return res;
    }

    List<Vector3> GetJacobian()
    {
        List<Vector3> res = new List<Vector3>();
        for (int i = 0; i < bs.boneList.Count; i++)
        {
            GameObject curBone = bs.boneList[i];
            BoneManager cbbm = curBone.GetComponent<BoneManager>();
            Vector3 axisToSnap = targetLoc - curBone.transform.position;
            Vector3 eeAxis = bs.endEffectorPos - curBone.transform.position;
            Vector3 axis = Vector3.Cross(eeAxis, axisToSnap).normalized;
            res.Add(Vector3.Cross(axis,eeAxis));
        }
        return res;
    }

    void DoJT()
    {
        if (slowMode)
        {
            if (Time.time - slowModeTime > timeDelay)
            {
                slowModeTime = Time.time;
            }
            else
            {
                return;
            }
        }
        if(bs.boneList.Count > 0)
        {
            List<Vector3> axes = GetRotationAxes();
            List<Vector3> jacobian = GetJacobian();
            Vector3 delEV = delE * (targetLoc - bs.endEffectorPos);
            for(int i = 0; i < bs.boneList.Count; i++)
            {
                Vector3 thisAxis = axes[i];
                float delTheta = Vector3.Dot(jacobian[i], delEV);
                Debug.Log(delTheta);
                GameObject curBone = bs.boneList[i];
                curBone.transform.Rotate(thisAxis, delTheta, Space.World);
                for (int j = i; j < bs.boneList.Count - 1; j++)
                {
                    GameObject prevBone = bs.boneList[j];
                    GameObject propBone = bs.boneList[j + 1];
                    propBone.transform.Rotate(thisAxis, delTheta, Space.World);
                    propBone.transform.position = prevBone.GetComponent<BoneManager>().endEffectorWorldPos;
                }
            }
        }
    }
    /*
    List<Vector3> GetPseudoInverse()
    {
    }

    void DoJPI()
    {
        if (slowMode)
        {
            if (Time.time - slowModeTime > timeDelay)
            {
                slowModeTime = Time.time;
            }
            else
            {
                return;
            }
        }
        if(bs.boneList.Count > 0)
        {
            List<Vector3> axes = GetRotationAxes();
            List<Vector3> jacobian = GetJacobian();
        }
    }

    void DoDPI()
    {

    }*/

    void DoFABRIK()
    {
        if (!slowMode)
        {
            if (bs.boneList.Count > 0)
            {
                Vector3 homeLoc = Vector3.zero;
                for (int i = bs.boneList.Count - 1; i >= 0; i--)
                {
                    GameObject curBone = bs.boneList[i];
                    BoneManager cbbm = curBone.GetComponent<BoneManager>();
                    Vector3 axisToSnap;
                    Vector3 snapPos;
                    if (i == bs.boneList.Count - 1)
                    {
                        axisToSnap = targetLoc - curBone.transform.position;
                        snapPos = targetLoc;
                    }
                    else
                    {
                        GameObject nextBone = bs.boneList[i + 1];
                        axisToSnap = nextBone.transform.position - curBone.transform.position;
                        snapPos = nextBone.transform.position;
                    }
                    if(i == 0)
                    {
                        homeLoc = curBone.transform.position;
                    }
                    Vector3 eeAxis = cbbm.endEffectorWorldPos - curBone.transform.position;
                    Vector3 rotAxis = Vector3.Cross(eeAxis, axisToSnap).normalized;
                    float rotAngle = Vector3.SignedAngle(eeAxis, axisToSnap, rotAxis);
                    curBone.transform.Rotate(rotAxis, rotAngle, Space.World);

                    eeAxis = cbbm.endEffectorWorldPos - curBone.transform.position;
                    curBone.transform.position = snapPos - eeAxis;
                }
                for (int i = 0; i < bs.boneList.Count; i++)
                {
                    GameObject curBone = bs.boneList[i];
                    BoneManager cbbm = curBone.GetComponent<BoneManager>();
                    Vector3 axisToSnap = cbbm.endEffectorWorldPos - homeLoc;
                    Vector3 snapPos = homeLoc;
                    Vector3 eeAxis = cbbm.endEffectorWorldPos - curBone.transform.position;
                    Vector3 rotAxis = Vector3.Cross(eeAxis, axisToSnap).normalized;
                    float rotAngle = Vector3.SignedAngle(eeAxis, axisToSnap, rotAxis);
                    curBone.transform.Rotate(rotAxis, rotAngle, Space.World);
                    curBone.transform.position = snapPos;
                    homeLoc = cbbm.endEffectorWorldPos;
                }
            }
        }
        else
        {
            if (bs.boneList.Count > 0 && Time.time - slowModeTime > timeDelay)
            {
                slowModeTime = Time.time;
                if(bonePos < 0)
                {
                    bonePos = 0;
                    isBackprop = false;
                }
                else if(bonePos >= bs.boneList.Count)
                {
                    bonePos = bs.boneList.Count - 1;
                    isBackprop = true;
                }
                if (isBackprop)
                {
                    GameObject curBone = bs.boneList[bonePos];
                    BoneManager cbbm = curBone.GetComponent<BoneManager>();
                    Vector3 axisToSnap;
                    Vector3 snapPos;
                    if (bonePos == bs.boneList.Count - 1)
                    {
                        axisToSnap = targetLoc - curBone.transform.position;
                        snapPos = targetLoc;
                    }
                    else
                    {
                        GameObject nextBone = bs.boneList[bonePos + 1];
                        axisToSnap = nextBone.transform.position - curBone.transform.position;
                        snapPos = nextBone.transform.position;
                    }
                    if (bonePos == 0)
                    {
                        stepHomeLoc = curBone.transform.position;
                    }
                    Vector3 eeAxis = cbbm.endEffectorWorldPos - curBone.transform.position;
                    Vector3 rotAxis = Vector3.Cross(eeAxis, axisToSnap).normalized;
                    float rotAngle = Vector3.SignedAngle(eeAxis, axisToSnap, rotAxis);
                    curBone.transform.Rotate(rotAxis, rotAngle, Space.World);

                    eeAxis = cbbm.endEffectorWorldPos - curBone.transform.position;
                    curBone.transform.position = snapPos - eeAxis;
                    bonePos--;
                }
                else
                {
                    GameObject curBone = bs.boneList[bonePos];
                    BoneManager cbbm = curBone.GetComponent<BoneManager>();
                    Vector3 axisToSnap = cbbm.endEffectorWorldPos - stepHomeLoc;
                    Vector3 snapPos = stepHomeLoc;
                    Vector3 eeAxis = cbbm.endEffectorWorldPos - curBone.transform.position;
                    Vector3 rotAxis = Vector3.Cross(eeAxis, axisToSnap).normalized;
                    float rotAngle = Vector3.SignedAngle(eeAxis, axisToSnap, rotAxis);
                    curBone.transform.Rotate(rotAxis, rotAngle, Space.World);
                    curBone.transform.position = snapPos;
                    stepHomeLoc = cbbm.endEffectorWorldPos;
                    bonePos++;
                }
            }
        }
    }

    void DoCCD()
    {
        if (!slowMode)
        {
            if (bs.boneList.Count > 0)
            {
                for (int i = bs.boneList.Count - 1; i >= 0; i--)
                {
                    GameObject curBone = bs.boneList[i];
                    BoneManager cbbm = curBone.GetComponent<BoneManager>();
                    Vector3 axisToSnap = targetLoc - curBone.transform.position;
                    Vector3 eeAxis = bs.endEffectorPos - curBone.transform.position;
                    Vector3 rotAxis = Vector3.Cross(eeAxis, axisToSnap).normalized;
                    float rotAngle = Vector3.SignedAngle(eeAxis, axisToSnap, rotAxis);
                    Debug.Log(rotAngle);
                    curBone.transform.Rotate(rotAxis, rotAngle, Space.World);

                    for (int j = i; j < bs.boneList.Count - 1; j++)
                    {
                        GameObject prevBone = bs.boneList[j];
                        GameObject propBone = bs.boneList[j + 1];
                        propBone.transform.Rotate(rotAxis, rotAngle, Space.World);
                        propBone.transform.position = prevBone.GetComponent<BoneManager>().endEffectorWorldPos;
                    }
                }
            }
        }
        else
        {
            if (bs.boneList.Count > 0 && Time.time - slowModeTime > timeDelay)
            {
                slowModeTime = Time.time;
                if (bonePos < 0 || bonePos >= bs.boneList.Count)
                {
                    bonePos = bs.boneList.Count - 1;
                }
                GameObject curBone = bs.boneList[bonePos];
                BoneManager cbbm = curBone.GetComponent<BoneManager>();
                Vector3 axisToSnap = targetLoc - curBone.transform.position;
                Vector3 eeAxis = bs.endEffectorPos - curBone.transform.position;
                Vector3 rotAxis = Vector3.Cross(eeAxis, axisToSnap).normalized;
                float rotAngle = Vector3.SignedAngle(eeAxis, axisToSnap, rotAxis);
                Debug.Log(rotAngle);
                curBone.transform.Rotate(rotAxis, rotAngle, Space.World);

                for (int j = bonePos; j < bs.boneList.Count - 1; j++)
                {
                    GameObject prevBone = bs.boneList[j];
                    GameObject propBone = bs.boneList[j + 1];
                    propBone.transform.Rotate(rotAxis, rotAngle, Space.World);
                    propBone.transform.position = prevBone.GetComponent<BoneManager>().endEffectorWorldPos;
                }
                bonePos--;
            }
        }
    }
}
