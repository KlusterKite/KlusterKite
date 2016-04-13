import React, { Component, PropTypes } from 'react';
import {connect} from 'react-redux';
import * as modulesActions from 'redux/modules/monitoring-modules';
import {isLoaded, loadModules as loadModulesOnInit} from 'redux/modules/monitoring-modules';
import { asyncConnect } from 'redux-async-connect';

@asyncConnect([{
  promise: ({store: {dispatch, getState}}) => {
    const promises = [];

    if (!isLoaded(getState())) {
      promises.push(dispatch(loadModulesOnInit()));
    }

    return Promise.all(promises);
  }
}])
@connect(
  state => ({
    data: state.monitoringModules.data,
    loading: state.monitoringModules.loading,
    loaded: state.monitoringModules.loaded,
    upgrading: state.monitoringModules.upgrading,
    upgradingId: state.monitoringModules.upgradingId,
    upgraded: state.monitoringModules.upgraded
  }),
  {...modulesActions })

export default class MonitoringModules extends Component {
  static propTypes = {
    data: PropTypes.arrayOf(PropTypes.object),
    loading: PropTypes.bool,
    loaded: PropTypes.bool.isRequired,
    loadModules: PropTypes.func.isRequired,
    upgradeNode: PropTypes.func.isRequired,
    upgraded: PropTypes.bool,
    upgrading: PropTypes.bool,
    upgradingId: PropTypes.string
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

  handleUpgrade = (mod) => {
    const {upgradeNode} = this.props;
    upgradeNode(mod.NodeId, mod.NodeAddress);
  }

  render() {
    const {loading, loaded, data, upgraded, upgrading, upgradingId} = this.props;
    const styles = require('./Monitoring.scss');

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

          {upgraded &&
            <div className="alert alert-success" role="alert">
              <span className="glyphicon glyphicon-ok" aria-hidden="true"></span>
              {' '}
              Node upgrade request has been submitted. It might take a while before the operation completes.
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
                    <br />
                    <button type="button" className={styles.upgrade + ' btn btn-xs'} onClick={() => this.handleUpgrade(module)} disabled={upgrading}>
                      <i className={upgradingId === module.NodeId ? 'fa fa-refresh fa-spin' : 'fa fa-refresh'}/> {' '} Upgrade Node
                    </button>
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
