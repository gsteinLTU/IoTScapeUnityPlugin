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
    }
}