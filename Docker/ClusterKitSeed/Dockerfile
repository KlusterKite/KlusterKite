FROM clusterkit/baseweb:latest
MAINTAINER Mikhail Kantarovskiy <kantoramob@gmail.com>
EXPOSE 3090
WORKDIR /opt/clusterkit/
COPY build/* /opt/clusterkit/
COPY packageCache/* /opt/packageCache/
COPY ["fallBackConfiguration.json", "ClusterKit.NodeManager.Launcher.exe.config", "./"]
COPY web/monitoring /opt/web/monitoring
RUN rm /etc/nginx/sites-enabled/default
CMD /etc/init.d/nginx start && mono ClusterKit.NodeManager.Launcher.exe

