# KlusterKite.API

Define your API for client applications / external services.

As KlusterKite main idea is that you have a cluster of many nodes and that nodes can have different functions, but service needs to provide API for it's client applications.

So some of the nodes can define and process the API requests, other nodes discover the defined API and publish it to the end-users.

To define API all you need is:

1. Define the inheritor of [`ApiProvider`](../Docs/Doxygen/html/class_kluster_kite_1_1_a_p_i_1_1_provider_1_1_api_provider.html) that contains methods of the API and register it in the DI
2. Don't forget to add `KlusterKite.API.Endpoint` package to the nodes plug-in to make your API discoverable.

The API can be published as GraphQL (see [`KlusterKite.Web.GraphQL.Publisher`](../KlusterKite.Web/Readme.md)) or RESTFull api (*not realized yet*).

Please look for the [`TestProvider`](../Docs/Doxygen/html/_test_provider_8cs_source.html) as example of API definition
