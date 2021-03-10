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

        public GenericDictionary<string, IoTScapeCommandCallback> Methods;

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
                        description = "A Unity light",
                        externalDocumentation = "",
                        license = "n/a",
                        termsOfService = "n/a",
                        version = "1"
                    }
                };
            }

            Definition = Object.Instantiate(Definition);
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
            if (!registered && ServiceName.Trim().Length > 0)
            {
                IoTScapeManager.Manager.Register(this);
                registered = true;
            }
        }
        
        public void OnValidate()
        {
            if (Definition != null)
            {
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
    }

    [Serializable]
    public class IoTScapeCommandCallback : SerializableCallback<string[], string[]> { }
}
