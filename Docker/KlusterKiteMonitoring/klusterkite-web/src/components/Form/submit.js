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
    deleting: React.PropTypes.bool,
    saved: React.PropTypes.bool,
    saveErrors: React.PropTypes.arrayOf(React.PropTypes.string),
    saveError: React.PropTypes.string,
    disabled: React.PropTypes.bool,
    onCancel: React.PropTypes.func,
    onSubmit: React.PropTypes.func,
    onDelete: React.PropTypes.func,
  };

  onSubmit() {
    // This clutch is needed to prevent submitting on enter
    // Another possible hack with preventDefault on keyPress affects textareas and makes editing hard
    this.refs.submitButton.removeAttribute('disabled');
    this.refs.submitButton.click();
    this.refs.submitButton.setAttribute('disabled', true);
  }

  render() {
    let saveClassName = '';
    if (this.props.saving) {
      saveClassName += ' fa-spin';
    }

    let deleteClassName = '';
    if (this.props.deleting) {
      deleteClassName += ' fa-spin';
    }

    let text = 'Save';
    if (this.props.buttonText) {
      text = this.props.buttonText;
    }

    let savedText = 'Saved';
    if (this.props.savedText) {
      savedText = this.props.savedText;
    }

    let disabled = false;
    if (this.props.saving || this.props.deleting){
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

          {this.props.saveErrors && this.props.saveErrors.map((error, index) => {
            return (
            <div className="alert alert-danger" role="alert" key={`error-${index}`}>
              <span className="glyphicon glyphicon-exclamation-sign" aria-hidden="true"></span>
              {' '}
              {error}
            </div>
            );
            })
          }

          {this.props.saved &&
          <div className="alert alert-success" role="alert">
            <span className="glyphicon glyphicon-ok" aria-hidden="true"></span>
            {' '}
            {savedText}
          </div>
          }

          <button className="btn btn-primary" disabled={disabled} type={this.props.submitOnEnter ? 'submit' : 'button'} onClick={this.props.onSubmit}>
            <Icon name="pencil" className={saveClassName} /> {' '} {text}
          </button>

          {this.props.onCancel &&
            <button className="btn btn-default btn-margined" type="button" onClick={this.props.onCancel}>
              Cancel
            </button>
          }

          {this.props.onDelete &&
            <button className="btn btn-danger btn-margined" disabled={disabled} type="button" onClick={this.props.onDelete}>
              <Icon name="remove" className={deleteClassName}/> {' '} {deleteText}
            </button>
          }
        </Row>
      </fieldset>
    );
  }
}
