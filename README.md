# mqtt.android

Domain class MyMqttClient is based on Xamarin Mqtt Client https://github.com/xamarin/mqtt
this wraps connect, disconected, publish and subscribe method, and also provides an event that will be trigged when receiving every message from a subscription.

Mqtt logic is placed at MqttService, which can be found at Services folder on the project.

This class contains the following key methods:
AddBroker(string address, int port)
AddBroker method creates a client with the given address and port and add it to a Client list (Clients)

Subscribe(string id, string topic)
Retrieves the client with the given id and subscribe to a topic on that client


Publish(string clientId, int qos, string topic, string message)
Retrieves the client with the given id, sets up a message with the given QoS, topic and message and sends it.
