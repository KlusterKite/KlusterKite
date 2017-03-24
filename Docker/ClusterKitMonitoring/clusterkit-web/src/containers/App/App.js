import React from 'react'

import { IndexLink } from 'react-router';
import { LinkContainer } from 'react-router-bootstrap';
import Navbar from 'react-bootstrap/lib/Navbar';
import Nav from 'react-bootstrap/lib/Nav';
import NavItem from 'react-bootstrap/lib/NavItem';

import { hasPrivilege } from '../../utils/privileges';
import Storage from '../../utils/ttl-storage';

import './App.css';

export default class App extends React.Component {
  render () {
    const username = this.getUsername();

    return (
      <div>
        <Navbar fixedTop>
          <Navbar.Header>
            <Navbar.Brand>
              <IndexLink to="/" activeStyle={{ color: '#333' }}>
                <div className="topLogo" />
                <span>ClusterKit</span>
              </IndexLink>
            </Navbar.Brand>
          </Navbar.Header>
          <Navbar.Collapse>
            <Nav navbar>
              <LinkContainer to="/GraphQL">
                <NavItem>GraphQL</NavItem>
              </LinkContainer>
            </Nav>
            <Nav navbar>
              {hasPrivilege('ClusterKit.NodeManager.NodeTemplate.Query') &&
                <LinkContainer to="/Templates">
                  <NavItem>Templates</NavItem>
                </LinkContainer>
              }
            </Nav>
            <Nav navbar>
              {hasPrivilege('ClusterKit.NodeManager.NugetFeed.Query') &&
                <LinkContainer to="/NugetFeeds">
                  <NavItem>Nuget Feeds</NavItem>
                </LinkContainer>
              }
            </Nav>
            <Nav navbar>
              {hasPrivilege('ClusterKit.Monitoring.GetClusterTree') &&
              <LinkContainer to="/ActorsTree">
                <NavItem>Actors Tree</NavItem>
              </LinkContainer>
              }
            </Nav>
            {username &&
            <Nav pullRight>
              <LinkContainer to="/Logout">
                <NavItem href="#">Logout ({username})</NavItem>
              </LinkContainer>
            </Nav>
            }
          </Navbar.Collapse>
        </Navbar>
        <div className="container app">
          {this.props.children}
        </div>
      </div>
    )
  }

  /**
   * Gets current authorized username from the local storage
   * @return {string} username
   */
  getUsername() {
    const refreshToken = Storage.get('refreshToken');
    let username = null;
    if (refreshToken) {
      username = Storage.get('username');
      if (!username) {
        username = 'user';
      }
    }
    return username;
  }
}
