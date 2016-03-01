FROM clusterkit/baseweb:latest
MAINTAINER Mikhail Kantarovskiy <kantoramob@gmail.com>
EXPOSE 3090
WORKDIR /opt/clusterkit/
COPY build/* /opt/clusterkit/
COPY web/monitoring /opt/web/monitoring
RUN rm /etc/nginx/sites-enabled/default
ENTRYPOINT /etc/init.d/nginx start && mono ClusterKit.Core.Service.exe

