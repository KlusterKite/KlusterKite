import React, { Component, PropTypes } from 'react';
import {connect} from 'react-redux';
import {initialize} from 'redux-form';
import * as actions from 'redux/modules/templates';
import {TemplateForm} from 'components';

@connect(
  state => ({
    data: state.templates.data,
    loading: state.templates.loading,
    loaded: state.templates.loaded,
    saving: state.templates.saving,
    saved: state.templates.saved,
    saveError: state.templates.saveError
  }),
  {...actions, initialize })

export default class TemplatesEdit extends Component {
  static propTypes = {
    params: PropTypes.object,
    loading: PropTypes.bool,
    loaded: PropTypes.bool.isRequired,
    loadById: PropTypes.func.isRequired,
    update: PropTypes.func.isRequired,
    initialize: PropTypes.func.isRequired,
    data: PropTypes.object,
    saving: PropTypes.bool,
    saved: PropTypes.bool,
    saveError: PropTypes.string
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
    const {update} = this.props;

    update(data);
  }

  render() {
    const {loaded, saving, saved, saveError} = this.props;

    return (
        <div className="container">
          <h1>Edit template</h1>

          {!loaded &&
          <div className="container">
            <p><i className="fa fa-spinner fa-spin"></i> Loading data… </p>
          </div>
          }

          {loaded &&
            <TemplateForm onSubmit={this.handleSubmit} saving={saving} saved={saved} saveError={saveError} />
          }
        </div>
    );
  }
}
