/*
 *
 * ActorsTreePage
 *
 */

import React, { Component, PropTypes } from 'react';
import { connect } from 'react-redux';
import selectActorsTreePage from './selectors';
import { autobind } from 'core-decorators';

import {
  treeLoadAction,
  treeScanAction,
} from './actions';

import ActorTree from '../../components/ActorsTree';
import styles from './styles.css';

export class ActorsTreePage extends Component { // eslint-disable-line react/prefer-stateless-function

  static propTypes = {
    dispatch: PropTypes.func.isRequired,
    tree: PropTypes.object.isRequired,
    hasError: PropTypes.bool.isRequired,
    isLoading: PropTypes.bool.isRequired,
  }

  componentWillMount() {
    this.handleReload();
  }

  @autobind()
  handleReload() {
    const { dispatch } = this.props;
    dispatch(treeLoadAction());
  }

  @autobind()
  handleScan() {
    const { dispatch } = this.props;
    dispatch(treeScanAction());
  }

  render() {
    let reloadClassName = 'fa fa-refresh';
    if (this.props.isLoading) {
      reloadClassName += ' fa-spin';
    }

    return (
      <div className={styles.container}>
        <h1>Actors tree</h1>

        <div className={`${styles.panel} row`} >
          <div className="col-md-6">
            <div className="alert alert-warning" role="alert">
              <div className={styles.scanPanel}>
                <div>
                  <button
                    type="button"
                    className="btn btn-warning btn-sm"
                    onClick={this.handleScan}
                    disabled={this.props.isLoading}
                  >
                    <i className={reloadClassName} /> {' '} Scan
                  </button>
                </div>
                <div>
                  <p>Scan generates additional load to all cluster node. Do this only in emergency situations or in
                  development infrastructure</p>
                </div>
              </div>
            </div>
          </div>
          <div className="col-md-6">
            <div className="alert alert-info" role="alert">
              <div className={styles.scanPanel}>
                <div>
                  <button
                    type="button"
                    className="btn btn-primary btn-sm"
                    onClick={this.handleReload}
                    disabled={this.props.isLoading}
                  >
                    <i className={reloadClassName} /> {' '} Reload tree
                  </button>
                </div>
                <div>
                  <p>Scan can take some time and can be unfinished in time of tree load. Try to reload tree before
                  initiating new scan</p>
                </div>
              </div>
            </div>
          </div>

        </div>

        {this.props.hasError &&
          <div className="alert alert-danger" role="alert">
            <span className="glyphicon glyphicon-exclamation-sign" aria-hidden="true"></span>
            <span> Could not connect to the server</span>
          </div>
        }

        <div className={styles.graphFrame}>
          <ActorTree tree={this.props.tree} />
        </div>

      </div>
    );
  }
}

const mapStateToProps = selectActorsTreePage();

function mapDispatchToProps(dispatch) {
  return {
    dispatch,
  };
}

export default connect(mapStateToProps, mapDispatchToProps)(ActorsTreePage);
