# ClusterKit.Core
Packs the actor system into the executable. Provides start-up configuration and plug-in management system.

The main idea is to get rid of writing custom service start-up code and concentrate on business logic. 

By default you have the `ClusterKit.Core.Service` that will start the almost empty actor system, configure logging (with use of [Serilog](https://serilog.net/)) and dependency injection (with use of [Autofac](https://autofac.org/)).
All you need is to extend the service functions by providing the plugin. Just put your plugin dll (and all it's dependencies) in the service directory and it will be loaded automatically on service start. Also you can provide end-level configuration with additional `akka.hocon` file. 

### Plugin description

#### Plan
* BasInstaller (the main methods)
* Config loading
* Service start
* NamespaceActor
* TestKit - BaseActorTest

The ClusterKit plugin is just normal class library with foloing requirements:
* It should be dependent of `ClusterKit.Core` package
* It should contain single class inherited from `ClusterKit.Core.BaseInstaller`
