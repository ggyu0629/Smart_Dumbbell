using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;
using UnityEngine.UI;
using SerialData;

namespace SerialData
{
    public static class InputData // 자이로 센서값
    {
        public static float battery;
        public static float x;
        public static float y;
        public static float z;
        public static float force;
    }
}


public class Serial : MonoBehaviour
{
    string str = "";
    string[] str2;

    public float tempx;
    public float tempy;
    public float tempz;

    bool offsetflag;

    public GameObject cube;

    public bool sendingFlag, flag,loginflag;
   
    int count;
    private SerialPort sp;

    public int[] data;
    public string user_nickname;
    string[] tempstr;
    public Button measureBtt;

    Queue<string> queue = new Queue<string>();

    public static Serial instance;
    public GameObject gm; // 찾는걸로 변경.


    private void Awake()
    {
        


        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this);
    }

    void Start()
    {
        tempx = 0;
        tempy = 0;
        tempz = 0;

        offsetflag = false;
        loginflag = false;
        
        ConnectSerial();

    }

    public void ConnectSerial() // 검색해서 자동 연결
    {
        string[] ports = SerialPort.GetPortNames();
        foreach (string p in ports)
        {
            sp = new SerialPort(p, 115200, Parity.None, 8, StopBits.One); // 초기화

            try
            {
                sp.WriteTimeout = 1000;
   
                sp.Open(); // 프로그램 시작시 포트 열기

                sp.Write("AB5");//50 Hz로 세팅


            }
            catch (Exception ex)
            {
                Debug.Log(ex);
                continue;
            }
            
            if (sp.ReadExisting().Equals(""))
            {
                continue;
            }
            else break;
        }
        
        
    }
    void Update()
    {
        /*if (loginflag)//로그인에 성공하면 연결.
        {
            gm = GameObject.Find("Canvas");
            //GameObject.Find("nickname").GetComponent<Text>().text = user_nickname + " 님";
        }*/
        if (sendingFlag)
        {
            MySerialReceived();
        }


        if (Input.GetKeyDown(KeyCode.O))
        {
            SerialSendingOffset();
        }

        if (Input.GetKeyDown(KeyCode.E))//시작
        {
            print("asd");
            SerialSendinggyro();
        }
        if (Input.GetKeyDown(KeyCode.T))//정지
        {
            SerialSendingStop();
        }

        if(Input.GetKeyDown(KeyCode.H))//악략일때
        {
            SerialSendingGrip();
        }
        
       
       print(InputData.x + " " + InputData.y + " " + InputData.z );

        //cube.transform.rotation = Quaternion.Euler(new Vector3(0, InputData.y, InputData.z));



        //print(InputData.y);
    }
    private void MySerialReceived()  //자이로 센서값(x ,y) 가공
    {
        // 인큐 과정
        string tmp = sp.ReadExisting(); //업데이트 마다 현재 입력 버퍼에서 가져옴
        //print(tmp);
        str2 = tmp.Split('\n');
        
        foreach(string s in str2)
        {
            if (s.Replace("\r", "").Replace("\n", "").Equals(""))
                continue;
            queue.Enqueue(s);
            GetDataFromQueue();
        }

    }

    
    void GetDataFromQueue() //디큐 과정
    {

        if (queue.Any()) // 큐안에 값이 존재한다면.
        {
            if (str == "") 
                str = queue.Dequeue();
                
            else if (!str.Contains('a'))
            {
                str = "";
            }

            if (str.Contains('a') && str.Contains('b'))
            {
                dataSave();
            }
            else if(queue.Any())
            {
                str += queue.Dequeue();
            }
        }

    }
    
    void dataSave()
    {

        tempstr = str.Split(','); //저장전 가공
        //print(tempstr[0] + " " + tempstr[1] + " " + tempstr[2] + " " + tempstr[3] + " " + tempstr[4] + " " + tempstr[5] + " " + tempstr[6]);
        try
        {
            InputData.battery = float.Parse(tempstr[1]);
            InputData.x = float.Parse(tempstr[2]) - tempx;
            InputData.y = float.Parse(tempstr[3]) - tempy;
            InputData.z = float.Parse(tempstr[4]) - tempz;
            InputData.force = float.Parse(tempstr[5]);


        }
        catch (Exception ex)
        {

        }

       // print(tempstr[0] + " " + tempstr[1] + " " + tempstr[2] + " " + tempstr[3] + " " + tempstr[4] + " " + tempstr[5] + " " + tempstr[6]);
        
         //데이터 저장


        /*if (flag) // 시리얼 디버그
        {
            PrintGyro();
        }
        else
        {
            PrintGrip();
        }*/

        str = "";
    }

    

    //동작이 시작할 때 오프셋 잡아야함.
    //***********시리얼 센서제어 및 센서 값 Debug*********************
    public void SerialSendinggyro()//유니티 -> Serial (E)전송
    {
        sp.Write("ABE");
        sendingFlag = true;
        flag = true;
        
    }

    public void SerialSendingGrip()//악력값 받기
    {
        sp.Write("ABH");
        sp.Write("ABO");
        sendingFlag = true;
        flag = false;

    }

    public void SerialSendingStop()//멈추기
    {
        sp.Write("ABT");
        //Debug.Log("정지");
        sendingFlag = false;

    }

    public void SerialSendingOffset()//값 리셋
    {
        tempx += InputData.x;
        tempy += InputData.y;
        tempz += InputData.z;

        print("asd");
        offsetflag = true;
        sendingFlag = true;
    }

/*    public void PrintGyro()
    {
        gm.transform.GetChild(1).GetComponent<Text>().text = "X : " + InputData.x;
        gm.transform.GetChild(2).GetComponent<Text>().text = "Y : " + InputData.y;
        gm.transform.GetChild(4).GetComponent<Text>().text = "배터리 : " + InputData.battery;
    }
    public void PrintGrip()
    {
        gm.transform.GetChild(3).GetComponent<Text>().text = "악력 : " + InputData.force;

    }*/



    //***********시리얼 센서제어 및 센서 값 Debug*********************

    private void OnApplicationQuit()
    {
        sp.Close(); // 프로그램 종료시 포트 닫기
    }
}
