FROM clusterkit/baseweb:latest
MAINTAINER Mikhail Kantarovskiy <kantoramob@gmail.com>
EXPOSE 3060
WORKDIR /opt/clusterkit/
COPY build/* /opt/clusterkit/
RUN /etc/init.d/nginx start
ENTRYPOINT mono ClusterKit.Core.Service.exe

