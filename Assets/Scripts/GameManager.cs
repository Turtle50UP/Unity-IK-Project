using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public SimManager sm;
    public GameObject target;
    public BoneSpawner bs;
    public Text infoText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    string AssembleText()
    {
        string nl = "\n";
        string introText = "Inverse Kinematics Funtime!";
        string gameMode = "Current Mode: ";
        switch (sm.simMode)
        {
            case 1:
                gameMode += "Cyclic Coordinate Descent (CCD)";
                break;
            case 2:
                gameMode += "Jacobian Transpose";
                break;
            case 3:
                gameMode += "Forward and Backward Reaching Inverse Kinematics (FABRIK)";
                break;
            default:
                gameMode += "Wut...";
                break;
        }
        string isRunning = "Running: " + sm.simStarted.ToString();
        string inStepMode = "Slow Mode: " + sm.slowMode.ToString();
        string stepSize = "Slow Mode Step Size: " + sm.timeDelay.ToString() + " seconds";
        string boneTotal = "Total Bones: " + bs.boneCount.ToString();
        string boneLength = "Bone Length: " + bs.bonesLength.ToString();
        string effectorStep = "Delta E Fraction: " + sm.delE.ToString();
        string targetPos = "Target Position: " + target.transform.position.ToString();
        string effectorPos = "Effector Position: " + bs.endEffectorPos.ToString();
        string output =
            introText + nl +
            gameMode + nl +
            isRunning + nl +
            inStepMode + nl +
            stepSize + nl +
            boneTotal + nl +
            boneLength + nl +
            effectorStep + nl +
            targetPos + nl +
            effectorPos;
        return output;
    }
            
    // Update is called once per frame
    void Update()
    {
        infoText.text = AssembleText();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
