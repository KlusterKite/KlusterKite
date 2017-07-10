import React from 'react'
import Relay from 'react-relay'
import { browserHistory } from 'react-router'

import Storage from '../../utils/ttl-storage';

class GetPrivilegesPage extends React.Component {
  static propTypes = {
    api: React.PropTypes.object,
  };

  constructor (props) {
    super(props);
    this.state = {
      error: false,
    };
  }


  componentWillMount() {
    if (this.props && this.props.api && this.props.api.me) {
      this.savePrivilegesAndRedirect(this.props.api.me.klusterKiteUserPrivileges);
    } else {
      this.setState({error: true});
    }
  }

  /**
   * Saves privileges list to the local storage; redirect user to the page he came from
   * @param data {string[]} - Privileges list
   */
  savePrivilegesAndRedirect(data) {
    Storage.set('privileges', JSON.stringify(data));

    this.setState({
      privilegesReceived: true
    });

    if (this.props.location && this.props.location.query && this.props.location.query.from) {
      browserHistory.push(decodeURIComponent(this.props.location.query.from));
    } else {
      browserHistory.push('/klusterkite/');
    }
  }

  render () {
    return (
      <div>
        <h1>Authorization</h1>

        {this.state.error &&
          <p>Unknown error retrieving privileges list. Please wait and refresh page after a while.</p>
        }
        {!this.state.error &&
          <p>Please wait…</p>
        }
      </div>
    )
  }
}

export default Relay.createContainer(
  GetPrivilegesPage,
  {
    fragments: {
      api: () => Relay.QL`fragment on IKlusterKiteNodeApi {
        __typename
        me {
          klusterKiteUserPrivileges
        }
      }
      `,
    }
  },
)
