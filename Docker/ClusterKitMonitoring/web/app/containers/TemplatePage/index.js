/*
 *
 * TemplatePage
 *
 */
import { withRouter } from 'react-router';
import { autobind } from 'core-decorators';
import React, { Component, PropTypes } from 'react';
import { connect } from 'react-redux';
import { shallowEqualImmutable } from 'react-immutable-render-mixin';

import selectTemplatePage from './selectors';

import TemplateForm from '../../components/TemplateForm';

import {
  templateLoadAction,
  templateSetLoadedAction,
  templateCreateAction,
  templateUpdateAction,
} from './actions';

@withRouter
export class TemplatePage extends Component { // eslint-disable-line react/prefer-stateless-function

  static propTypes = {
    dispatch: PropTypes.func.isRequired,
    params: PropTypes.object,
    updateError: PropTypes.string,
    isLoaded: PropTypes.bool.isRequired,
    template: PropTypes.object.isRequired,
    location: PropTypes.object.isRequired,
  }

  componentWillMount() {
    const { dispatch, params: { id } } = this.props;

    if (id !== 'create') {
      dispatch(templateLoadAction(id));
    } else {
      dispatch(templateSetLoadedAction());
    }
  }

  componentWillReceiveProps(nextProps) {
    if (nextProps.updateError && nextProps.updateError !== this.props.updateError && this.onError) {
      this.onError(nextProps.updateError);
      this.onSuccess = null;
      this.onError = null;
    }

    if (!shallowEqualImmutable(this.props.template, nextProps.template) && this.onSuccess) {
      this.onSuccess();
      this.onSuccess = null;
      this.onError = null;

      if (this.props.template.Id !== nextProps.template.Id) {
        const newPath = `/${this.props.location.pathname
          .split('/')
          .filter(p => p)
          .slice(0, -1)
          .join('/')}/${nextProps.template.Id}`;

        // console.log('pushing new path', newPath);
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
  create(template, onSuccess, onError) {
    const { dispatch } = this.props;
    this.onSuccess = onSuccess;
    this.onError = onError;
    dispatch(templateCreateAction(template, onSuccess, onError));
  }

  @autobind
  update(template, onSuccess, onError) {
    const { dispatch } = this.props;
    this.onSuccess = onSuccess;
    this.onError = onError;
    dispatch(templateUpdateAction(template, onSuccess, onError));
  }

  render() {
    const { template, params: { id }, isLoaded } = this.props;
    const newItem = (id === 'create');
    const name = newItem ? 'Create' : 'Edit';

    return (
      <div>
        <h1>{name} template</h1>
        {isLoaded && <TemplateForm initialValues={template} onSave={newItem ? this.create : this.update} />}
        {!isLoaded && <div className="alert alert-info"><i className="fa fa-refresh fa-spin" /> Loading...</div>}
      </div>
    );
  }
}

const mapStateToProps = selectTemplatePage();

function mapDispatchToProps(dispatch) {
  return {
    dispatch,
  };
}

export default connect(mapStateToProps, mapDispatchToProps)(TemplatePage);
