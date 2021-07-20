using System;
using UnityEngine;

namespace IoTScapeUnityPlugin
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/IoTScape Service Definition", order = 1)]
    [Serializable]
    public class IoTScapeServiceDefinition : ScriptableObject
    {
        public IoTScapeServiceDescription service;
        public string id;
        public GenericDictionary<string, IoTScapeMethodDescription> methods = new GenericDictionary<string, IoTScapeMethodDescription>();
        public GenericDictionary<string, IoTScapeEventDescription> events = new GenericDictionary<string, IoTScapeEventDescription>();

        public void OnValidate()
        {
            // Update version when changed
            if(int.TryParse(service.version, out int versionNumber))
            {
                service.version = (versionNumber + 1).ToString();
            } 
        }
    }
}