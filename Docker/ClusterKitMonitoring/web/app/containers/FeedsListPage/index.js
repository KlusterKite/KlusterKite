/*
 *
 * FeedsListPage
 *
 */

import React from 'react';
import { connect } from 'react-redux';
import selectFeedsListPage from './selectors';
import styles from './styles.css';

export class FeedsListPage extends React.Component { // eslint-disable-line react/prefer-stateless-function
  render() {
    return (
      <div className={styles.feedsListPage}>
      This is FeedsListPage container !
      </div>
    );
  }
}

const mapStateToProps = selectFeedsListPage();

function mapDispatchToProps(dispatch) {
  return {
    dispatch,
  };
}

export default connect(mapStateToProps, mapDispatchToProps)(FeedsListPage);
