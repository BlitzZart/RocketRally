using System;
using System.Collections;
using UnityEngine;

public class StartupOptions : MonoBehaviour
{
    public static bool isHeadlessServer = false;
    [SerializeField] private bool m_useLocalServer = false;

    //207.154.233.46 - digital ocean
    private void Start()
    {
        print("Starting up");

        bool isServer = false;
        string ip = "0.0.0.0";

        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-server")
            {
                isServer = true;
            }
            else if (args[i] == "-ip")
            {
                if (args.Length > i + 1)
                {
                    ip = args[i + 1];
                }
            }
        }

        ConnectionModeScript cms = FindObjectOfType<ConnectionModeScript>();
        if (isServer)
        {
            if (cms)
            {
                cms.StartServer(true, ip);
                isHeadlessServer = true;
            }
        }
        else if (m_useLocalServer)
        {
            if (cms)
            {
                cms.SetIp("127.0.0.1");
            }
        }

    }
}