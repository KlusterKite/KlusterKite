FROM clusterkit/baseworker
RUN apt-get install --no-install-recommends -y -q curl python build-essential git ca-certificates
RUN mkdir /nodejs && curl http://nodejs.org/dist/v0.10.36/node-v0.10.36-linux-x64.tar.gz | tar xvzf - -C /nodejs --strip-components=1

ENV PATH $PATH:/nodejs/bin

WORKDIR /app
COPY ./web/package.json /app/
RUN npm install
COPY ./web /app
EXPOSE 8080

ENTRYPOINT ["/nodejs/bin/npm", "start"]
