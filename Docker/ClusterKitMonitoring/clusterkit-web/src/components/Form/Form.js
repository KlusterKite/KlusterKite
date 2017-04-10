import React from 'react';
import { Form as FormsyForm } from 'formsy-react-components';

import Submit from './submit';

import './styles.css';

export default class Form extends React.Component { // eslint-disable-line react/prefer-stateless-function
  constructor(props) {
    super(props);
    this.submit = this.submit.bind(this);
    this.enableButton = this.enableButton.bind(this);
    this.disableButton = this.disableButton.bind(this);

    this.state = {
      canSubmit: false
    };
  }

  static propTypes = {
    onSubmit: React.PropTypes.func.isRequired,
    onCancel: React.PropTypes.func,
    onDelete: React.PropTypes.func,
    className: React.PropTypes.string,
    saving: React.PropTypes.bool,
    deleting: React.PropTypes.bool,
    saved: React.PropTypes.bool,
    saveErrors: React.PropTypes.arrayOf(React.PropTypes.string),
    saveError: React.PropTypes.string,
    buttonText: React.PropTypes.string,
    savedText: React.PropTypes.string,
    disabled: React.PropTypes.bool,
  };

  enableButton() {
    this.setState({
      canSubmit: true
    });
  }

  disableButton() {
    this.setState({
      canSubmit: false
    });
  }

  submit(model) {
    this.props.onSubmit(model);
  }

  onKeyPress(event) {
    if (event.which === 13 /* Enter */) {
      event.preventDefault();
    }
  }

  render() {
    return (
      <FormsyForm
        onKeyPress={this.onKeyPress}
        onValidSubmit={this.submit}
        onValid={this.enableButton}
        onInvalid={this.disableButton}
        className={this.props.className}
        validatePristine={true}
      >
        {this.props.children}
        <Submit
          canSubmit={this.state.canSubmit}
          saving={this.props.saving}
          deleting={this.props.deleting}
          saved={this.props.saved}
          disabled={this.props.disabled}
          saveError={this.props.saveError}
          saveErrors={this.props.saveErrors}
          buttonText={this.props.buttonText}
          savedText={this.props.savedText}
          onDelete={this.props.onDelete}
          onCancel={this.props.onCancel}
        />
      </FormsyForm>
    );
  }
}
