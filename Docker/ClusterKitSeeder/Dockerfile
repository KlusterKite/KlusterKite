FROM clusterkit/baseworker:latest
MAINTAINER Mikhail Kantarovskiy <kantoramob@gmail.com>
EXPOSE 3090
WORKDIR /opt/clusterkit/
COPY build/ /opt/clusterkit/
COPY seeder.hocon /opt/clusterkit/
CMD dotnet ClusterKit.NodeManager.Seeder.Launcher.dll

