# ClusterKit

Bundle of utils to create scalable and redundant services.
Based on https://github.com/akkadotnet/akka.net
Contains several subprojects to solve most typical service creation problems.

## [ClusterKit.Core] (./ClusterKit.Core/Readme.md)
Some most basic abstractions to assemble, configure and run service executable.

## [ClusterKit.Log] (./ClusterKit.Log/Readme.md)
Centralized log writing configuration.

## [ClusterKit.LargeObjects] (./ClusterKit.LargeObjects/Readme.md)
Due to performance issue internal Akka.NET messaging system has limitation on message size. Sometimes you need to pass huge ammount of data (although this should be strongly avoided). This lib provides additional functionality to pass huge amount of data between cluster nodes.

## [ClusterKit.Data] (./ClusterKit.Data/Readme.md)
A bundle of generic actors and other abstractions to handle basic data work (ORM)

## [ClusterKit.Web] (./ClusterKit.Web/Readme.md)
A bundle of generic actors and other abstractions to provide Web API (web - actor system integration)

## [ClusterKit.Monitoring] (./ClusterKit.Monitoring/Readme.md)
Collecting diagnostics and monitoring information from cluster nodes.

## [ClusterKit.NodeManager] (./ClusterKit.NodeManager/Readme.md)
Cluster orchestration, remote node configuration, managing and updating.

