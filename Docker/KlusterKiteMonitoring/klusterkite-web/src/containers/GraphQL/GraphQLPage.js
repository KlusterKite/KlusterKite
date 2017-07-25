import React from 'react'
import GraphiQL from 'graphiql';

import Storage from '../../utils/ttl-storage';

import 'graphiql/graphiql.css';
import './style.css';

export default class GraphQLPage extends React.Component {
  graphQLFetcher(graphQLParams) {
    const url = 'http://entry/api/1.x/graphQL';

    return fetch(url, {
      method: 'post',
      headers: { 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + Storage.get('accessToken') },
      body: JSON.stringify(graphQLParams),
    }).then(response => response.json());
  }

  render () {
    return (
      <GraphiQL fetcher={this.graphQLFetcher} />
    )
  }
}
