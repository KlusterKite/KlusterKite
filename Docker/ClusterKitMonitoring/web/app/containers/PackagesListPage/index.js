/*
 *
 * PackagesListPage
 *
 */

import React, { Component, PropTypes } from 'react';
import { connect } from 'react-redux';
import selectPackagesListPage from './selectors';

import {
  packagesLoadAction,
} from './actions';

import PackageList from '../../components/PackageList';

export class PackagesListPage extends Component { // eslint-disable-line react/prefer-stateless-function

  static propTypes = {
    dispatch: PropTypes.func.isRequired,
    packages: PropTypes.array.isRequired,
  }

  componentWillMount() {
    const { dispatch } = this.props;
    dispatch(packagesLoadAction());
  }


  render() {
    return (
      <div>
        <PackageList packages={this.props.packages} />
      </div>
    );
  }
}

const mapStateToProps = selectPackagesListPage();

function mapDispatchToProps(dispatch) {
  return {
    dispatch,
  };
}

export default connect(mapStateToProps, mapDispatchToProps)(PackagesListPage);
