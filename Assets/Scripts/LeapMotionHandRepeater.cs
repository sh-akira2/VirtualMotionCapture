using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeapMotionHandRepeater : MonoBehaviour
{

    public HandController handController;
    public RiggedHand leftHand;
    public RiggedHand rightHand;

    public int[] sels;
    public float[] axis;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var eulers = new List<Vector3>();
        for (int f = 4; f >= 0; f--)
        {
            for (int b = 3; b >= 1; b--)
            {
                var orig = leftHand.fingers[f].bones[b].localEulerAngles;
                if (f == 0)
                {
                    if (b == 1)
                    {
                        eulers.Add(Vector3.zero);
                    }
                    else
                    {
                        var origarr = new float[] { orig.x, orig.y, orig.z };
                        origarr[0] = origarr[0] * axis[0];
                        origarr[1] = origarr[1] * axis[1];
                        origarr[2] = origarr[2] * axis[2];
                        eulers.Add(new Vector3(origarr[sels[0]], origarr[sels[1]], origarr[sels[2]]));
                    }
                }
                else
                {
                    eulers.Add(new Vector3(-orig.x, -orig.y, -orig.z));
                }
            }
        }
        for (int f = 4; f >= 0; f--)
        {
            for (int b = 3; b >= 1; b--)
            {
                var orig = rightHand.fingers[f].bones[b].localEulerAngles;
                if (f == 0)
                {
                    if (b == 1)
                    {
                        eulers.Add(Vector3.zero);
                    }
                    else
                    {
                        var origarr = new float[] { orig.x, orig.y, orig.z };
                        origarr[0] = origarr[0] * axis[0];
                        origarr[1] = origarr[1] * axis[1];
                        origarr[2] = origarr[2] * -axis[2];
                        eulers.Add(new Vector3(origarr[sels[0]], origarr[sels[1]], origarr[sels[2]]));
                    }
                }
                else
                {
                    eulers.Add(new Vector3(orig.x, orig.y, orig.z));
                }
            }
        }
        handController.SetHandEulerAngles(true, true, eulers);
    }
}
