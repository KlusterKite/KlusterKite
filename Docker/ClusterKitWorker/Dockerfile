FROM clusterkit/baseworker:latest
MAINTAINER Mikhail Kantarovskiy <kantoramob@gmail.com>
WORKDIR /opt/clusterkit/
COPY build/* /opt/clusterkit/
COPY preinstalled /opt/preinstalled/
COPY packageCache/* /opt/packageCache/
COPY start.sh /opt/clusterkit/
ENTRYPOINT ls -lah . && bash ./start.sh


