# ClusterKit

A framework to create scalable and redundant services based on awesome [Akka.Net](https://github.com/akkadotnet/akka.net) project.

Contains several subprojects to solve most typical service creation and managing problems.

### [ClusterKit.Core](./ClusterKit.Core/Readme.md)
Packs the actor system into the executable. Provides start-up configuration and plug-in management system.

### [ClusterKit.Security](./ClusterKit.Security/Readme.md)
Provides basic abstractions for authentication and athorization in service

### [ClusterKit.Log](./ClusterKit.Log/Readme.md)
Centralized log management configuration.

### [ClusterKit.API](./ClusterKit.API/Readme.md)
Define your API for client applications / external servicies.

### [ClusterKit.LargeObjects](./ClusterKit.LargeObjects/Readme.md)
Due to performance issues internal Akka.NET messaging system has limitation on message size. Sometimes you need to pass huge ammount of data (although this should be strongly avoided). This lib provides additional functionality to pass huge amount of data between cluster nodes.

### [ClusterKit.Data](./ClusterKit.Data/Readme.md)
A bundle of generic actors and other abstractions to handle basic data work (mainly CRUD)

### [ClusterKit.Web](./ClusterKit.Web/Readme.md)
A bundle of generic actors and other abstractions to publish Web API (both REST and GraphQL). Also provides authentication and authorization for external applications.

### [ClusterKit.Monitoring](./ClusterKit.Monitoring/Readme.md)
Collecting diagnostics and monitoring information from cluster nodes.

### [ClusterKit.NodeManager](./ClusterKit.NodeManager/Readme.md)
Cluster configuration and orchestration, remote node configuration, managing and updating.

### [Sources build instructions](./BuildScript.md)
### [Sample cluster build with docker-compose](./Docker/Readme.md)
