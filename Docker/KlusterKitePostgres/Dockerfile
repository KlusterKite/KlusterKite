FROM postgres:9.5.3
MAINTAINER Mikhail Kantarovskiy <kantoramob@gmail.com>
RUN export DEBIAN_FRONTEND=noninteractive && \
	apt-get update && \
	apt-get -y upgrade
RUN echo en_US.UTF-8 UTF-8 > /etc/locale.gen && /usr/sbin/locale-gen
COPY updateConf.sh ./docker-entrypoint-initdb.d/
RUN chmod 777 ./docker-entrypoint-initdb.d/updateConf.sh


