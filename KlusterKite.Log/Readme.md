# KlusterKite.Log

Centralized log management configuration.

KlusterKite uses `Serilog` to mange the logs record. Every plugin can enrich log with additional data and/or configure `Serilog` sinks for proper log storage.

In order to perform configuration plugin should have [`ILoggerConfigurator`](../Docs/Doxygen/html/interface_kluster_kite_1_1_core_1_1_log_1_1_i_logger_configurator.html) implementation and register it with the DI. There can be any number of log configurators and they are all work together.

There are two predefined log sinks out of the box:
* `KlusterKit.Log.Console` that outputs log records to the service console. Please look for the sample [configuration](./KlusterKite.Log.Console/Resources/akka.hocon)
* `KlusterKite.Log.ElasticSearch` that puts log records to the ElasticSearch service. Please look for the sample [configuration](./KlusterKite.Log.ElasticSearch/Resources/akka.hocon)

Please note, that `KlusterKite.Log.ElasticSearch` devides log records for regular ones and security audit logs to put them in sepparte sinks. This can be used as reference.
