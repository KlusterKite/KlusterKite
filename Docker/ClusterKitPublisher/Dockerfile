FROM clusterkit/baseweb:latest
MAINTAINER Mikhail Kantarovskiy <kantoramob@gmail.com>
EXPOSE 3090
WORKDIR /opt/clusterkit/
COPY build/* /opt/clusterkit/
COPY packageCache/* /opt/packageCache/
COPY ["fallBackConfiguration.json", "ClusterKit.NodeManager.Launcher.exe.config", "./"]

RUN rm /etc/nginx/sites-enabled/default
RUN chmod 777 nuget.exe
CMD /etc/init.d/nginx start && mono ClusterKit.NodeManager.Launcher.exe

