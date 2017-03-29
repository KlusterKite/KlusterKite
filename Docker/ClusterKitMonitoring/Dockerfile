FROM nginx:latest
MAINTAINER Mikhail Kantarovskiy <kantoramob@gmail.com>
RUN apt-get update && \
	apt-get -y upgrade

#RUN rm /etc/nginx/sites-enabled/default
COPY clusterkit-web/.nginx.conf /etc/nginx/conf.d/default.conf
COPY clusterkit-web/build /var/www/clusterkit
COPY clusterkit-web/.env-build /var/www/clusterkit/.env



