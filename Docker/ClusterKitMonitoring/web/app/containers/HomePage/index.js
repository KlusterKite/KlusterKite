/*
 *
 * HomePage
 *
 */

import React, { Component, PropTypes } from 'react';
import { autobind } from 'core-decorators';
import { connect } from 'react-redux';
import selectHomePage from './selectors';

import {
  nodeDescriptionsLoadAction,
  nodeUpgradeAction,
  nodeReloadPackagesAction,
} from './actions';

import NodesList from '../../components/NodesList';
import SwaggerLinksList from '../../components/SwaggerLinksList';


export class HomePage extends Component { // eslint-disable-line react/prefer-stateless-function

  static propTypes = {
    dispatch: PropTypes.func.isRequired,
    nodeDescriptions: PropTypes.array.isRequired,
    swaggerLinks: PropTypes.array.isRequired,
    hasError: PropTypes.bool.isRequired,
  }

  componentWillMount() {
    const { dispatch } = this.props;
    dispatch(nodeDescriptionsLoadAction());
  }

  @autobind
  onNodeUpgradeClick(node) {
    const { dispatch } = this.props;
    dispatch(nodeUpgradeAction(node));
  }

  @autobind
  handleReload() {
    const { dispatch } = this.props;
    dispatch(nodeReloadPackagesAction());
  }

  render() {
    return (
      <div className="container">
        <h1>Monitoring</h1>
        <button type="button" className="btn btn-primary btn-lg" onClick={this.handleReload}>
          <i className="fa fa-refresh" /> {' '} Reload packages
        </button>

        <NodesList nodes={this.props.nodeDescriptions} hasError={this.props.hasError} onManualUpgrade={this.onNodeUpgradeClick} />
        <SwaggerLinksList links={this.props.swaggerLinks} />
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
