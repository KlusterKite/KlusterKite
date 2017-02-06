import React, { Component, PropTypes } from 'react';

export default class Submit extends Component { // eslint-disable-line react/prefer-stateless-function
  static propTypes = {
    onClick: PropTypes.func.isRequired,
    text: PropTypes.string.isRequired,
    saving: PropTypes.bool,
    saved: PropTypes.bool,
    saveError: PropTypes.string,
    valid: PropTypes.bool.isRequired,
    savedText: PropTypes.string,
  }

  render() {
    const { onClick, text, saving, saveError, saved, valid, savedText } = this.props;
    const savedTextStr = savedText != null ? savedText : 'Saved';

    let saveClassName = 'fa fa-refresh';
    if (saving) {
      saveClassName += ' fa-spin';
    }

    return (
      <div className="form-group">
        {saveError &&
          <div className="alert alert-danger" role="alert">
            <span className="glyphicon glyphicon-exclamation-sign" aria-hidden="true"></span>
            {' '}
            {saveError}
          </div>
        }

        {saved &&
          <div className="alert alert-success" role="alert">
            <span className="glyphicon glyphicon-ok" aria-hidden="true"></span>
            {' '}
            {savedTextStr}
          </div>
        }

        <button type="button" className="btn btn-primary btn-lg" onClick={onClick} disabled={!valid}>
          <i className={saveClassName} /> {' '} {text}
        </button>
      </div>
    );
  }
}
