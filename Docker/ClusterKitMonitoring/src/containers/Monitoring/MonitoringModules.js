import React, { Component, PropTypes } from 'react';
import {connect} from 'react-redux';
import connectData from 'helpers/connectData';
import * as modulesActions from 'redux/modules/monitoring-modules';
import {isLoaded, loadModules as loadModulesOnInit} from 'redux/modules/monitoring-modules';

function fetchDataDeferred(getState, dispatch) {
  if (!isLoaded(getState())) {
    return dispatch(loadModulesOnInit());
  }
}

@connectData(null, fetchDataDeferred)
@connect(
  state => ({
    data: state.monitoringModules.data,
    loading: state.monitoringModules.loading,
    loaded: state.monitoringModules.loaded
  }),
  {...modulesActions })

export default class MonitoringModules extends Component {
  static propTypes = {
    data: PropTypes.arrayOf(PropTypes.object),
    loading: PropTypes.bool,
    loaded: PropTypes.bool.isRequired,
    loadModules: PropTypes.func.isRequired
  };

  componentDidMount() {
    const {loading, loadModules} = this.props;

    if (!loading) {
      loadModules();
    }

    this.interval = window.setInterval(loadModules, 5000);
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
          <h2>Modules</h2>
          {!loaded && loading &&
          <div className="container">
            <p><i className="fa fa-spinner fa-spin"></i> Loading data… </p>
          </div>
          }

          {loaded &&
          <table className="table table-hover">
            <thead>
                <tr>
                    <th>Id</th>
                    <th>Address</th>
                    <th>Modules</th>
                    <th>Status</th>
                    <th>Template</th>
                    <th>Container</th>
                </tr>
            </thead>
            <tbody>
              {data && data.map((module) =>
                <tr key={module.NodeId}>
                  <td>
                    <small>{module.NodeId}</small>
                  </td>
                  <td>{module.NodeAddress.Host}:{module.NodeAddress.Port}</td>
                  <td>
                    {module.Modules.map((subModule) =>
                        <span key={module.NodeId + '/' + subModule.Id}>
                          <span className="label label-default">{subModule.Id}</span>{' '}
                        </span>
                      )
                    }
                  </td>
                  <td>
                    {!module.IsObsolete &&
                      <span className="label label-success">Actual</span>
                    }
                    {module.IsObsolete &&
                      <span className="label label-warning">Obsolete</span>
                    }
                  </td>
                  <td>
                    {module.NodeTemplate}
                  </td>
                  <td>
                    {module.ContainerType}
                  </td>
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
