FROM klusterkite/baseworker:latest
MAINTAINER Mikhail Kantarovskiy <kantoramob@gmail.com>
EXPOSE 3090
WORKDIR /opt/klusterkite/
COPY build/ /opt/klusterkite/
COPY akka.hocon /opt/klusterkite/
CMD dotnet KlusterKite.Core.Service.dll

