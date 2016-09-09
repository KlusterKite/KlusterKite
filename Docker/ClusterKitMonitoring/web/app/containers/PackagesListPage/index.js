/*
 *
 * PackagesListPage
 *
 */

import React from 'react';
import { connect } from 'react-redux';
import selectPackagesListPage from './selectors';
import styles from './styles.css';

export class PackagesListPage extends React.Component { // eslint-disable-line react/prefer-stateless-function
  render() {
    return (
      <div className={styles.packagesListPage}>
      This is PackagesListPage container !
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
