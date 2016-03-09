FROM readytalk/nodejs

WORKDIR /app
ADD package.json /app/
RUN npm install
ADD . /app

CMD []
ENTRYPOINT ["/nodejs/bin/npm", "start"]
