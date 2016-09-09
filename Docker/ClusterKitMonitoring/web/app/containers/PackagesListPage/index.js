/*
 *
 * PackagesListPage
 *
 */

import React from 'react';
import { connect } from 'react-redux';
import selectPackagesListPage from './selectors';
import styles from './styles.css';

import {
  packagesLoadAction
} from './actions';

import PackageList from '../../components/PackageList'

export class PackagesListPage extends React.Component { // eslint-disable-line react/prefer-stateless-function


  componentWillMount() {
    const {dispatch} = this.props;
    dispatch(packagesLoadAction());
  }


  render() {
    return (
      <div className={styles.packagesListPage}>
        <PackageList packages={this.props.packages}/>
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
