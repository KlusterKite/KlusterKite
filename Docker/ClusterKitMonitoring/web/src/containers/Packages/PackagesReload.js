import React, { Component, PropTypes } from 'react';
import {connect} from 'react-redux';
import * as actions from 'redux/modules/packages';
import {isLoaded, load as loadOnInit} from 'redux/modules/packages';
import { asyncConnect } from 'redux-async-connect';

@asyncConnect([{
  promise: ({store: {dispatch, getState}}) => {
    const promises = [];

    if (!isLoaded(getState())) {
      promises.push(dispatch(loadOnInit()));
    }

    return Promise.all(promises);
  }
}])
@connect(
  state => ({
    reloading: state.packages.reloading,
    reloaded: state.packages.reloaded,
    error: state.packages.error
  }),
  {...actions })

export default class PackagesList extends Component {
  static propTypes = {
    load: PropTypes.func.isRequired,
    error: PropTypes.string,
    reloading: PropTypes.bool,
    reloaded: PropTypes.bool,
    reload: PropTypes.func.isRequired
  };

  handleReload = () => {
    const {reload, load} = this.props;
    reload().then(function afterReload() {
      load();
    });
  }

  render() {
    const {error, reloading, reloaded} = this.props;

    let reloadClassName = 'fa fa-refresh';
    if (reloading) {
      reloadClassName += ' fa-spin';
    }

    return (
        <div>
          {error &&
          <div className="alert alert-danger" role="alert">
            <span className="glyphicon glyphicon-exclamation-sign" aria-hidden="true"></span>
            {' '}
            {error}
          </div>
          }

          {reloaded &&
          <div className="alert alert-success" role="alert">
            <span className="glyphicon glyphicon-ok" aria-hidden="true"></span>
            {' '}
            Reloaded
          </div>
          }

          <button type="button" className="btn btn-primary btn-lg" onClick={this.handleReload}>
            <i className={reloadClassName}/> {' '} Reload packages
          </button>
        </div>
    );
  }
}
