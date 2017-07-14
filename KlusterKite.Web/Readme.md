# KlusterKite.Web
A bundle of generic actors and other abstractions to publish Web API (both REST and GraphQL). Also provides authentication and authorization for external applications.

## Publishing API for client applications and Web

We have an interesting problem. From the one hand we want flexible, fault-tolerant application cluster where nodes are going up and down, upgrading, changing there roles and e.t.c. From the other we have client applications that need fixed API access points with well known addresses.

How this problem is solved in **KlusterKite**:
![service run](./Docs/Network.png "Web network")

For example we have some API nodes that publishes their part of an API, some content or else. They can go up and down in any moment, get new API, remove old and e.t.c.

Then we need several nodes, that have fixed addresses and doesn't need frequent update. They are aware of cluster state, the API nodes and the API itself. They will distribute requests to the API node. We call them `Publisher nodes`

And an entry point. This can be a CDN like CloudFlare, some hosting provider solution or else. All we need is reliability, ability to work as NLB and fault tolerance. It should proxy all requests to the list of fixed `Publisher nodes` nodes.

To make your API discoverable by `Publisher nodes` the `API node` should have `KlusterKite.Web.Descriptor` plugin installed and well configured.
Example configuration:
```
{
	KlusterKite {
		Web {
			Services {
				KlusterKite/Monitoring  { // ServiceName is just unique service identification, used in order to handle stacked config properly. It is used just localy on node
				Port = 8080 // default port, current node listening port for server access
				PublicHostName = default //public host name of this service. It doesn't supposed (but is not prohibited) that this should be real public service hostname. It's just used to distinguish services with identical url paths to be correctly published on frontend web servers. Real expected hostname should be configured in NginxConfigurator or similar publisher
				Route = "/api/1.x/klusterkite/monitoring" //route (aka directory) path to service
				}                    
		}
	}
}
```

As for now `Publisher node` can be created with use of `KlusterKite.Web.NginxConfigurator` plugin. It assuems that there is an installed dedicated `Nginx` service on this node. The `Nginx` will make actual proxing and the node will make a dynamic nginx reconfiguration from the current cluster state.

An example `KlusterKite.Web.NginxConfigurator` configuration:
```
{
  KlusterKite {
    Web {
      Nginx {
        PathToConfig = "/etc/nginx/sites-enabled/klusterkite.config"
        ReloadCommand {
          Command = /etc/init.d/nginx
          Arguments = reload
        } 
        Configuration {
          default { // here can be defined static paths
            "location /klusterkite" { 
              proxy_pass = "http://monitoringUI/klusterkite"
            }
          }
        }
      }
    }
  }
} 	
``` 