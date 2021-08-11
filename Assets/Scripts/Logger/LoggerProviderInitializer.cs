using System;
using UnityEngine;

namespace Logger
{
    public class LoggerProviderInitializer : MonoBehaviour
    {
        private void Awake()
        {
            var logFileDirectory = Application.persistentDataPath + "/Logs/";
            LoggerProvider.Logger = new FileLogger($"{logFileDirectory}network_traffic_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.log");
        }
    }
}