import React, { Component, PropTypes } from 'react';
import {connect} from 'react-redux';
import * as actions from 'redux/modules/packages';
import {isLoaded, load as loadOnInit} from 'redux/modules/packages';
import { asyncConnect } from 'redux-async-connect';
import PackagesReload from './PackagesReload';

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
    loaded: state.packages.loaded
  }),
  {...actions })

export default class PackagesList extends Component {
  static propTypes = {
    data: PropTypes.any,
    loading: PropTypes.bool,
    loaded: PropTypes.bool.isRequired,
    load: PropTypes.func.isRequired
  };

  componentDidMount() {
    const {loading, load} = this.props;

    if (!loading) {
      load();
    }
  }

  render() {
    const {loading, loaded, data} = this.props;

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

    return (
        <div>
          {!loaded && loading &&
          <div className="container">
            <p><i className="fa fa-spinner fa-spin"></i> Loading data… </p>
          </div>
          }

          <PackagesReload />

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
