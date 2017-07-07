import React from 'react';
import { storiesOf, action } from '@kadira/storybook';
import StubContainer from 'react-storybooks-relay-container';

import ActorsTree from './tree';
import ActorsTreeButtons from './buttons';

storiesOf('Actor tree')
  .add('graph', () => {
    const data = getScanResult();
    return <ActorsTree tree={data.data.api.clusterKitMonitoringApi.getClusterTree.nodes.edges} />;
  })
  // .add('graph', () => {
  //   const data = getScanResult();
  //
  //   const props = {
  //     data: data.data.api.clusterKitMonitoringApi
  //   };
  //   return <StubContainer Component={ActorsTree} props={props} />;
  // })
  .add('buttons', () => {
    return <ActorsTreeButtons isLoading={false} handleReload={action('handleReload')} handleScan={action('handleScan')} />;
  })
  .add('buttons loading', () => {
    return <ActorsTreeButtons isLoading={true} handleReload={action('handleReload')} handleScan={action('handleScan')} />;
  })
;

const getScanResult = () => {
  return {
    "data": {
      "api": {
        "clusterKitMonitoringApi": {
          "getClusterTree": {
            "nodes": {
              "edges": [
                {
                  "node": {
                    "value": {
                      "name": "clusterManager-v0 172.18.0.10:40679",
                      "actorType": null,
                      "dispatcherType": null,
                      "currentMessage": null,
                      "queueSize": 0,
                      "queueSizeSum": 4,
                      "maxQueueSize": 4,
                      "children": {
                        "edges": [
                          {
                            "node": {
                              "name": "system",
                              "actorType": "Akka.Actor.SystemGuardianActor",
                              "dispatcherType": "Dispatcher",
                              "currentMessage": null,
                              "queueSize": 0,
                              "queueSizeSum": 4,
                              "maxQueueSize": 4,
                              "children": {
                                "edges": [
                                  {
                                    "node": {
                                      "name": "cluster",
                                      "actorType": "Akka.Cluster.ClusterDaemon",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "clusterEventBusListener",
                                      "actorType": "Akka.Cluster.ClusterReadView+EventBusListener",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "endpointManager",
                                      "actorType": "Akka.Remote.EndpointManager",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 4,
                                      "queueSizeSum": 4,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "EventStreamUnsubscriber-1",
                                      "actorType": "Akka.Event.EventStreamUnsubscriber",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "log1-SerilogLogger",
                                      "actorType": "Akka.Logger.Serilog.SerilogLogger",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "remote-deployment-watcher",
                                      "actorType": "Akka.Remote.RemoteDeploymentWatcher",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "remote-watcher",
                                      "actorType": "Akka.Cluster.ClusterRemoteWatcher",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "remoting-terminator",
                                      "actorType": "Akka.Remote.RemoteActorRefProvider+RemotingTerminator",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "transports",
                                      "actorType": "Akka.Remote.TransportSupervisor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  }
                                ]
                              }
                            }
                          },
                          {
                            "node": {
                              "name": "user",
                              "actorType": "Akka.Actor.GuardianActor",
                              "dispatcherType": "Dispatcher",
                              "currentMessage": null,
                              "queueSize": 0,
                              "queueSizeSum": 0,
                              "maxQueueSize": 0,
                              "children": {
                                "edges": [
                                  {
                                    "node": {
                                      "name": "ClusterKit",
                                      "actorType": "ClusterKit.Core.NameSpaceActor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "Core",
                                      "actorType": "ClusterKit.Core.NameSpaceActor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "Monitoring",
                                      "actorType": "ClusterKit.Core.NameSpaceActor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "NodeManager",
                                      "actorType": "ClusterKit.Core.NameSpaceActor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "Web",
                                      "actorType": "ClusterKit.Core.NameSpaceActor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  }
                                ]
                              }
                            }
                          }
                        ]
                      }
                    }
                  }
                },
                {
                  "node": {
                    "value": {
                      "name": "clusterManager-v0 172.18.0.11:39664",
                      "actorType": null,
                      "dispatcherType": null,
                      "currentMessage": null,
                      "queueSize": 0,
                      "queueSizeSum": 0,
                      "maxQueueSize": 0,
                      "children": {
                        "edges": [
                          {
                            "node": {
                              "name": "system",
                              "actorType": "Akka.Actor.SystemGuardianActor",
                              "dispatcherType": "Dispatcher",
                              "currentMessage": null,
                              "queueSize": 0,
                              "queueSizeSum": 0,
                              "maxQueueSize": 0,
                              "children": {
                                "edges": [
                                  {
                                    "node": {
                                      "name": "cluster",
                                      "actorType": "Akka.Cluster.ClusterDaemon",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "clusterEventBusListener",
                                      "actorType": "Akka.Cluster.ClusterReadView+EventBusListener",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "endpointManager",
                                      "actorType": "Akka.Remote.EndpointManager",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "EventStreamUnsubscriber-1",
                                      "actorType": "Akka.Event.EventStreamUnsubscriber",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "log1-SerilogLogger",
                                      "actorType": "Akka.Logger.Serilog.SerilogLogger",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "remote-deployment-watcher",
                                      "actorType": "Akka.Remote.RemoteDeploymentWatcher",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "remote-watcher",
                                      "actorType": "Akka.Cluster.ClusterRemoteWatcher",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "remoting-terminator",
                                      "actorType": "Akka.Remote.RemoteActorRefProvider+RemotingTerminator",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "transports",
                                      "actorType": "Akka.Remote.TransportSupervisor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  }
                                ]
                              }
                            }
                          },
                          {
                            "node": {
                              "name": "user",
                              "actorType": "Akka.Actor.GuardianActor",
                              "dispatcherType": "Dispatcher",
                              "currentMessage": null,
                              "queueSize": 0,
                              "queueSizeSum": 0,
                              "maxQueueSize": 0,
                              "children": {
                                "edges": [
                                  {
                                    "node": {
                                      "name": "ClusterKit",
                                      "actorType": "ClusterKit.Core.NameSpaceActor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "Core",
                                      "actorType": "ClusterKit.Core.NameSpaceActor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "Monitoring",
                                      "actorType": "ClusterKit.Core.NameSpaceActor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "NodeManager",
                                      "actorType": "ClusterKit.Core.NameSpaceActor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "Web",
                                      "actorType": "ClusterKit.Core.NameSpaceActor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  }
                                ]
                              }
                            }
                          }
                        ]
                      }
                    }
                  }
                },
                {
                  "node": {
                    "value": {
                      "name": "seed:3090",
                      "actorType": null,
                      "dispatcherType": null,
                      "currentMessage": null,
                      "queueSize": 0,
                      "queueSizeSum": 0,
                      "maxQueueSize": 0,
                      "children": {
                        "edges": [
                          {
                            "node": {
                              "name": "system",
                              "actorType": "Akka.Actor.SystemGuardianActor",
                              "dispatcherType": "Dispatcher",
                              "currentMessage": null,
                              "queueSize": 0,
                              "queueSizeSum": 0,
                              "maxQueueSize": 0,
                              "children": {
                                "edges": [
                                  {
                                    "node": {
                                      "name": "cluster",
                                      "actorType": "Akka.Cluster.ClusterDaemon",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "clusterEventBusListener",
                                      "actorType": "Akka.Cluster.ClusterReadView+EventBusListener",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "deadLetterListener",
                                      "actorType": "Akka.Event.DeadLetterListener",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "endpointManager",
                                      "actorType": "Akka.Remote.EndpointManager",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "EventStreamUnsubscriber-1",
                                      "actorType": "Akka.Event.EventStreamUnsubscriber",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "log1-SerilogLogger",
                                      "actorType": "Akka.Logger.Serilog.SerilogLogger",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "remote-deployment-watcher",
                                      "actorType": "Akka.Remote.RemoteDeploymentWatcher",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "remote-watcher",
                                      "actorType": "Akka.Cluster.ClusterRemoteWatcher",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "remoting-terminator",
                                      "actorType": "Akka.Remote.RemoteActorRefProvider+RemotingTerminator",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "transports",
                                      "actorType": "Akka.Remote.TransportSupervisor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  }
                                ]
                              }
                            }
                          },
                          {
                            "node": {
                              "name": "user",
                              "actorType": "Akka.Actor.GuardianActor",
                              "dispatcherType": "Dispatcher",
                              "currentMessage": null,
                              "queueSize": 0,
                              "queueSizeSum": 0,
                              "maxQueueSize": 0,
                              "children": {
                                "edges": [
                                  {
                                    "node": {
                                      "name": "ClusterKit",
                                      "actorType": "ClusterKit.Core.NameSpaceActor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "Core",
                                      "actorType": "ClusterKit.Core.NameSpaceActor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "Monitoring",
                                      "actorType": "ClusterKit.Core.NameSpaceActor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  }
                                ]
                              }
                            }
                          }
                        ]
                      }
                    }
                  }
                },
                {
                  "node": {
                    "value": {
                      "name": "publisher-v0 publisher1:40079",
                      "actorType": null,
                      "dispatcherType": null,
                      "currentMessage": null,
                      "queueSize": 0,
                      "queueSizeSum": 0,
                      "maxQueueSize": 0,
                      "children": {
                        "edges": [
                          {
                            "node": {
                              "name": "system",
                              "actorType": "Akka.Actor.SystemGuardianActor",
                              "dispatcherType": "Dispatcher",
                              "currentMessage": null,
                              "queueSize": 0,
                              "queueSizeSum": 0,
                              "maxQueueSize": 0,
                              "children": {
                                "edges": [
                                  {
                                    "node": {
                                      "name": "cluster",
                                      "actorType": "Akka.Cluster.ClusterDaemon",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "clusterEventBusListener",
                                      "actorType": "Akka.Cluster.ClusterReadView+EventBusListener",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "deadLetterListener",
                                      "actorType": "Akka.Event.DeadLetterListener",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "endpointManager",
                                      "actorType": "Akka.Remote.EndpointManager",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "EventStreamUnsubscriber-1",
                                      "actorType": "Akka.Event.EventStreamUnsubscriber",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "log1-SerilogLogger",
                                      "actorType": "Akka.Logger.Serilog.SerilogLogger",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "remote-deployment-watcher",
                                      "actorType": "Akka.Remote.RemoteDeploymentWatcher",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "remote-watcher",
                                      "actorType": "Akka.Cluster.ClusterRemoteWatcher",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "remoting-terminator",
                                      "actorType": "Akka.Remote.RemoteActorRefProvider+RemotingTerminator",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "transports",
                                      "actorType": "Akka.Remote.TransportSupervisor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  }
                                ]
                              }
                            }
                          },
                          {
                            "node": {
                              "name": "user",
                              "actorType": "Akka.Actor.GuardianActor",
                              "dispatcherType": "Dispatcher",
                              "currentMessage": null,
                              "queueSize": 0,
                              "queueSizeSum": 0,
                              "maxQueueSize": 0,
                              "children": {
                                "edges": [
                                  {
                                    "node": {
                                      "name": "ClusterKit",
                                      "actorType": "ClusterKit.Core.NameSpaceActor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "Core",
                                      "actorType": "ClusterKit.Core.NameSpaceActor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "Monitoring",
                                      "actorType": "ClusterKit.Core.NameSpaceActor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "NodeManager",
                                      "actorType": "ClusterKit.Core.NameSpaceActor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "Web",
                                      "actorType": "ClusterKit.Core.NameSpaceActor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  }
                                ]
                              }
                            }
                          }
                        ]
                      }
                    }
                  }
                },
                {
                  "node": {
                    "value": {
                      "name": "publisher-v0 publisher2:33168",
                      "actorType": null,
                      "dispatcherType": null,
                      "currentMessage": null,
                      "queueSize": 0,
                      "queueSizeSum": 0,
                      "maxQueueSize": 0,
                      "children": {
                        "edges": [
                          {
                            "node": {
                              "name": "system",
                              "actorType": "Akka.Actor.SystemGuardianActor",
                              "dispatcherType": "Dispatcher",
                              "currentMessage": null,
                              "queueSize": 0,
                              "queueSizeSum": 0,
                              "maxQueueSize": 0,
                              "children": {
                                "edges": [
                                  {
                                    "node": {
                                      "name": "cluster",
                                      "actorType": "Akka.Cluster.ClusterDaemon",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "clusterEventBusListener",
                                      "actorType": "Akka.Cluster.ClusterReadView+EventBusListener",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "deadLetterListener",
                                      "actorType": "Akka.Event.DeadLetterListener",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "endpointManager",
                                      "actorType": "Akka.Remote.EndpointManager",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "EventStreamUnsubscriber-1",
                                      "actorType": "Akka.Event.EventStreamUnsubscriber",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "log1-SerilogLogger",
                                      "actorType": "Akka.Logger.Serilog.SerilogLogger",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "remote-deployment-watcher",
                                      "actorType": "Akka.Remote.RemoteDeploymentWatcher",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "remote-watcher",
                                      "actorType": "Akka.Cluster.ClusterRemoteWatcher",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "remoting-terminator",
                                      "actorType": "Akka.Remote.RemoteActorRefProvider+RemotingTerminator",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "transports",
                                      "actorType": "Akka.Remote.TransportSupervisor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  }
                                ]
                              }
                            }
                          },
                          {
                            "node": {
                              "name": "user",
                              "actorType": "Akka.Actor.GuardianActor",
                              "dispatcherType": "Dispatcher",
                              "currentMessage": null,
                              "queueSize": 0,
                              "queueSizeSum": 0,
                              "maxQueueSize": 0,
                              "children": {
                                "edges": [
                                  {
                                    "node": {
                                      "name": "ClusterKit",
                                      "actorType": "ClusterKit.Core.NameSpaceActor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "Core",
                                      "actorType": "ClusterKit.Core.NameSpaceActor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "Monitoring",
                                      "actorType": "ClusterKit.Core.NameSpaceActor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "NodeManager",
                                      "actorType": "ClusterKit.Core.NameSpaceActor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  },
                                  {
                                    "node": {
                                      "name": "Web",
                                      "actorType": "ClusterKit.Core.NameSpaceActor",
                                      "dispatcherType": "Dispatcher",
                                      "currentMessage": null,
                                      "queueSize": 0,
                                      "queueSizeSum": 0,
                                      "maxQueueSize": 0
                                    }
                                  }
                                ]
                              }
                            }
                          }
                        ]
                      }
                    }
                  }
                }
              ]
            }
          }
        }
      }
    }
  }
};
