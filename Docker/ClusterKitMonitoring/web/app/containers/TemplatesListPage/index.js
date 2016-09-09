/*
 *
 * TemplatesListPage
 *
 */

import React from 'react';
import { connect } from 'react-redux';
import selectTemplatesListPage from './selectors';
import styles from './styles.css';

import {
  templatesLoadAction
} from './actions';

import TemplatesList from '../../components/TemplatesList'

export class TemplatesListPage extends React.Component { // eslint-disable-line react/prefer-stateless-function


  componentWillMount() {
    const {dispatch} = this.props;
    dispatch(templatesLoadAction());
  }


  render() {
    return (
      <div className={styles.templatesListPage}>
        <TemplatesList templates={this.props.templates}/>
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
