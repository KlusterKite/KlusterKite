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
    saveError: state.templates.saveError,
    createId: state.templates.createId
  }),
  {...actions, initialize })

export default class TemplatesEdit extends Component {
  static propTypes = {
    params: PropTypes.object,
    loading: PropTypes.bool,
    loaded: PropTypes.bool.isRequired,
    loadById: PropTypes.func.isRequired,
    saveData: PropTypes.func.isRequired,
    createRecord: PropTypes.func.isRequired,
    initialize: PropTypes.func.isRequired,
    data: PropTypes.object,
    saving: PropTypes.bool,
    saved: PropTypes.bool,
    saveError: PropTypes.string,
    createId: PropTypes.number,
    onRedirectStart: PropTypes.func.isRequired
  };

  componentDidMount() {
    const {loading, loadById} = this.props;

    if (!loading && this.props.params.id) {
      loadById(this.props.params.id);
    }
  }

  componentDidUpdate() {
    const {loaded, data, createId, onRedirectStart, loadById} = this.props;

    if (loaded && data) {
      this.props.initialize('template', data);
    }
    if (createId && createId > 0) {
      // Redirect to edit page
      const id = createId;
      onRedirectStart();
      loadById(id);
    }
  }

  handleSubmit = (data) => {
    const {saveData, createRecord} = this.props;

    if (data.Id) {
      saveData(data);
    } else {
      createRecord(data);
    }
  }

  render() {
    const {loaded, saving, saved, saveError} = this.props;
    const newItem = !this.props.params.id;
    const name = newItem ? 'Create' : 'Edit';

    return (
        <div className="container">
          <h1>{name} template</h1>

          {!loaded && !newItem &&
          <div className="container">
            <p><i className="fa fa-spinner fa-spin"></i> Loading data… </p>
          </div>
          }

          {(loaded || newItem) &&
            <TemplateForm onSubmit={this.handleSubmit} saving={saving} saved={saved} saveError={saveError} />
          }
        </div>
    );
  }
}
