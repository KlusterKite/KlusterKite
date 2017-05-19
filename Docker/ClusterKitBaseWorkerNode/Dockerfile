FROM ubuntu:14.04
MAINTAINER Mikhail Kantarovskiy <kantoramob@gmail.com>
RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
RUN echo "deb http://download.mono-project.com/repo/debian wheezy/snapshots/4.8.1 main" | sudo tee /etc/apt/sources.list.d/mono-xamarin.list
RUN apt-get update && \
	apt-get -y upgrade \
	&& apt-get install -y \
		mono-complete


