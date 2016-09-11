/*
 *
 * TemplatesListPage
 *
 */

import React, { Component, PropTypes } from 'react';
import { connect } from 'react-redux';
import selectTemplatesListPage from './selectors';

import {
  templatesLoadAction,
} from './actions';

import TemplatesList from '../../components/TemplatesList';

export class TemplatesListPage extends Component { // eslint-disable-line react/prefer-stateless-function

  static propTypes = {
    dispatch: PropTypes.func.isRequired,
    templates: PropTypes.array.isRequired,
  }

  componentWillMount() {
    const { dispatch } = this.props;
    dispatch(templatesLoadAction());
  }


  render() {
    return (
      <div>
        <TemplatesList templates={this.props.templates} />
      </div>
    );
  }
}

const mapStateToProps = selectTemplatesListPage();

function mapDispatchToProps(dispatch) {
  return {
    dispatch,
  };
}

export default connect(mapStateToProps, mapDispatchToProps)(TemplatesListPage);
