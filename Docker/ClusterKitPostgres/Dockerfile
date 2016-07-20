FROM postgres:9.5.3
MAINTAINER Mikhail Kantarovskiy <kantoramob@gmail.com>
RUN export DEBIAN_FRONTEND=noninteractive && \
	apt-get update && \
	apt-get -y upgrade
RUN echo listen_addresses = '*' >> /var/lib/postgresql/data/postgresql.conf

