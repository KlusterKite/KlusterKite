# we'll need to set the right version here
FROM redis:latest
MAINTAINER Mikhail Kantarovskiy <kantoramob@gmail.com>
RUN apt-get update && \
	apt-get -y upgrade
ENTRYPOINT sysctl vm.overcommit_memory=1 && sysctl -w net.core.somaxconn=65535 && echo never > /sys/kernel/mm/transparent_hugepage/enabled && redis-server --protected-mode no


