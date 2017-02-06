/*
 *
 * AuthPage
 *
 */

import { withRouter } from 'react-router';
import { autobind } from 'core-decorators';
import React, { PropTypes } from 'react';
import { connect } from 'react-redux';
import selectAuthPage from './selectors';
import styles from './styles.css';

import AuthForm from '../../components/AuthForm';

import {
  requestLogin,
} from './actions';

@withRouter
export class AuthPage extends React.Component { // eslint-disable-line react/prefer-stateless-function

  static propTypes = {
    dispatch: PropTypes.func.isRequired,
    params: PropTypes.object.isRequired,
    location: PropTypes.object.isRequired,

    authorizing: PropTypes.bool.isRequired,
    authorized: PropTypes.bool,
    authorizationError: PropTypes.string,
    authorizationException: PropTypes.object,
    privilegesReceived: PropTypes.bool,
  };

  componentWillReceiveProps(nextProps) {
    if (nextProps.authorized && nextProps.privilegesReceived) {
      if (this.props.location && this.props.location.query && this.props.location.query.from) {
        window.location = this.props.location.query.from;
      } else {
        window.location = '/clusterkit/';
      }
    }
  }

  @autobind
  login(data) {
    const { dispatch } = this.props;

    const action = requestLogin(data);
    dispatch(action);
  }

  render() {
    const { authorizing, authorized, authorizationError } = this.props;

    return (
      <div className="container">
        <h1>Login</h1>
        <AuthForm onSubmit={this.login} authorizing={authorizing} authorized={authorized} authorizationError={authorizationError} />
      </div>
    );
  }
}

const mapStateToProps = selectAuthPage();

function mapDispatchToProps(dispatch) {
  return {
    dispatch,
  };
}

export default connect(mapStateToProps, mapDispatchToProps)(AuthPage);
