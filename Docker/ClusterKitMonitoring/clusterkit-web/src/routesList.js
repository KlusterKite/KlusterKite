import React from 'react';

import Relay from 'react-relay'
import useRelay from 'react-router-relay'
import { Router, IndexRoute, Route, browserHistory, applyRouterMiddleware } from 'react-router'

import App from './containers/App/App';
import AuthPage from './containers/AuthPage/AuthPage';
import EditFormPage from './containers/EditForm/EditFormPage';
import FeedsListPage from './containers/FeedsListPage/FeedsListPage';
import FeedPage from './containers/FeedPage/FeedPage';
import GraphQLPage from './containers/GraphQL/GraphQLPage';
import HomePage from './containers/Home/HomePage';
import LogoutPage from './containers/LogoutPage/LogoutPage';
import NotFoundPage from './containers/NotFoundPage/NotFoundPage';
import TemplatesListPage from './containers/TemplatesListPage/TemplatesListPage';
import TemplatePage from './containers/TemplatePage/TemplatePage';

export default class RoutesList extends React.Component {
  render () {
    const ApiQueries = { api: () => Relay.QL`query {
      api
    }` };

    return (
      <Router forceFetch environment={Relay.Store} render={applyRouterMiddleware(useRelay)} history={browserHistory}>
        <Route path="/" component={App}>
          <IndexRoute component={HomePage} queries={ApiQueries} />
          <Route path='/Login' component={AuthPage} />
          <Route path='/Logout' component={LogoutPage} />
          <Route path='/Drivers' component={EditFormPage} queries={ApiQueries} />
          <Route path='/GraphQL' component={GraphQLPage} />
          <Route path='/NugetFeeds' component={FeedsListPage} queries={ApiQueries} />
          <Route path='/NugetFeeds/create' component={FeedPage} queries={ApiQueries} />
          <Route path='/NugetFeeds/:id' component={FeedPage} queries={ApiQueries} />
          <Route path='/Templates' component={TemplatesListPage} queries={ApiQueries} />
          <Route path='/Templates/create' component={TemplatePage} queries={ApiQueries} />
          <Route path='/Templates/:id' component={TemplatePage} queries={ApiQueries} />
          <Route path='*' components={NotFoundPage} />
        </Route>
      </Router>
    )
  }
}
