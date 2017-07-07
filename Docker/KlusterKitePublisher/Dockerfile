FROM clusterkit/baseweb:latest
MAINTAINER Mikhail Kantarovskiy <kantoramob@gmail.com>
EXPOSE 3090
WORKDIR /opt/clusterkit/
COPY build/ /opt/clusterkit/
COPY packageCache/ /opt/packageCache/
COPY ["fallBackConfiguration.json", "config.hocon", "./"]

RUN rm /etc/nginx/sites-enabled/default
RUN chmod 777 nuget.exe
CMD /etc/init.d/nginx start && dotnet ClusterKit.NodeManager.Launcher.dll

