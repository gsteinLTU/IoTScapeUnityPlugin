using System;
using System.Collections.Generic;
using UnityEngine;

namespace IoTScapeUnityPlugin
{
    public class IoTScapeObject : MonoBehaviour
    {
        [HideInInspector]
        public IoTScapeServiceDefinition Definition = new IoTScapeServiceDefinition()
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

        private bool registered = false;

        public string ServiceName = "IoTScapeService";

        public Dictionary<string, Func<string[], string[]>> RegisteredMethods = new Dictionary<string, Func<string[], string[]>>();

        // Start is called before the first frame update
        void Start()
        {
            RegisteredMethods.Add("heartbeat", strings => new[]{"true"} );
        }

        // Update is called once per frame
        void Update()
        {
            if (!registered)
            {
                IoTScapeManager.Manager.Register(this);
                registered = true;
            }    
        }

        public void RegisterMethod(string name, Func<string[], string[]> method, IoTScapeMethodDescription description)
        {
            Definition.methods.Add(name, description);
            RegisteredMethods.Add(name, method);
        }
    }
}
