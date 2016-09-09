/*
 *
 * HomePage
 *
 */

import React from 'react';
import {autobind} from 'core-decorators'
import { connect } from 'react-redux';
import selectHomePage from './selectors';

import styles from './styles.css';

import {
  nodeDescriptionsLoadAction,
  nodeUpgradeAction,
  nodeReloadPackagesAction
} from './actions';

import NodesList from '../../components/NodesList'



export class HomePage extends React.Component { // eslint-disable-line react/prefer-stateless-function

  @autobind
  onNodeUpgradeClick(node) {
    const {dispatch} = this.props;
    dispatch(nodeUpgradeAction(node));
  }

  @autobind
  handleReload() {
    const {dispatch} = this.props;
    dispatch(nodeReloadPackagesAction());
  }

  componentWillMount() {
    const {dispatch} = this.props;
    dispatch(nodeDescriptionsLoadAction());
  }


  render() {
    return (
      <div>
        <h1>Monitoring</h1>
        <button type="button" className="btn btn-primary btn-lg" onClick={this.handleReload}>
          <i className="fa fa-refresh"/> {' '} Reload packages
        </button>

        <NodesList nodes={this.props.nodeDescriptions} onManualUpgrade={this.onNodeUpgradeClick}/>
      </div>
    );
  }
}

const mapStateToProps = selectHomePage();

function mapDispatchToProps(dispatch) {
  return {
    dispatch,
  };
}

export default connect(mapStateToProps, mapDispatchToProps)(HomePage);
