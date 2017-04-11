import React from 'react';

import Relay from 'react-relay'
import useRelay from 'react-router-relay'
import { Router, IndexRoute, Route, Redirect, browserHistory, applyRouterMiddleware } from 'react-router'

import App from './containers/App/App';
import ActorsTreePage from './containers/ActorsTreePage/ActorsTreePage';
import AuthPage from './containers/AuthPage/AuthPage';
import FeedPage from './containers/FeedPage/FeedPage';
import GraphQLPage from './containers/GraphQL/GraphQLPage';
import HomePage from './containers/Home/HomePage';
import Loading from './components/Loading/index';
import LogoutPage from './containers/LogoutPage/LogoutPage';
import NotFoundPage from './containers/NotFoundPage/NotFoundPage';
import ReleaseConfigCopyPage from './containers/ConfigCopyPage/ReleaseConfigCopyPage';
import ReleasesListPage from './containers/ReleasesListPage/ReleasesListPage';
import ReleasePage from './containers/ReleasePage/ReleasePage';
import SeedPage from './containers/SeedPage/SeedPage';
import TemplatePage from './containers/TemplatePage/TemplatePage';

export default class RoutesList extends React.Component {
  render () {
    const ApiQueries = { api: () => Relay.QL`query {
      api
    }` };

    return (
      <Router forceFetch environment={Relay.Store} render={applyRouterMiddleware(useRelay)} history={browserHistory}>
        <Route path="/clusterkit/" component={App}>
          <IndexRoute component={HomePage} queries={ApiQueries} />
          <Route path='/clusterkit/Login' component={AuthPage} />
          <Route path='/clusterkit/Logout' component={LogoutPage} />
          <Route path='/clusterkit/ActorsTree' component={ActorsTreePage} queries={ApiQueries} />
          <Route path='/clusterkit/CopyConfig/:releaseId/:mode' component={ReleaseConfigCopyPage} queries={ApiQueries} render={({ props }) => props ? <ReleaseConfigCopyPage {...props} /> : <Loading />} />
          <Route path='/clusterkit/GraphQL' component={GraphQLPage} />
          <Route path='/clusterkit/NugetFeeds/:releaseId/create' component={FeedPage} queries={ApiQueries} />
          <Route path='/clusterkit/NugetFeeds/:releaseId/:id' component={FeedPage} queries={ApiQueries} render={({ props }) => props ? <FeedPage {...props} /> : <Loading />} />
          <Route path='/clusterkit/Releases' component={ReleasesListPage} queries={ApiQueries} render={({ props }) => props ? <ReleasesListPage {...props} /> : <Loading />} />
          <Route path='/clusterkit/Releases/create' component={ReleasePage} queries={ApiQueries} render={({ props }) => props ? <ReleasePage {...props} /> : <Loading />} />
          <Route path='/clusterkit/Releases/:id' component={ReleasePage} queries={ApiQueries} render={({ props }) => props ? <ReleasePage {...props} /> : <Loading />} />
          <Route path='/clusterkit/Seeds/:releaseId' component={SeedPage} queries={ApiQueries} render={({ props }) => props ? <SeedPage {...props} /> : <Loading />} />
          <Route path='/clusterkit/Templates/:releaseId/create' component={TemplatePage} queries={ApiQueries} render={({ props }) => props ? <TemplatePage {...props} /> : <Loading />} />
          <Route path='/clusterkit/Templates/:releaseId/:id' component={TemplatePage} queries={ApiQueries} render={({ props }) => props ? <TemplatePage {...props} /> : <Loading />} />
          <Route path='*' components={NotFoundPage} />
        </Route>
        <Redirect from="/" to="clusterkit/" />
      </Router>
    )
  }
}
