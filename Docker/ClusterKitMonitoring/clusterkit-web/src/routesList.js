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
import RolesListPage from './containers/RolesListPage/RolesListPage';
import RolePage from './containers/RolePage/RolePage';
import UsersListPage from './containers/UsersListPage/UsersListPage';
import UserPage from './containers/UserPage/UserPage';
import SeedPage from './containers/SeedPage/SeedPage';
import MigratorTemplatePage from './containers/MigratorTemplatePage/TemplatePage';
import NodeTemplatePage from './containers/NodeTemplatePage/TemplatePage';

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
          <Route path='/clusterkit/NugetFeeds/:releaseId' component={FeedPage} queries={ApiQueries} render={({ props }) => props ? <FeedPage {...props} /> : <Loading />} />
          <Route path='/clusterkit/Releases' component={ReleasesListPage} queries={ApiQueries} render={({ props }) => props ? <ReleasesListPage {...props} /> : <Loading />} />
          <Route path='/clusterkit/Releases/:page' component={ReleasesListPage} queries={ApiQueries} render={({ props }) => props ? <ReleasesListPage {...props} /> : <Loading />} />
          <Route path='/clusterkit/Release/create' component={ReleasePage} queries={ApiQueries} render={({ props }) => props ? <ReleasePage {...props} /> : <Loading />} />
          <Route path='/clusterkit/Release/:id' component={ReleasePage} queries={ApiQueries} render={({ props }) => props ? <ReleasePage {...props} /> : <Loading />} />
          <Route path='/clusterkit/Roles' component={RolesListPage} queries={ApiQueries} render={({ props }) => props ? <RolesListPage {...props} /> : <Loading />} />
          <Route path='/clusterkit/Roles/create' component={RolePage} queries={ApiQueries} render={({ props }) => props ? <RolePage {...props} /> : <Loading />} />
          <Route path='/clusterkit/Roles/:id' component={RolePage} queries={ApiQueries} render={({ props }) => props ? <RolePage {...props} /> : <Loading />} />
          <Route path='/clusterkit/Seeds/:releaseId' component={SeedPage} queries={ApiQueries} render={({ props }) => props ? <SeedPage {...props} /> : <Loading />} />
          <Route path='/clusterkit/NodeTemplates/:releaseId/create' component={NodeTemplatePage} queries={ApiQueries} render={({ props }) => props ? <NodeTemplatePage {...props} /> : <Loading />} />
          <Route path='/clusterkit/NodeTemplates/:releaseId/:id' component={NodeTemplatePage} queries={ApiQueries} render={({ props }) => props ? <NodeTemplatePage {...props} /> : <Loading />} />
          <Route path='/clusterkit/MigratorTemplates/:releaseId/create' component={MigratorTemplatePage} queries={ApiQueries} render={({ props }) => props ? <MigratorTemplatePage {...props} /> : <Loading />} />
          <Route path='/clusterkit/MigratorTemplates/:releaseId/:id' component={MigratorTemplatePage} queries={ApiQueries} render={({ props }) => props ? <MigratorTemplatePage {...props} /> : <Loading />} />
          <Route path='/clusterkit/Users' component={UsersListPage} queries={ApiQueries} render={({ props }) => props ? <UsersListPage {...props} /> : <Loading />} />
          <Route path='/clusterkit/Users/create' component={UserPage} queries={ApiQueries} render={({ props }) => props ? <UserPage {...props} /> : <Loading />} />
          <Route path='/clusterkit/Users/:id' component={UserPage} queries={ApiQueries} render={({ props }) => props ? <UserPage {...props} /> : <Loading />} />
          <Route path='*' components={NotFoundPage} />
        </Route>
        <Redirect from="/" to="clusterkit/" />
      </Router>
    )
  }
}
