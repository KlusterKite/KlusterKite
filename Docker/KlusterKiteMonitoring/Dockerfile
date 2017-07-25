FROM nginx:latest
MAINTAINER Mikhail Kantarovskiy <kantoramob@gmail.com>
RUN apt-get update && \
	apt-get -y upgrade

#RUN rm /etc/nginx/sites-enabled/default
COPY klusterkite-web/.nginx.conf /etc/nginx/conf.d/default.conf
COPY klusterkite-web/build /var/www/klusterkite


