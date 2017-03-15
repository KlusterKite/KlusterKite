import React from 'react';
import Relay from 'react-relay'

import ReloadPackagesMutation from './mutations/ReloadPackagesMutation'

class ReloadPackages extends React.Component {

  static propTypes = {
    feeds: React.PropTypes.object,
  };

  handleReload = () => {
    console.log('Inner reload packages');

    Relay.Store.commitUpdate(
      new ReloadPackagesMutation({}),
      {
        onSuccess: () => console.log('success'),
        onFailure: (transaction) => console.log(transaction),
      },
    )
  };

  render() {
    return (
      <div>
        <button type="button" className="btn btn-primary btn-lg" onClick={this.handleReload}>
          <i className="fa fa-refresh"/> {' '} Reload packages
        </button>
      </div>
    );
  }
}

export default ReloadPackages
