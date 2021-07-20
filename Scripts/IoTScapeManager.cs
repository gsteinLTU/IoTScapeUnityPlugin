using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using Random = UnityEngine.Random;

namespace IoTScapeUnityPlugin
{
    [DefaultExecutionOrder(-50)]
    public class IoTScapeManager : MonoBehaviour
    {
        public static IoTScapeManager Manager;
        public string Host = "52.73.65.98";
        private IPAddress hostIpAddress;
        public ushort Port = 1975;

        private Socket _socket;

        private int idprefix;
        private int lastid = 0;

        private Dictionary<string, IoTScapeObject> objects = new Dictionary<string, IoTScapeObject>();
        private Dictionary<string, int> lastIDs = new Dictionary<string, int>();

        private EndPoint hostEndPoint;
        // Wait time in seconds.
        private float waitTime = 30.0f;
        private float timer = 0.0f;

        // Start is called before the first frame update
        void Start()
        {
            hostIpAddress = IPAddress.Parse(Host);
            hostEndPoint = new IPEndPoint(hostIpAddress, Port);

            idprefix = Random.Range(0, 0x10000);
            Manager = this;

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _socket.Bind(new IPEndPoint(IPAddress.Any, 0));
        }

        /// <summary>
        /// Announces all services to server
        /// </summary>
        /// <param name="o">IoTScapeObject to announce</param>
        void announce(IoTScapeObject o)
        {
            string serviceJson = JsonConvert.SerializeObject(new Dictionary<string, IoTScapeServiceDefinition>(){{o.ServiceName, o.Definition}});
            UnityEngine.Debug.Log($"Announcing service {o.ServiceName} from object with ID {o.Definition.id}");

            _socket.SendTo(serviceJson.Select(c => (byte) c).ToArray(), SocketFlags.None, hostEndPoint);
        }

        /// <summary>
        /// Announce all object-services to server
        /// </summary>
        void announceAll()
        {
            objects.Values.ToList().ForEach(announce);
        }

        /// <summary>
        /// Register an IoTScapeObject
        /// </summary>
        /// <param name="o">IoTScapeObject to register</param>
        public void Register(IoTScapeObject o)
        {
            int newID;
            string newIDString;

            string fullDeviceType = o.ServiceName;

            // Allow device types to have their own numeric ids
            if (o.DeviceTypeID.Length > 1)
            {
                fullDeviceType += ":" + o.DeviceTypeID;
            }

            if (!lastIDs.ContainsKey(fullDeviceType))
            {
                lastIDs.Add(fullDeviceType, 0);
            }

            newID = lastIDs[fullDeviceType]++;

            // Assign IDs
            if (o.DeviceTypeID != "")
            {
                newIDString = idprefix.ToString("x4") + "_" + o.DeviceTypeID + "_" + (newID).ToString("x4");
            }
            else
            {
                newIDString = idprefix.ToString("x4") + (newID).ToString("x4");
            }

            o.Definition.id = newIDString;
            objects.Add(o.ServiceName + ":" + newIDString, o);
            announce(o);
        }

        // Update is called once per frame
        void Update()
        {
            // Parse incoming messages
            if (_socket.Available > 0)
            {
                byte[] incoming = new byte[2048];
                int len = _socket.Receive(incoming);

                string incomingString = Encoding.UTF8.GetString(incoming, 0, len);

                var json = JsonSerializer.Create();
                IoTScapeRequest request = json.Deserialize<IoTScapeRequest>(new JsonTextReader(new StringReader(incomingString)));
                UnityEngine.Debug.Log(request);

                // Verify device exists
                if (objects.ContainsKey(request.service + ":" + request.device))
                {
                    var device = objects[request.service + ":" + request.device];

                    // Call function if valid
                    if (device.Methods.ContainsKey(request.function))
                    {
                        string[] result = device.Methods[request.function].Invoke(request.ParamsList.ToArray());

                        IoTScapeResponse response = new IoTScapeResponse
                        {
                            id = request.device,
                            request = request.id,
                            service = request.service,
                            response = (result ?? new string[]{}).ToList()
                        };

                        // Send response
                        string responseJson = JsonConvert.SerializeObject(response,
                            new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});

                        _socket.SendTo(responseJson.Select(c => (byte)c).ToArray(), SocketFlags.None, hostEndPoint);
                    }
                }
            }
            timer += Time.deltaTime;
            if (timer > waitTime)
            {
                announceAll();
                timer = 0.0f;
            }
        }
    }

    public class IoTScapeEventDescription
    {
        [JsonProperty(PropertyName = "params")]
        public List<string> paramsList = new List<string>();
    }

    [Serializable]
    public class IoTScapeServiceDescription
    {
        public string description;
        public string externalDocumentation;
        public string termsOfService;
        public string contact;
        public string license;
        public string version;
    }

    [Serializable]
    public class IoTScapeMethodDescription
    {
        public string documentation;

        [JsonProperty(PropertyName = "params")]
        public List<IoTScapeMethodParams> paramsList = new List<IoTScapeMethodParams>();
        public IoTScapeMethodReturns returns;
    }

    [Serializable]
    public class IoTScapeMethodParams
    {
        public string name;
        public string documentation;
        public string type;
        public bool optional;
    }

    [Serializable]
    public class IoTScapeMethodReturns
    {
        public string documentation;
        public List<string> type = new List<string>();
    }

    [Serializable]
    public class IoTScapeRequest
    {
        public string id;
        public string service;
        public string device;
        public string function;

        [JsonProperty(PropertyName = "params")]
        public List<String> ParamsList = new List<string>();

        public override string ToString()
        {
            return $"IoTScape Request #{id}: call {service}/{function} on {device} with params [{string.Join(", ", ParamsList)}]";
        }
    }

    [Serializable]
    public class IoTScapeResponse
    {
        public string id;
        public string request;
        public string service;
        [CanBeNull] public List<string> response;

        [JsonProperty(PropertyName = "event")]
        [CanBeNull] public IoTScapeEventResponse EventResponse;

        [CanBeNull] public string error;

    }

    [Serializable]
    public class IoTScapeEventResponse
    {
        public string type;
        public string args;
    }
}