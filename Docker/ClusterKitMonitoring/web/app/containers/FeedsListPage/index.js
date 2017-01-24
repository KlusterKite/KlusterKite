/*
 *
 * FeedsListPage
 *
 */

import React, { Component, PropTypes } from 'react';
import { connect } from 'react-redux';
import selectFeedsListPage from './selectors';

import FeedList from '../../components/FeedList';

import {
  feedsLoadAction,
} from './actions';

export class FeedsListPage extends Component { // eslint-disable-line react/prefer-stateless-function

  static propTypes = {
    dispatch: PropTypes.func.isRequired,
    feeds: PropTypes.array.isRequired,
  }

  componentWillMount() {
    const { dispatch } = this.props;
    dispatch(feedsLoadAction());
  }

  render() {
    return (
      <div className="container">
        <FeedList feeds={this.props.feeds} />
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
