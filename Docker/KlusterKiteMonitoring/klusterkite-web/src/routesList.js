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
import MigrationPage from './containers/MigrationPage/MigrationPage';
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
        <Route path="/klusterkite/" component={App}>
          <IndexRoute component={HomePage} queries={ApiQueries} />
          <Route path='/klusterkite/Login' component={AuthPage} />
          <Route path='/klusterkite/Logout' component={LogoutPage} />
          <Route path='/klusterkite/ActorsTree' component={ActorsTreePage} queries={ApiQueries} />
          <Route path='/klusterkite/CopyConfig/:releaseId/:mode' component={ReleaseConfigCopyPage} queries={ApiQueries} render={({ props }) => props ? <ReleaseConfigCopyPage {...props} /> : <Loading />} />
          <Route path='/klusterkite/GraphQL' component={GraphQLPage} />
          <Route path='/klusterkite/Migration' component={MigrationPage} queries={ApiQueries} />
          <Route path='/klusterkite/NugetFeeds/:releaseId' component={FeedPage} queries={ApiQueries} render={({ props }) => props ? <FeedPage {...props} /> : <Loading />} />
          <Route path='/klusterkite/Releases' component={ReleasesListPage} queries={ApiQueries} render={({ props }) => props ? <ReleasesListPage {...props} /> : <Loading />} />
          <Route path='/klusterkite/Releases/:page' component={ReleasesListPage} queries={ApiQueries} render={({ props }) => props ? <ReleasesListPage {...props} /> : <Loading />} />
          <Route path='/klusterkite/Release/create' component={ReleasePage} queries={ApiQueries} render={({ props }) => props ? <ReleasePage {...props} /> : <Loading />} />
          <Route path='/klusterkite/Release/:id' component={ReleasePage} queries={ApiQueries} render={({ props }) => props ? <ReleasePage {...props} /> : <Loading />} />
          <Route path='/klusterkite/Roles' component={RolesListPage} queries={ApiQueries} render={({ props }) => props ? <RolesListPage {...props} /> : <Loading />} />
          <Route path='/klusterkite/Roles/create' component={RolePage} queries={ApiQueries} render={({ props }) => props ? <RolePage {...props} /> : <Loading />} />
          <Route path='/klusterkite/Roles/:id' component={RolePage} queries={ApiQueries} render={({ props }) => props ? <RolePage {...props} /> : <Loading />} />
          <Route path='/klusterkite/Seeds/:releaseId' component={SeedPage} queries={ApiQueries} render={({ props }) => props ? <SeedPage {...props} /> : <Loading />} />
          <Route path='/klusterkite/NodeTemplates/:releaseId/create' component={NodeTemplatePage} queries={ApiQueries} render={({ props }) => props ? <NodeTemplatePage {...props} /> : <Loading />} />
          <Route path='/klusterkite/NodeTemplates/:releaseId/:id' component={NodeTemplatePage} queries={ApiQueries} render={({ props }) => props ? <NodeTemplatePage {...props} /> : <Loading />} />
          <Route path='/klusterkite/MigratorTemplates/:releaseId/create' component={MigratorTemplatePage} queries={ApiQueries} render={({ props }) => props ? <MigratorTemplatePage {...props} /> : <Loading />} />
          <Route path='/klusterkite/MigratorTemplates/:releaseId/:id' component={MigratorTemplatePage} queries={ApiQueries} render={({ props }) => props ? <MigratorTemplatePage {...props} /> : <Loading />} />
          <Route path='/klusterkite/Users' component={UsersListPage} queries={ApiQueries} render={({ props }) => props ? <UsersListPage {...props} /> : <Loading />} />
          <Route path='/klusterkite/Users/create' component={UserPage} queries={ApiQueries} render={({ props }) => props ? <UserPage {...props} /> : <Loading />} />
          <Route path='/klusterkite/Users/:id' component={UserPage} queries={ApiQueries} render={({ props }) => props ? <UserPage {...props} /> : <Loading />} />
          <Route path='*' components={NotFoundPage} />
        </Route>
        <Redirect from="/" to="klusterkite/" />
      </Router>
    )
  }
}
