using System;
using System.Collections;
using UnityEngine;

public class StartupOptions : MonoBehaviour
{
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

        if (isServer)
        {
            ConnectionModeScript cms = FindObjectOfType<ConnectionModeScript>();
            if (cms)
            {
                cms.StartServer(true, ip);
            }
        }
    }
}