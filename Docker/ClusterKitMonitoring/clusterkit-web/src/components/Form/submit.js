import React from 'react';
import { Row } from 'formsy-react-components';
import Icon from 'react-fa';

import './styles.css';

export default class Submit extends React.Component { // eslint-disable-line react/prefer-stateless-function
  static propTypes = {
    canSubmit: React.PropTypes.bool.isRequired,
    buttonText: React.PropTypes.string,
    savedText: React.PropTypes.string,
    saving: React.PropTypes.bool,
    saved: React.PropTypes.bool,
    saveError: React.PropTypes.string,
    disabled: React.PropTypes.bool,
    onDelete: React.PropTypes.func,
  };

  render() {
    let saveClassName = '';
    if (this.props.saving) {
      saveClassName += ' fa-spin';
    }

    let text = 'Save';
    if (this.props.buttonText) {
      text = this.props.buttonText;
    }

    let savedText = 'Saved';
    if (this.props.savedText) {
      savedText = this.props.savedText;
    }

    let disabled = !this.props.canSubmit;
    if (this.props.disabled || this.props.saving){
      disabled = true;
    }

    const deleteText = 'Delete';

    return (
      <fieldset>
        <Row layout="horizontal">
          {this.props.saveError &&
          <div className="alert alert-danger" role="alert">
            <span className="glyphicon glyphicon-exclamation-sign" aria-hidden="true"></span>
            {' '}
            {this.props.saveError}
          </div>
          }

          {this.props.saved &&
          <div className="alert alert-success" role="alert">
            <span className="glyphicon glyphicon-ok" aria-hidden="true"></span>
            {' '}
            {savedText}
          </div>
          }

          <button className="btn btn-primary" disabled={disabled} type="submit">
            <Icon name="pencil" className={saveClassName} /> {' '} {text}
          </button>

          {this.props.onDelete &&
            <button className="btn btn-danger btn-margined" disabled={disabled} type="button" onClick={this.props.onDelete}>
              <Icon name="remove" className={saveClassName}/> {' '} {deleteText}
            </button>
          }
        </Row>
      </fieldset>
    );
  }
}
