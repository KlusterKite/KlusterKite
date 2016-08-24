# we'll need to set the right version here
FROM kylemanna/openvpn:latest
MAINTAINER Mikhail Kantarovskiy <kantoramob@gmail.com>
RUN apk update && \
	apk -q -f upgrade
COPY ./OpenVpn /etc/openvpn
COPY ./OpenVpn /etc/openvpn.orig
#CMD cp /etc/openvpn.orig/openvpn.conf /etc/openvpn/ && openvpn --config /etc/openvpn/openvpn.conf
CMD cp /etc/openvpn.orig/openvpn.conf /etc/openvpn/ && ovpn_run



