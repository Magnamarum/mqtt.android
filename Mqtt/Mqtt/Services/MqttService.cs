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

namespace Mqtt.Services
{
    public class MqttService
    {
        private IMqttClient _mqttClient;
        public event Action<MqttApplicationMessage> MessageReceived;

        private void OnMessageReceived(MqttApplicationMessage msg)
        {
            MessageReceived?.Invoke(msg);
        }
        public MqttService()
        {
            Setup();
        }

        public async Task Setup()
        {
            _mqttClient = await MqttClient.CreateAsync("iot.eclipse.org", 1883);

            await _mqttClient.ConnectAsync();
            await _mqttClient.SubscribeAsync("#", MqttQualityOfService.AtMostOnce);
            
            _mqttClient
                .MessageStream
                .Subscribe(OnMessageReceived);
        }
    }
}