import React from 'react';
import { storiesOf, action } from '@kadira/storybook';
// import StubContainer from 'react-storybooks-relay-container';

import ActorsTree from './index';
import ActorsTreeButtons from './buttons';

storiesOf('Actor tree')
  .add('graph', () => {
    const scanResult = getScanResult();
    return <ActorsTree tree={scanResult} />;
  })
  .add('buttons', () => {
    return <ActorsTreeButtons isLoading={false} handleReload={action('handleReload')} handleScan={action('handleScan')} />;
  })
  .add('buttons loading', () => {
    return <ActorsTreeButtons isLoading={true} handleReload={action('handleReload')} handleScan={action('handleScan')} />;
  })
;

let getScanResult = function () {
  const tree = {
    "Nodes": {
      "clusterManager-v0 172.18.0.10:36125": {
        "ActorType": null,
        "CurrentMessage": null,
        "DispatcherId": null,
        "DispatcherType": null,
        "MaxQueueSize": 0,
        "Name": "clusterManager-v0 172.18.0.10:36125",
        "QueueSize": 0,
        "QueueSizeSum": 0,
        "Children": [
          {
            "ActorType": "Akka.Actor.SystemGuardianActor",
            "CurrentMessage": null,
            "DispatcherId": "akka.actor.default-dispatcher",
            "DispatcherType": "Dispatcher",
            "MaxQueueSize": 0,
            "Name": "system",
            "QueueSize": 0,
            "QueueSizeSum": 0,
            "Children": [
              {
                "ActorType": "Akka.Cluster.ClusterDaemon",
                "CurrentMessage": null,
                "DispatcherId": "akka.actor.default-dispatcher",
                "DispatcherType": "Dispatcher",
                "MaxQueueSize": 0,
                "Name": "cluster",
                "QueueSize": 0,
                "QueueSizeSum": 0,
                "Children": [
                  {
                    "ActorType": "Akka.Cluster.ClusterCoreSupervisor",
                    "CurrentMessage": null,
                    "DispatcherId": "akka.actor.default-dispatcher",
                    "DispatcherType": "Dispatcher",
                    "MaxQueueSize": 0,
                    "Name": "core",
                    "QueueSize": 0,
                    "QueueSizeSum": 0,
                    "Children": []
                  },
                  {
                    "ActorType": "Akka.Cluster.ClusterHeartbeatReceiver",
                    "CurrentMessage": null,
                    "DispatcherId": "akka.actor.default-dispatcher",
                    "DispatcherType": "Dispatcher",
                    "MaxQueueSize": 0,
                    "Name": "heartbeatReceiver",
                    "QueueSize": 0,
                    "QueueSizeSum": 0,
                    "Children": []
                  }
                ]
              },
              {
                "ActorType": "Akka.Cluster.ClusterReadView+EventBusListener",
                "CurrentMessage": null,
                "DispatcherId": "akka.actor.default-dispatcher",
                "DispatcherType": "Dispatcher",
                "MaxQueueSize": 0,
                "Name": "clusterEventBusListener",
                "QueueSize": 0,
                "QueueSizeSum": 0,
                "Children": []
              },
              {
                "ActorType": "Akka.Event.DeadLetterListener",
                "CurrentMessage": null,
                "DispatcherId": "akka.actor.default-dispatcher",
                "DispatcherType": "Dispatcher",
                "MaxQueueSize": 0,
                "Name": "deadLetterListener",
                "QueueSize": 0,
                "QueueSizeSum": 0,
                "Children": []
              },
              {
                "ActorType": "Akka.Remote.EndpointManager",
                "CurrentMessage": null,
                "DispatcherId": "akka.remote.default-remote-dispatcher",
                "DispatcherType": "Dispatcher",
                "MaxQueueSize": 0,
                "Name": "endpointManager",
                "QueueSize": 0,
                "QueueSizeSum": 0,
                "Children": [
                  {
                    "ActorType": "Akka.Remote.ReliableDeliverySupervisor",
                    "CurrentMessage": null,
                    "DispatcherId": "akka.remote.default-remote-dispatcher",
                    "DispatcherType": "Dispatcher",
                    "MaxQueueSize": 0,
                    "Name": "reliableEndpointWriter-akka.tcp%3A%2F%2FClusterKit%40172.18.0.11%3A41011-4",
                    "QueueSize": 0,
                    "QueueSizeSum": 0,
                    "Children": [
                      {
                        "ActorType": "Akka.Remote.EndpointWriter",
                        "CurrentMessage": null,
                        "DispatcherId": "akka.remote.default-remote-dispatcher",
                        "DispatcherType": "Dispatcher",
                        "MaxQueueSize": 0,
                        "Name": "endpointWriter",
                        "QueueSize": 0,
                        "QueueSizeSum": 0,
                        "Children": [
                          {
                            "ActorType": "Akka.Remote.EndpointReader",
                            "CurrentMessage": null,
                            "DispatcherId": "akka.remote.default-remote-dispatcher",
                            "DispatcherType": "Dispatcher",
                            "MaxQueueSize": 0,
                            "Name": "endpointReader-akka.tcp%3A%2F%2FClusterKit%40172.18.0.11%3A41011-1",
                            "QueueSize": 0,
                            "QueueSizeSum": 0,
                            "Children": []
                          }
                        ]
                      }
                    ]
                  }
                ]
              },
              {
                "ActorType": "Akka.Event.EventStreamUnsubscriber",
                "CurrentMessage": null,
                "DispatcherId": "akka.actor.default-dispatcher",
                "DispatcherType": "Dispatcher",
                "MaxQueueSize": 0,
                "Name": "EventStreamUnsubscriber-1",
                "QueueSize": 0,
                "QueueSizeSum": 0,
                "Children": []
              }
            ]
          }
        ]
      },
      "seed:3090": {
        "ActorType": null,
        "CurrentMessage": null,
        "DispatcherId": null,
        "DispatcherType": null,
        "MaxQueueSize": 0,
        "Name": "seed:3090",
        "QueueSize": 0,
        "QueueSizeSum": 0,
        "Children": [
          {
            "ActorType": "Akka.Actor.SystemGuardianActor",
            "CurrentMessage": null,
            "DispatcherId": "akka.actor.default-dispatcher",
            "DispatcherType": "Dispatcher",
            "MaxQueueSize": 0,
            "Name": "system",
            "QueueSize": 0,
            "QueueSizeSum": 0,
            "Children": []
          }
        ]
      }
    },
    "QueueSizeSum": 0,
    "MaxQueueSize": 0
  };
  return tree;
};
