FROM clusterkit/basenuget:latest
MAINTAINER Mikhail Kantarovskiy <kantoramob@gmail.com>

WORKDIR /opt/
COPY packages/* /opt/packages/
COPY nuget.exe /opt/
COPY loadPackages.sh /opt/
RUN service php5-fpm start && /etc/init.d/nginx start && bash loadPackages.sh

