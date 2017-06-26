FROM ubuntu:14.04
MAINTAINER Mikhail Kantarovskiy <kantoramob@gmail.com>
RUN apt-get update && apt-get install -y apt-transport-https
RUN sh -c 'echo "deb [arch=amd64] https://apt-mo.trafficmanager.net/repos/dotnet-release/ trusty main" > /etc/apt/sources.list.d/dotnetdev.list' && \
	apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 417A0893 && \
	apt-get update && \
	apt-get -y upgrade && \
	apt-get install -y dotnet-dev-1.0.4


