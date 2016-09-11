/*
 *
 * FeedPage
 *
 */

import { withRouter } from 'react-router';
import { autobind } from 'core-decorators';
import React, { Component, PropTypes } from 'react';
import { connect } from 'react-redux';
import { shallowEqualImmutable } from 'react-immutable-render-mixin';

import selectFeedPage from './selectors';

import FeedForm from '../../components/FeedForm';

import {
  feedLoadAction,
  feedSetLoadedAction,
  feedCreateAction,
  feedUpdateAction,
} from './actions';

@withRouter
export class FeedPage extends Component { // eslint-disable-line react/prefer-stateless-function

  static propTypes = {
    dispatch: PropTypes.func.isRequired,
    params: PropTypes.object,
    location: PropTypes.object.isRequired,
    feed: PropTypes.object.isRequired,
    updateError: PropTypes.string,
    isLoaded: PropTypes.bool.isRequired,
  }

  componentWillMount() {
    const { dispatch, params: { id } } = this.props;

    if (id !== 'create') {
      dispatch(feedLoadAction(id));
    } else {
      dispatch(feedSetLoadedAction());
    }
  }

  componentWillReceiveProps(nextProps) {
    if (nextProps.updateError && nextProps.updateError !== this.props.updateError && this.onError) {
      this.onError(nextProps.updateError);
      this.onSuccess = null;
      this.onError = null;
    }

    if (!shallowEqualImmutable(this.props.feed, nextProps.feed) && this.onSuccess) {
      this.onSuccess();
      this.onSuccess = null;
      this.onError = null;

      if (this.props.feed.Id !== nextProps.feed.Id) {
        const newPath = `/${this.props.location.pathname
          .split('/')
          .filter(p => p)
          .slice(0, -1)
          .join('/')}/${nextProps.feed.Id}`;

        const newLocation = {
          ...this.props.location,
          pathname: newPath,
        };

        nextProps.router.push(newLocation);
      }
    }
  }

  onSuccess = null;
  onError = null;

  @autobind
  create(feed, onSuccess, onError) {
    const { dispatch } = this.props;
    this.onSuccess = onSuccess;
    this.onError = onError;
    dispatch(feedCreateAction(feed, onSuccess, onError));
  }

  @autobind
  update(feed, onSuccess, onError) {
    const { dispatch } = this.props;
    this.onSuccess = onSuccess;
    this.onError = onError;
    dispatch(feedUpdateAction(feed, onSuccess, onError));
  }

  render() {
    const { feed, params: { id }, isLoaded } = this.props;
    const newItem = (id === 'create');
    const name = newItem ? 'Create' : 'Edit';

    return (
      <div>
        <h1>{name} feed</h1>
        {isLoaded && <FeedForm initialValues={feed} onSave={newItem ? this.create : this.update} />}
        {!isLoaded && <div className="alert alert-info"><i className="fa fa-refresh fa-spin" /> Loading...</div>}
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

