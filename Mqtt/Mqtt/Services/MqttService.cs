using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mqtt;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Mqtt.Domain;

namespace Mqtt.Services
{
    public class MqttService
    {
        private IMqttClient _mqttClient;
        public event Action< MqttApplicationMessage> MessageReceived;
        public IList<MyMqttClient> Clients { get; set; }  = new List<MyMqttClient>();
        private void OnMessageReceived(MqttApplicationMessage msg)
        {
            MessageReceived?.Invoke(msg);
        }
        public MqttService()
        {

        }

        public async Task<MyMqttClient> AddBroker(string address, int port)
        {
            var client = Clients.FirstOrDefault(c => c.Id == address+":" + port);
            if(client==null)
            {

                client = new MyMqttClient(address, port);
                Clients.Add(client);
                client.MessageReceived += MessageReceived;
            }
            return client;
        }

        public async Task Subscribe(string id, string topic)
        {
            var client = Clients.First(c => c.Id == id);
            await client.Subscribe(topic);
        }

        public async Task Publish(string clientId, int qos, string topic, string message)
        {
            var msg = new MqttApplicationMessage(topic, Encoding.UTF8.GetBytes(message));

            var client = Clients.First(c => c.Id == clientId);
            MqttQualityOfService mqttQoS = MqttQualityOfService.AtMostOnce;//QoS0
            switch (qos)
            {
                case 1:
                    mqttQoS = MqttQualityOfService.AtMostOnce;//QoS1
                    break;
                case 2:
                    mqttQoS = MqttQualityOfService.ExactlyOnce;//QoS2
                    break;

            }
            await client.PublishAsync(msg, mqttQoS); 

        }
    }
}