using System;
using System.Net.Mqtt;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Mqtt.Services;
using System.Collections.Generic;
using Android.Content.PM;

namespace Mqtt
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
    {
        private MqttService _mqttService;
        private IList<string> _items;
        private ArrayAdapter<string> _listAdapter;
        ListView lista;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            _mqttService = new MqttService();
            lista = (ListView)FindViewById<ListView>(Resource.Id.listView);
            _items = new List<string>();
            _mqttService.MessageReceived += msg => RunOnUiThread(() =>
            {
                MqttServiceOnMessageReceived(msg);
            });
            _listAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, _items);
            lista.Adapter = _listAdapter;
        }

        private void MqttServiceOnMessageReceived(MqttApplicationMessage mqttApplicationMessage)
        {

            // _listAdapter.NotifyDataSetChanged();

            if (_listAdapter.Count < 10)
            {
                var msg = mqttApplicationMessage.Topic + ": " + System.Text.Encoding.UTF8.GetString(mqttApplicationMessage.Payload);
                //_items.Add(mqttApplicationMessage.Topic + ": " + System.Text.Encoding.UTF8.GetString(mqttApplicationMessage.Payload));

                //_listAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, _items);
                //lista.Adapter = _listAdapter;
                _listAdapter.Add(msg);
                _listAdapter.NotifyDataSetChanged();

            }


        }
    }
}