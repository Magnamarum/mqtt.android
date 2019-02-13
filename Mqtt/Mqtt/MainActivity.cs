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
using System.Threading.Tasks;
using System.Linq;
using Mqtt.Domain;

namespace Mqtt
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
    {
        private MqttService _mqttService;
        private ArrayAdapter<string> _messageListAdapter;
        private ListView _messageList;
        private ListView _brokerList;
        private ArrayAdapter<string> _brokerListAdapter;
        private Button _buttonEditBroker;
        private Button _buttonManage;
        private MyMqttClient _currentClient;
        private int _currentView;
        private EditText _editTextAddress;
        private EditText _editTextPort;
        private Button _buttonAddBroker;
        private Button _buttonSubscribe;
        private EditText _editTextTopic;
        private Button _buttonGoToSendMessage;
        private Spinner _qosSpinner;
        private Spinner _brokerSpinner;
        private EditText _editTextSendTopic;
        private EditText _editTextMessage;
        private Button _buttonSendMessage;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _mqttService = new MqttService();

            SetUpMain();
        }

        private void SetUpMain()
        {
            _currentView = Resource.Layout.activity_main;
            SetContentView(_currentView);
            _messageList = (ListView)FindViewById<ListView>(Resource.Id.listView);
            _mqttService.MessageReceived += msg => RunOnUiThread(() =>
            {
                MqttServiceOnMessageReceived(msg);
            });
            _messageListAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1);
            _messageList.Adapter = _messageListAdapter;
            _buttonManage = FindViewById<Button>(Resource.Id.buttonManageBrokers);
            _buttonManage.Click += ButtonManage_Click;
            _buttonGoToSendMessage = FindViewById<Button>(Resource.Id.buttonGoToSendMessage);
            _buttonGoToSendMessage.Click += _buttonGoToSendMessage_Click;
        }

        private void _buttonGoToSendMessage_Click(object sender, EventArgs e)
        {
            SetUpSendMessage();
        }

        private void SetUpSendMessage()
        {
            _currentView = Resource.Layout.send_message;
            SetContentView(_currentView);

            _qosSpinner = FindViewById<Spinner>(Resource.Id.spinnerQoS);

            var qosAdapter = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.QoS_list, Android.Resource.Layout.SimpleSpinnerItem);

            qosAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            _qosSpinner.Adapter = qosAdapter;


            _brokerSpinner = FindViewById<Spinner>(Resource.Id.spinnerBrokers);
            var brokerSpinnerAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, _mqttService.Clients.Select(x => x.Id).ToList());

            brokerSpinnerAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            _brokerSpinner.Adapter = brokerSpinnerAdapter;

            _editTextSendTopic = FindViewById<EditText>(Resource.Id.editTextSendTopic);

            _editTextMessage = FindViewById<EditText>(Resource.Id.editTextMessage);

            _buttonSendMessage = FindViewById<Button>(Resource.Id.buttonSendMessage);
            _buttonSendMessage.Click += _buttonSendMessage_Click;

        }

        private void _buttonSendMessage_Click(object sender, EventArgs e)
        {
            var qos = int.Parse(_qosSpinner.SelectedItem.ToString());
            var clientId = _brokerSpinner.SelectedItem.ToString();
            var topic = _editTextSendTopic.Text;
            var message = _editTextMessage.Text;
            Publish(clientId, qos, topic, message);
        }

        private async Task Publish(string clientId, int qos, string topic, string message)
        {

            await _mqttService.Publish(clientId, qos, topic, message);

            SetUpMain();
        }

        public override void OnBackPressed()
        {
            switch (_currentView)
            {
                case Resource.Layout.activity_main:
                    Finish();
                    break;
                case Resource.Layout.manage_brokers:
                case Resource.Layout.send_message:
                    SetUpMain();
                    break;
                case Resource.Layout.edit_broker:
                case Resource.Layout.subscribe_topic:
                    SetUpManageBrokers();
                    break;



            }
        }

        private void ButtonManage_Click(object sender, EventArgs e)
        {
            SetUpManageBrokers();
        }

        private void SetUpManageBrokers()
        {
            _currentView = Resource.Layout.manage_brokers;
            SetContentView(_currentView);
            _brokerList = (ListView)FindViewById<ListView>(Resource.Id.listViewBrokers);
            _brokerListAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, _mqttService.Clients.Select(cli => cli.Id).ToList());
            _brokerList.Adapter = _brokerListAdapter;          
            _buttonEditBroker = FindViewById<Button>(Resource.Id.buttonEditBroker);
            _buttonEditBroker.Click += ButtonAddBroker_Click;
            _brokerList.ItemClick += _brokerList_ItemClick;

        }

        private void _brokerList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            _currentClient = _mqttService.Clients[e.Position];
            _currentView = Resource.Layout.subscribe_topic;
            SetContentView(_currentView);
            _buttonSubscribe = FindViewById<Button>(Resource.Id.buttonSubscribeTopic);
            _editTextTopic = FindViewById<EditText>(Resource.Id.editTextTopic);
            _buttonSubscribe.Click += _buttonSubscribe_Click;
        }

        private void _buttonSubscribe_Click(object sender, EventArgs e)
        {
            var topic = _editTextTopic.Text;
            Subscribe(topic);
        }

        private async Task Subscribe(string topic)
        {
            await _currentClient.Subscribe(topic);

            Toast.MakeText(this, "Subscribed to "+topic, ToastLength.Short);
            SetUpManageBrokers();

        }
        private void ButtonAddBroker_Click(object sender, EventArgs e)
        {
            _currentView = Resource.Layout.edit_broker;
            SetContentView(_currentView);
            _editTextAddress = FindViewById<EditText>(Resource.Id.editTextAddress);
            _editTextPort = FindViewById<EditText>(Resource.Id.editTextPort);
            _buttonAddBroker = FindViewById<Button>(Resource.Id.buttonAddBroker);
            _buttonAddBroker.Click += _buttonAddBroker_Click;
        }

        private void _buttonAddBroker_Click(object sender, EventArgs e)
        {
            AddBroker();

        }

        private async Task AddBroker()
        {
            var address = _editTextAddress.Text;
            var port = int.Parse(_editTextPort.Text);
            var client = await _mqttService.AddBroker(address, port);
            await client.Connect();

            //await client.Subscribe("#");
            Toast.MakeText(this, "Broked added", ToastLength.Short);
            SetUpManageBrokers();
        }


        private void MqttServiceOnMessageReceived(MqttApplicationMessage mqttApplicationMessage)
        {

            // _listAdapter.NotifyDataSetChanged();

            if (_messageListAdapter.Count < 20)
            {
                var msg = mqttApplicationMessage.Topic + ": " + System.Text.Encoding.UTF8.GetString(mqttApplicationMessage.Payload);
                //_items.Add(mqttApplicationMessage.Topic + ": " + System.Text.Encoding.UTF8.GetString(mqttApplicationMessage.Payload));

                //_listAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, _items);
                //lista.Adapter = _listAdapter;
                _messageListAdapter.Add(msg);
                _messageListAdapter.NotifyDataSetChanged();

            }


        }
    }
}