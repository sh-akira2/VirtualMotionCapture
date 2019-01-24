using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

public class UDPReceive : MonoBehaviour
{
    public HandController handController;

    int LOCA_LPORT = 21220;
    static UdpClient udp;
    Thread thread;

    private List<int> rock = new List<int> { -90, -90, -90, 0, -90, -90, -90, 0, -90, -90, -90, 0, -90, -90, -90, 0, -90, -90, 2, 10, -90, -90, -90, 0, -90, -90, -90, 0, -90, -90, -90, 0, -90, -90, -90, 0, -90, -90, 2, 10 };
    private List<int> paper = new List<int> { 3, 0, 4, 10, 0, 4, 2, 7, 0, 5, 1, 2, -2, -1, 0, -6, 10, 10, 10, 10, 3, 0, 4, 10, 0, 4, 2, 7, 0, 5, 1, 2, -2, -1, 0, -6, 10, 10, 10, 10 };

    private List<Vector3> rockdefault = null;
    private List<Vector3> paperdefault = null;
    void Start()
    {
        udp = new UdpClient(LOCA_LPORT);
        udp.Client.ReceiveTimeout = 1000;
        thread = new Thread(new ThreadStart(ThreadMethod));
        thread.Start();
        currentAngles.AddRange(paper);
        currentAngles.AddRange(paper);
    }

    int[] currentValue = { 1, 1, 1, 1, 1 };
    int[] maxValue = { 1, 1, 1, 1, 1 };
    int[] minValue = { 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF };
    int[] rawMinValue = { 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF };

    private List<int> currentAngles = new List<int>();

    private int[] filtertmp = { 0, 0, 0, 0, 0 };
    private int[] filterdvalue = { 0, 0, 0, 0, 0 };

    private List<float> ts = new List<float>();
    void Update()
    {
        if (rockdefault == null)
        {
            rockdefault = handController.CalcHandEulerAngles(rock);
            paperdefault = handController.CalcHandEulerAngles(paper);
        }
        else
        {
            ts.Clear();
            ts.AddRange(Enumerable.Repeat(0.0f, 15));
            for (int i = 4; i >= 0; i--)
            {
                var addvalue = (float)(filtertmp[i] - minValue[i]) / (maxValue[i] - minValue[i]);
                if (float.IsNaN(addvalue)) addvalue = 0.5f;
                ts.Add(addvalue);
                ts.Add(addvalue);
                ts.Add(addvalue);
            }
            handController.SetHandEulerAngles(false, true, handController.eulersLerps(paperdefault, rockdefault, ts));
        }
        if (Time.frameCount % Application.targetFrameRate == 0)
        {
            var logtext = "";
            for (int i = 4; i >= 0; i--)
            {
                logtext += $"c:{currentValue[i]} f:{filterdvalue[i]} ft:{filtertmp[i]}";
            }
            Debug.Log(logtext);
        }
    }

    void OnApplicationQuit()
    {
        thread.Abort();
    }

    private void ThreadMethod()
    {
        while (true)
        {
            IPEndPoint remoteEP = null;
            byte[] data = udp.Receive(ref remoteEP);
            for (int i = 0; i < 5; i++)
            {
                currentValue[i] = (((int)data[i * 2 + 1] << 8) | ((int)data[i * 2])) & 0xFFFF;
                if (currentValue[i] < rawMinValue[i]) rawMinValue[i] = currentValue[i];
                if (filtertmp[i] == 0) filtertmp[i] = currentValue[i];
                filtertmp[i] = (int)(0.97 * filtertmp[i] + (1 - 0.97) * currentValue[i]);
                filterdvalue[i] = currentValue[i] - filtertmp[i] + rawMinValue[i];
                if (filtertmp[i] > maxValue[i]) maxValue[i] = filtertmp[i];
                if (filtertmp[i] < minValue[i]) minValue[i] = filtertmp[i];
            }
        }
        //Debug.Log(currentValue);
    }
}
