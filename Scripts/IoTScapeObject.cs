using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IoTScapeUnityPlugin
{
    public class IoTScapeObject : MonoBehaviour
    {
        public IoTScapeServiceDefinition Definition;

        private bool registered = false;

        public string ServiceName = "IoTScapeService";

        public string DeviceTypeID = "";

        public string IDOverride = "";

        public GenericDictionary<string, IoTScapeCommandCallback> Methods;

        internal string ID;

        [Tooltip("Set to false to prevent automatically registering the object with the IoTScapeManager")]
        public bool ShouldRegister = true;

        // Start is called before the first frame update
        void Start()
        {
            if (Definition == null)
            {
                // Default definition
                Definition = new IoTScapeServiceDefinition()
                {
                    service = new IoTScapeServiceDescription()
                    {
                        contact = "gstein@ltu.edu",
                        description = "A Unity service",
                        externalDocumentation = "",
                        license = "n/a",
                        termsOfService = "n/a",
                        version = "1"
                    }
                };
            }

            Definition = Instantiate(Definition);
            Definition.id = "";

            // Register default heartbeat
            Methods.Add("heartbeat", new IoTScapeCommandCallback());
            Methods["heartbeat"].SetMethod(this, "heartbeat", true);
        }

        /// <summary>
        /// Respond to the server with a heartbeat
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private String[] heartbeat(String[] args)
        {
            return new[] { "true" };
        }

        // Update is called once per frame
        void Update()
        {
            if (!registered && ShouldRegister && ServiceName.Trim().Length > 0)
            {
                ID = IoTScapeManager.Manager.Register(this);
                registered = true;
            }
        }
        
        public void OnValidate()
        {
            if (Definition != null)
            {
                // Replace default name with ID of service definition
                if (Definition != null && (ServiceName == "" || ServiceName == "IoTScapeService"))
                {
                    ServiceName = Definition.name;
                }

                // Populate Methods list
                foreach (var method in Definition.methods.Keys)
                {
                    if (!Methods.ContainsKey(method))
                    {
                        Methods.Add(method, new IoTScapeCommandCallback());
                    }
                }
            }
        }

        /// <summary>
        /// Send an event for this device to the server
        /// </summary>
        /// <param name="eventType">Type of event, should be in service definition</param>
        /// <param name="args">Data to be passed with event</param>
        public void SendEvent(string eventType, string[] args)
        {
            IoTScapeResponse eventResponse = new IoTScapeResponse
            {
                id = ID,
                service = ServiceName,
                request = "",
                EventResponse = new IoTScapeEventResponse()
                {
                    type = eventType,
                    args = args
                }
            };

            IoTScapeManager.Manager.SendToServer(eventResponse);
        }

        /// <summary>
        /// Send an event with no arguments
        /// </summary>
        /// <param name="EventType">Type of event, should be in service definition</param>
        public void SendEvent(string EventType)
        {
            SendEvent(EventType, new string[]{});
        }
    }

    [Serializable]
    public class IoTScapeCommandCallback : SerializableCallback<string[], string[]> { }
}
