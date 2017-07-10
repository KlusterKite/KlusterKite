# KlusterKite

A framework to create scalable and redundant services based on awesome [Akka.Net](https://github.com/akkadotnet/akka.net) project.

Documentation and UI is under constraction. Will be delivered soon...

Contains several subprojects to solve most typical service creation and managing problems.

### [KlusterKite.Core](./KlusterKite.Core/Readme.md)
Packs the actor system into the executable. Provides start-up configuration and plug-in management system.

### [KlusterKite.Security](./KlusterKite.Security/Readme.md)
Provides basic abstractions for authentication and athorization in service

### [KlusterKite.Log](./KlusterKite.Log/Readme.md)
Centralized log management configuration.

### [KlusterKite.API](./KlusterKite.API/Readme.md)
Define your API for client applications / external servicies.

### [KlusterKite.LargeObjects](./KlusterKite.LargeObjects/Readme.md)
Due to performance issues internal Akka.NET messaging system has limitation on message size. Sometimes you need to pass huge ammount of data (although this should be strongly avoided). This lib provides additional functionality to pass huge amount of data between cluster nodes.

### [KlusterKite.Data](./KlusterKite.Data/Readme.md)
A bundle of generic actors and other abstractions to handle basic data work (mainly CRUD)

### [KlusterKite.Web](./KlusterKite.Web/Readme.md)
A bundle of generic actors and other abstractions to publish Web API (both REST and GraphQL). Also provides authentication and authorization for external applications.

### [KlusterKite.Monitoring](./KlusterKite.Monitoring/Readme.md)
Collecting diagnostics and monitoring information from cluster nodes.

### [KlusterKite.NodeManager](./KlusterKite.NodeManager/Readme.md)
Cluster configuration and orchestration, remote node configuration, managing and updating.

### [Sources build instructions](./BuildScript.md)
### [Sample cluster build with docker-compose](./Docker/Readme.md)
