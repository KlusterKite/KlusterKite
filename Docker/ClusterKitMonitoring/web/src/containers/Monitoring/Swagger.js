import React, { Component, PropTypes } from 'react';
import {connect} from 'react-redux';
import connectData from 'helpers/connectData';
import * as actions from 'redux/modules/monitoring-swagger';
import {isLoaded, loadModules as loadModulesOnInit} from 'redux/modules/monitoring-swagger';

function fetchDataDeferred(getState, dispatch) {
  if (!isLoaded(getState())) {
    return dispatch(loadModulesOnInit());
  }
}

@connectData(null, fetchDataDeferred)
@connect(
  state => ({
    data: state.monitoringSwagger.data,
    loading: state.monitoringSwagger.loading,
    loaded: state.monitoringSwagger.loaded
  }),
  {...actions })

export default class MonitoringSwagger extends Component {
  static propTypes = {
    data: PropTypes.arrayOf(PropTypes.string),
    loading: PropTypes.bool,
    loaded: PropTypes.bool.isRequired,
    load: PropTypes.func.isRequired
  };

  componentDidMount() {
    const {loading, load} = this.props;

    if (!loading) {
      load();
    }

    this.interval = window.setInterval(load, 30000);
  }

  componentWillUnmount() {
    clearInterval(this.interval);
  }

  render() {
    const {loading, loaded, data} = this.props;

    let loadClassName = 'fa fa-refresh';
    if (loading) {
      loadClassName += ' fa-spin';
    }

    return (
        <div>
          <h2>Swagger</h2>
          {!loaded && loading &&
          <div className="container">
            <p><i className="fa fa-spinner fa-spin"></i> Loading data… </p>
          </div>
          }

          {loaded &&
          <table className="table table-hover">
            <thead>
                <tr>
                    <th>Name</th>
                </tr>
            </thead>
            <tbody>
              <tr>
                <td>{data}</td>
              </tr>
            </tbody>
          </table>
          }
        </div>
    );
  }
}
