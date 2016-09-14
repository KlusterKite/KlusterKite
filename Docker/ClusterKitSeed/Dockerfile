FROM clusterkit/baseworker:latest
MAINTAINER Mikhail Kantarovskiy <kantoramob@gmail.com>
EXPOSE 3090
WORKDIR /opt/clusterkit/
COPY build/* /opt/clusterkit/
COPY akka.hocon /opt/clusterkit/
CMD mono ClusterKit.Core.Service.exe

