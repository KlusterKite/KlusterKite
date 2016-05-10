import React, { Component, PropTypes } from 'react';
import {connect} from 'react-redux';
import * as actions from 'redux/modules/nugetfeeds';
import {isLoaded, load as loadOnInit} from 'redux/modules/nugetfeeds';
import { asyncConnect } from 'redux-async-connect';

@asyncConnect([{
  promise: ({store: {dispatch, getState}}) => {
    const promises = [];

    if (!isLoaded(getState())) {
      promises.push(dispatch(loadOnInit()));
    }

    return Promise.all(promises);
  }
}])
@connect(
  state => ({
    data: state.nugetfeeds.data,
    loading: state.nugetfeeds.loading,
    loaded: state.nugetfeeds.loaded
  }),
  {...actions })

export default class NugetFeedsList extends Component {
  static propTypes = {
    data: PropTypes.any,
    loading: PropTypes.bool,
    loaded: PropTypes.bool.isRequired,
    load: PropTypes.func.isRequired
  };

  componentDidMount() {
    const {loading, load} = this.props;

    if (!loading) {
      load();
    }
  }

  render() {
    const {loading, loaded, data} = this.props;

    let loadClassName = 'fa fa-refresh';
    if (loading) {
      loadClassName += ' fa-spin';
    }

    return (
        <div>
          {!loaded && loading &&
          <div className="container">
            <p><i className="fa fa-spinner fa-spin"></i> Loading data… </p>
          </div>
          }

          {loaded &&
          <table className="table table-hover">
            <thead>
                <tr>
                    <th>Address</th>
                    <th>Type</th>
                </tr>
            </thead>
            <tbody>
              {data && data.length && data.map((item) =>
                <tr key={item.Id}>
                  <td>
                    {item.Address}
                  </td>
                  <td>
                    {item.Type}
                  </td>
                </tr>
                )
              }
            </tbody>
          </table>
          }
        </div>
    );
  }
}
