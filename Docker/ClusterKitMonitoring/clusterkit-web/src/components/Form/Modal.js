import React from 'react';
import { Modal, Button } from 'react-bootstrap';

import './styles.css';

export default class ModalBlock extends React.Component {
  static propTypes = {
    title: React.PropTypes.string.isRequired,
    cancelText: React.PropTypes.string,
    confirmText: React.PropTypes.string,
    confirmClass: React.PropTypes.string,
    onCancel: React.PropTypes.func.isRequired,
    onConfirm: React.PropTypes.func.isRequired
  };

  render() {
    return (
      <div>
        <div className="static-modal static-modal-center">
          <Modal.Dialog>
            <Modal.Header>
              <Modal.Title>{this.props.title}</Modal.Title>
            </Modal.Header>

            <Modal.Body>
              {this.props.children}
            </Modal.Body>

            <Modal.Footer>
              <Button onClick={this.props.onCancel}>{this.props.cancelText || "Cancel"}</Button>
              <Button onClick={this.props.onConfirm} bsStyle={this.props.confirmClass || "danger"}>{this.props.confirmText || "Confirm"}</Button>
            </Modal.Footer>

          </Modal.Dialog>
        </div>
      </div>
    );
  }
}
