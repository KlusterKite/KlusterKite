FROM clusterkit/baseworker:latest
MAINTAINER Mikhail Kantarovskiy <kantoramob@gmail.com>
WORKDIR /opt/clusterkit/
COPY build/* /opt/clusterkit/
COPY packageCache/* /opt/packageCache/
COPY ["ClusterKit.NodeManager.Launcher.exe.config", "./"]
RUN chmod 777 nuget.exe
CMD bash ./start.sh


