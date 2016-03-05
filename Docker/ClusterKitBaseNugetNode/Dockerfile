FROM clusterkit/baseweb:latest
MAINTAINER Mikhail Kantarovskiy <kantoramob@gmail.com>
RUN apt-get update && apt-get install -y \
	git \
	php5-fpm \
	sqlite3 \
	php5-sqlite

RUN rm /etc/nginx/sites-enabled/default

WORKDIR /opt/
RUN git clone https://github.com/Daniel15/simple-nuget-server
COPY php.ini /etc/php5/fpm/php.ini
COPY nuget.conf /etc/nginx/sites-enabled/ 
RUN chown www-data:www-data /opt/simple-nuget-server/db /opt/simple-nuget-server/packagefiles
ENTRYPOINT service php5-fpm start && /etc/init.d/nginx start && touch /var/log/nginx/error.log && tail -f /var/log/nginx/error.log

