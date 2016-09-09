/*
 *
 * FeedPage
 *
 */

import React from 'react';
import { connect } from 'react-redux';
import selectFeedPage from './selectors';
import styles from './styles.css';

export class FeedPage extends React.Component { // eslint-disable-line react/prefer-stateless-function
  render() {
    return (
      <div className={styles.feedPage}>
      This is FeedPage container !
      </div>
    );
  }
}

const mapStateToProps = selectFeedPage();

function mapDispatchToProps(dispatch) {
  return {
    dispatch,
  };
}

export default connect(mapStateToProps, mapDispatchToProps)(FeedPage);
