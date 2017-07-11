# KlusterKite.LargeObjects

Due to performance issues internal Akka.NET messaging system has a limitation on message size. Sometimes you need to pass a huge amount of data (although this should be strongly avoided). This lib provides additional functionality to pass a huge amount of data between cluster nodes.

In order to use this, you will need the `KlusterKite.LargeObjects.Client` to receive huge messages and `KlusterKite.LargeObjects` to send and receive them.


In order to send huge method the [`Parcel`](../Docs/Doxygen/html/class_kluster_kite_1_1_large_objects_1_1_parcel.html) should be formed and passed to `ParcelManager` like this:
```csharp
Context.GetParcelManager().Tell(new Parcel { Payload = hugeMessage, Recipient = recepient }, sender);
```

Where:
* `hugeMessage` is some large message that you can't pass normally
* `recipient` - is `ICanTell`, the address of receiver actor
* `sender` - is `IActorRef`, the back address of sender (usually is `this.Self`)

Optional `StoreTimeout` property of `Parcel` can be specified to handle the delivery problems. The parcel is stored locally until it will be received or timeout occurred.

The receiver will get the [`ParcelNotification`](../Docs/Doxygen/html/class_kluster_kite_1_1_large_objects_1_1_client_1_1_parcel_notification.html) message. The end-message type can be checked via `PayloadTypeName` property and/or `GetPayloadType()` method. The parcel payload then can be accessed with async `Receive` method of the notification.
