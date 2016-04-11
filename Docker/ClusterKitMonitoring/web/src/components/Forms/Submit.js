import React, { Component, PropTypes } from 'react';

export default class Submit extends Component {
  static propTypes = {
    onClick: PropTypes.func.isRequired,
    text: PropTypes.string.isRequired,
    saving: PropTypes.bool,
    saved: PropTypes.bool,
    saveError: PropTypes.string
  }

  render() {
    const {onClick, text, saving, saveError, saved} = this.props;

    let saveClassName = 'fa fa-refresh';
    if (saving) {
      saveClassName += ' fa-spin';
    }

    return (
      <div className="col-xs-12 form-group">
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
          Saved
        </div>
        }

        <button type="button" className="btn btn-primary btn-lg" onClick={onClick}>
          <i className={saveClassName}/> {' '} {text}
        </button>
      </div>
    );
  }
}
