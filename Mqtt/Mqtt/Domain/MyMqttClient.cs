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

namespace Mqtt.Domain
{
    public class MyMqttClient
    {
        private IMqttClient _mqttClient;

        public event Action<MqttApplicationMessage> MessageReceived;
        private void OnMessageReceived(MqttApplicationMessage msg)
        {
            MessageReceived?.Invoke(msg);
        }
        public string Address { get; set; }
        public int Port { get; set; }
        public string Id { get; internal set; }

        public MyMqttClient(string address, int port)
        {
            Id = address +":"+ port;
            Address = address;
            Port = port;
        }

        public async Task Connect()
        {
            _mqttClient = await MqttClient.CreateAsync(Address, Port);

            await _mqttClient.ConnectAsync();

            _mqttClient
                .MessageStream
                .Subscribe(OnMessageReceived);
        }

        public async Task Disconnect()
        {
            await _mqttClient.DisconnectAsync();
        }

        public async Task Subscribe(string topic)
        {

            await _mqttClient.SubscribeAsync(topic, MqttQualityOfService.AtMostOnce);
        }

        public async Task PublishAsync(MqttApplicationMessage message, MqttQualityOfService qos)
        {
            await _mqttClient.PublishAsync(message, qos);
        }
    }
}