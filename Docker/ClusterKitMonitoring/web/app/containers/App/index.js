/**
 *
 * App.react.js
 *
 * This component is the skeleton around the actual pages, and should only
 * contain code that should be seen on all pages. (e.g. navigation bar)
 *
 * NOTE: while this component should technically be a stateless functional
 * component (SFC), hot reloading does not currently support SFCs. If hot
 * reloading is not a neccessity for you then you can refactor it and remove
 * the linting exception.
 */

import React from 'react';

import { IndexLink } from 'react-router';
import { LinkContainer } from 'react-router-bootstrap';
import Navbar from 'react-bootstrap/lib/Navbar';
import Nav from 'react-bootstrap/lib/Nav';
import NavItem from 'react-bootstrap/lib/NavItem';
import config from '../../config';
import { hasPrivilege } from '../../utils/privileges';

import Cookies from 'js-cookie';

import styles from './styles.css';

export default class App extends React.Component { // eslint-disable-line react/prefer-stateless-function

  static propTypes = {
    children: React.PropTypes.node,
  };

  render() {
    const username = this.getUsername();

    return (
      <div className={styles.app}>

        <Navbar fixedTop>
          <Navbar.Header>
            <Navbar.Brand>
              <IndexLink to="/clusterkit/" activeStyle={{ color: '#333' }}>
                <div className={styles.brand} />
                <span>{config.app.title}</span>
              </IndexLink>
            </Navbar.Brand>
            <Navbar.Toggle />
          </Navbar.Header>

          <Navbar.Collapse>
            <Nav navbar>
              {hasPrivilege('ClusterKit.NodeManager.NodeTemplate.GetList') &&
                <LinkContainer to="/clusterkit/templates">
                  <NavItem>Templates</NavItem>
                </LinkContainer>
              }
              {hasPrivilege('ClusterKit.NodeManager.NugetFeed.GetList') &&
                <LinkContainer to="/clusterkit/nugetfeeds">
                  <NavItem>Nuget Feeds</NavItem>
                </LinkContainer>
              }
              {hasPrivilege('ClusterKit.NodeManager.GetPackages') &&
                <LinkContainer to="/clusterkit/packages">
                  <NavItem>Packages</NavItem>
                </LinkContainer>
              }
              {hasPrivilege('ClusterKit.Monitoring.GetClusterTree') &&
                <LinkContainer to="/clusterkit/actorsTree">
                  <NavItem>Actors tree</NavItem>
                </LinkContainer>
              }
            </Nav>
            {username &&
              <Nav pullRight>
                <LinkContainer to="/clusterkit/logout">
                  <NavItem href="#">Logout ({username})</NavItem>
                </LinkContainer>
              </Nav>
            }
          </Navbar.Collapse>
        </Navbar>

        <div className={styles.appContent}>
          {this.props.children}
        </div>
      </div>
    );
  }

  /**
   * Gets current authorized username from Cookies
   * @return {string} username
   */
  getUsername() {
    const refreshToken = Cookies.get('refreshToken');
    let username = null;
    if (refreshToken) {
      username = Cookies.get('username');
      if (!username) {
        username = 'user';
      }
    }
    return username;
  }
}
