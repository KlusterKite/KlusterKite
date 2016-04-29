import React, { Component, PropTypes } from 'react';
import {connect} from 'react-redux';
import * as actions from 'redux/modules/templates';
import {isLoaded, load as loadOnInit} from 'redux/modules/templates';
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
    data: state.templates.data,
    loading: state.templates.loading,
    loaded: state.templates.loaded
  }),
  {...actions })

export default class TemplatesList extends Component {
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

          <a href="/templates/create/" className="btn btn-primary" role="button">Add a new template</a>

          {loaded &&
          <table className="table table-hover">
            <thead>
                <tr>
                    <th>Code</th>
                    <th>Name</th>
                    <th>Packages</th>
                    <th>Min</th>
                    <th>Max</th>
                    <th>Priority</th>
                    <th>Version</th>
                </tr>
            </thead>
            <tbody>
              {data && data.length && data.map((item) =>
                <tr key={item.Id}>
                  <td>
                    <a href={'/templates/edit/' + item.Id}>
                      {item.Code}
                    </a>
                  </td>
                  <td>{item.Name}</td>
                  <td>
                    {item.Packages.map((pack) =>
                        <span key={data.Id + '/' + pack}>
                          <span className="label label-default">{pack}</span>{' '}
                        </span>
                      )
                    }
                  </td>
                  <td>{item.MininmumRequiredInstances}</td>
                  <td>{item.MaximumNeededInstances}</td>
                  <td>{item.Priority}</td>
                  <td>{item.Version}</td>
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
