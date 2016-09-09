/*
 *
 * FeedsListPage
 *
 */

import React from 'react';
import { connect } from 'react-redux';
import selectFeedsListPage from './selectors';
import styles from './styles.css';

import FeedList from '../../components/FeedList'

import {
  feedsLoadAction
} from './actions';

export class FeedsListPage extends React.Component { // eslint-disable-line react/prefer-stateless-function

  componentWillMount() {
    const {dispatch} = this.props;
    dispatch(feedsLoadAction());
  }

  render() {
    return (
      <div className={styles.feedsListPage}>
        <FeedList feeds={this.props.feeds}/>
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
