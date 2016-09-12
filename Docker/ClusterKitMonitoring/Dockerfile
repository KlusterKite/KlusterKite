FROM nginx:latest
MAINTAINER Mikhail Kantarovskiy <kantoramob@gmail.com>
RUN apt-get update && \
	apt-get -y upgrade

#RUN rm /etc/nginx/sites-enabled/default
COPY web/app/.nginx.conf /etc/nginx/conf.d/default.conf
COPY web/build /var/www/clusterkit




