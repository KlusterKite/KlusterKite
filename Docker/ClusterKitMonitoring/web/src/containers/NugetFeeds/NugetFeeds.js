import React, { Component } from 'react';
import NugetFeedsList from './NugetFeedsList';

export default class NugetFeeds extends Component {
  render() {
    return (
        <div className="container">
          <h1>Nuget Feeds</h1>
          <NugetFeedsList />
        </div>
    );
  }
}
