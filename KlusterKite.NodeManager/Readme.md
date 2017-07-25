# KlusterKite.NodeManager

Cluster configuration and orchestration, remote node configuration, managing, and updating.
  
## Aim

We have some *system* that is located on a bunch of servers/VMs/Containers (from now on, this documentation will call it **container**). These containers can join and leave the system without the disturbance of the service. We should have an easy way to deploy new features and services, bug fixes to the whole cluster with minimum manual work (let assume that there are a huge amount of containers in our **system**. We have a lot of **nodes** and resources. The new containers should be easily introduced into the cluster. We should have an ability to quickly reconfigure any node, to redistribute roles among containers if needed.

Some of the containers are persistent (that holds the DB, storage data, endpoints, e.t.c), some are not and should be easily added and removed to scale performance / reduce hosting cost according to current **system** load.

## Glossary

* **System** - the application in the broadest sense (including DBMS, web-sites e.t.c.)
* **Node** - the server application node that paticipates in Akka.NET cluster
* **Resource** - the external (from Akka.NET cluster point of view) part of an application (like DB, web-site, e.t.c) that should be updated with the .net code synchroniously.

## Node container configuration

To store all executed code **KlusterKite** based **system** should have a private NuGet server as part of the cluster. It is used to store and distribute code across all nodes that are going to join the cluster. The malfunction of NuGet server will not halt the **system** work but will prevent the new nodes start.

Each container, intended to run some of the **systems** code should have a preinstalled **KlusterKite.NodeManager.Launcher** service, that should start on container start. This service is rather lightweight and is supposed to be updated very rarely. It's the only purpose to request the node configuration from the **system**, download and extract needed packages from the NuGet server, create the [`KlusterKite.Core.Service`](../KlusterKite.Core/Readme.md), add the top-level configuration and launch it. In the case of the service stop - it restarts the whole cycle from the beginning. This service has some configuration parameters that are stored in `config.hocon`:
* `NodeManagerUrl` - the endpoint (URL) of `KlusterKite.NodeManager` configuration API
* `authenticationUrl` - the endpoint to authenticate in **system** to access API
* `apiClientId` and `apiClientSecret` - the authentication credentials to authenticate in **system** to access API
* `runtime` - the description of the container runtime (see [RID](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog))
* `containerType` - the symbolic description of the current container type. Not all containers are identical. The can have different hardware parameters or different preinstalled third-party software or else. The received configuration depends on container type
* `fallbackConfiguration` - the path to the fallback configuration file (in JSON serialized format) that will be used in case of whole **system** down. That is used only on global **system** start-up. This configuration is also embedded in the container.

In order to make things work (to distribute configurations to the starting **nodes**), there should be always some working **node** with `KlusterKite.NodeManager` plugin that is correctly published to the **system** endpoint (see [`KlusterKite.Web`](../KlusterKite.Web/Readme.md))

## Cluster `Configuration` and `Migrations`

### Node template
In order to define the configuration that is sended to `KlusterKite.NodeManager.Launcher` **KlusterKite** introduces the [`NodeTemplate`](../Docs/Doxygen/html/class_kluster_kite_1_1_node_manager_1_1_client_1_1_o_r_m_1_1_node_template.html) entity.

When `NodeManager` receives a new configuration request it selects the template in following order (the template should have the `containerType` among it's `ContainerTypes`):
1. If there are templates with less active nodes then `MinimumRequiredInstances` it will apply one of them in the `Priority` order (from highest to lowest)
2. If there are templates with less active nodes then `MaximumNeededInstances` (or `MaximumNeededInstances` is `null`) it will apply one of them in the `Priority` order (from highest to lowest)
3. Otherwise, it will send a special signal, so none of the templates will be applied and `KlusterKite.NodeManager.Launcher` will wait for some time to repeat the request.

The node template (aka the node configuration) includes the following information:
* The list of NuGet packages (along with their exact versions) to be installed
* The top-level configuration (that overrides any parameter from plugins default configuration)

## Cluster Configuration
The list of all **Node templates** along with some other parameters is called **Cluster Configuration** or just [`Configuration`](../Docs/Doxygen/html/class_kluster_kite_1_1_node_manager_1_1_client_1_1_o_r_m_1_1_configuration.html).

The special parameters are:
* **Packages** - the list of all (with direct or indirect references) used NuGet packages and their versions. `NodeTemplate` defines only the list of plugin packages (with optional version, if omitted the version from configuration packages list will be used) and optionally special packages and their version if they are not specified or another version in cluster configuration packages list
* **SeedAddresses** - the list of Akka.NET Cluster seed nodes that are used as Cluster Seeds (or lighthouse) to let the new node join Akka.NET Cluster. Please check the Akka.NET Cluster documentation.
* **NugetFeed** - the address of the **system** NuGet server to acquire the packages
* **Migrator templates** - the migrator templates are described below

## Migrations

There can be any number of defined **configurations**, but only one can be used at a time (the one that has `Active` state). The *Active* configuration cannot be changed and is immutabel. The process of switching from one configuration to another is called: [`Migration`](../Docs/Doxygen/html/class_kluster_kite_1_1_node_manager_1_1_client_1_1_o_r_m_1_1_migration.html).

But during the migration process, not only **nodes** are needed to be upgraded, but also **resources**. Some of them, like DB schemas, are needed to be updated before **nodes** (`CodeDependsOnResource` dependence type), others - like web sites that use the **system** API - after the **nodes** (`ResourceDependsOnCode` dependence type). And if the **system** has a large amount of **resources** it is hard to make adjustments manually and it is more reliable to have this adjustment to be scripted and distributed among all code so the developers can be sure that **resources** and **nodes** are of the same version.

To provide the automation of this processes there are [`MigratorTemplates`](../Docs/Doxygen/html/class_kluster_kite_1_1_node_manager_1_1_client_1_1_o_r_m_1_1_migrator_template.html) and **Migrators** entities.

The **MigratorTemplate** is much alike **NodeTemplate** and defined in a similiar way in the configuration. `KlusterKite.NodeManager` has a cluster singletone that assembles and launches the `KlusterKite.NodeManager.Migrator.Executor` service assembled based on `MigratorTemplates` configuration. The top-level configuration of the template should have `KlusterKite.NodeManager.Migrators` string array that contains the list of type names of **Migrators** to be executed. The **Migrator** is a class that implements [`IMigrator`](../Docs/Doxygen/html/interface_kluster_kite_1_1_node_manager_1_1_migrator_1_1_i_migrator.html) interface.

The resource migration model was copied from Entity Framework Code-First migrations. The resource should have some chronological states (called **migration points**) and **migrator** should be able to change states from one to another. It is assumed, that **migrator** should be able to revers changes to any state in the past and can upgrade the resource from any past state to current state.

If there are no active migrations, `KlusterKite.NodeManager` will launch all defined `MigratorTemplates` and their migrators to assure that all defined resources are existing and in the state of last defined migration point. If everything is ok the new migration can be created.

After migration is created the `MigratorTemplates` and their migrators are executed for both old and new configurations to check the resource changes. If the list of migration points for some **Migrator** of new configuration starts with all points of old configuration and have some new one - it is considered as resource upgrade. If the list of migration points for some **Migrator** of old configuration starts with all points of the new configuration and have some extra points - it is considered as resource downgrade (it can happen in the case of system update rollback, when the previous version is installed). 

The migration is executed in the following steps:
1. All upgrading or creating resources with `CodeDependsOnResource` dependence type and all downgrading resources with `ResourceDependsOnCode` dependence type should be adjusted. In the case of butch resource migration, the resources are migrated in following order: the downgraded resources are migrated first, then resources are migrated in the **Migrator** priority order (`asc` for downgrade and `desc` for upgrade).
2. All nodes should be adjusted. This process is performed automatically. Only those node that has changes in packages definitions and/or configuration will be updated. During the update process the `KlusterKite.NodeManager` will assure that there will be no moment when the **system** will have less active nodes of `NodeTemplate` that is defined in `MinimumRequiredInstances` to maintain the zero time **system** work interruption.
3. All upgrading or creating resources with `ResourceDependsOnCode` dependence type and all downgrading resources with `CodeDependsOnResource` dependence type should be adjusted. In the case of butch resource migration, the resources are migrated in following order: the downgraded resources are migrated first, then resources are migrated in the **Migrator** priority order (`asc` for downgrade and `desc` for upgrade).

The migration step execution is controlled via API or UI.
There is UI that provides access to the `KlusterKite.NodeManager` API. Please check the sample [`Docker`](../Docker/Readme.md) documentation.


## Seeders

In order to provide easy sandbox start-up, KlusterKite has **seeder** function to create the resources from the scratch. The sandbox should have a container with preinstalled and configured `KlusterKite.NodeManager.Seeder.Launcher` utility. This utility will read it's configuration and start the specified **Seeders** (that inherits the [`BaseSeeder`](../Docs/Doxygen/html/class_kluster_kite_1_1_node_manager_1_1_migrator_1_1_base_seeder.html) class). Every seeder should check for resource pre-existence to avoid generating errors in case of a subsequent run.

Please check the [`Docker`](../Docker/Readme.md) example of the confiugured seeder.