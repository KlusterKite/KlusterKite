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
    data: state.packages.data,
    loading: state.packages.loading,
    loaded: state.packages.loaded,
    reloading: state.packages.reloading,
    reloaded: state.packages.reloaded,
    error: state.packages.error
  }),
  {...actions })

export default class PackagesList extends Component {
  static propTypes = {
    data: PropTypes.any,
    loading: PropTypes.bool,
    loaded: PropTypes.bool.isRequired,
    load: PropTypes.func.isRequired,
    error: PropTypes.string,
    reloading: PropTypes.bool,
    reloaded: PropTypes.bool,
    reload: PropTypes.func.isRequired
  };

  componentDidMount() {
    const {loading, load} = this.props;

    if (!loading) {
      load();
    }
  }

  handleReload = () => {
    const {reload, load} = this.props;
    reload().then(function afterReload() {
      load();
    });
  }

  render() {
    const {loading, loaded, error, data, reloading, reloaded} = this.props;

    let records = null;
    // Sort items by name
    if (data && data.length) {
      records = data.sort(function sort(aa, bb) {
        const nameA = aa.Id.toLowerCase();
        const nameB = bb.Id.toLowerCase();
        if (nameA < nameB) {
          return -1;
        }
        if (nameA > nameB) {
          return 1;
        }
        return 0;
      });
    }

    let loadClassName = 'fa fa-refresh';
    if (loading) {
      loadClassName += ' fa-spin';
    }

    let reloadClassName = 'fa fa-refresh';
    if (reloading) {
      reloadClassName += ' fa-spin';
    }

    return (
        <div>
          {!loaded && loading &&
          <div className="container">
            <p><i className="fa fa-spinner fa-spin"></i> Loading data… </p>
          </div>
          }

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

          {loaded &&
          <table className="table table-hover">
            <thead>
                <tr>
                    <th>Id</th>
                    <th>Version</th>
                </tr>
            </thead>
            <tbody>
              {records && records.length && records.map((item) =>
                <tr key={item.Id}>
                  <td>{item.Id}</td>
                  <td>{item.Version}</td>
                </tr>
                )
              }
            </tbody>
          </table>
          }
        </div>
    );
  }
}
