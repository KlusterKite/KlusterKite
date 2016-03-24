import React, { Component, PropTypes } from 'react';
import {connect} from 'react-redux';
import {initialize} from 'redux-form';
import connectData from 'helpers/connectData';
import * as actions from 'redux/modules/templates';
import {TemplateForm} from 'components';

function fetchDataDeferred() {
  return null;
}

@connectData(null, fetchDataDeferred)
@connect(
  state => ({
    data: state.templates.data,
    loading: state.templates.loading,
    loaded: state.templates.loaded
  }),
  {...actions, initialize })

export default class TemplatesEdit extends Component {
  static propTypes = {
    params: PropTypes.object,
    loading: PropTypes.bool,
    loaded: PropTypes.bool.isRequired,
    loadById: PropTypes.func.isRequired,
    initialize: PropTypes.func.isRequired,
    data: PropTypes.object
  };

  componentDidMount() {
    const {loading, loadById} = this.props;

    if (!loading) {
      loadById(this.props.params.id);
    }
  }

  componentDidUpdate() {
    const {loaded, data} = this.props;

    if (loaded && data) {
      this.props.initialize('template', data);
    }
  }

  handleSubmit = (data) => {
    window.alert('Data submitted! ' + JSON.stringify(data));
  }

  render() {
    const {loading, loaded, data} = this.props;

    let loadClassName = 'fa fa-refresh';
    if (loading) {
      loadClassName += ' fa-spin';
    }

    return (
        <div className="container">
          <h1>Edit</h1>

          {!loaded && loading &&
          <div className="container">
            <p><i className="fa fa-spinner fa-spin"></i> Loading data… </p>
          </div>
          }

          <TemplateForm onSubmit={this.handleSubmit} data={data}/>
        </div>
    );
  }
}
