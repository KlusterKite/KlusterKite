import React from 'react';

import './styles.css';

export default class ObsoleteOperations extends React.Component {
  static propTypes = {
    currentState: React.PropTypes.string.isRequired,
  };

  render() {
    return (
    <div>
      {this.props.currentState && this.props.currentState === 'Obsolete' &&
        <p>Configuration is obsolete. No actions possible.</p>
      }
    </div>
    );
  }
}
