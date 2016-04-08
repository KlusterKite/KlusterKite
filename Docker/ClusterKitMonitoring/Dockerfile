FROM clusterkit/baseworker
RUN apt-get install --no-install-recommends -y -q curl python build-essential git ca-certificates xz-utils
RUN mkdir /nodejs && curl https://nodejs.org/dist/v4.2.2/node-v4.2.2-linux-x64.tar.xz | tar xvJf - -C /nodejs --strip-components=1

ENV PATH $PATH:/nodejs/bin

WORKDIR /app
COPY ./web/package.json /app/
RUN npm -g install npm@2.1.1
RUN npm install
COPY ./web /app
EXPOSE 8080

ENTRYPOINT ["/nodejs/bin/npm", "run", "dev"]
