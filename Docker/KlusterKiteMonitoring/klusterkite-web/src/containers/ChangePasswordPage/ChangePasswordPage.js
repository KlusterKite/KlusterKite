import React from 'react'
import Relay from 'react-relay'
import { browserHistory } from 'react-router'

import ChangePassword from '../../components/ChangePassword/ChangePassword'

import ChangePasswordMutation from './mutations/ChangePasswordMutation'

class ChangePasswordPage extends React.Component {

  static propTypes = {
    api: React.PropTypes.object,
    params: React.PropTypes.object,
  };

  static contextTypes = {
    router: React.PropTypes.object,
  };

  constructor (props) {
    super(props);
    this.state = {
      saving: false,
      saveErrors: null,
    }
  }

  getErrorMessagesFromEdge = (edges) => {
    return edges.map(x => x.node).map(x => x.message);
  };

  onResetPassword(model, userUid) {
    console.log('reset password', model, userUid);

    this.setState({
      saving: true
    });

    Relay.Store.commitUpdate(
      new ChangePasswordMutation(
        {
          oldPassword: model.passwordOld,
          newPassword: model.password,
        }),
      {
        onSuccess: (response) => {
          console.log('response', response);
          if (response.klusterKiteNodeApi_me_changePassword) {
            const result = response.klusterKiteNodeApi_me_changePassword.result;

            if (result && result.result) {
              this.setState({
                saving: false,
                saveErrors: null
              });

              browserHistory.push(`/klusterkite/`);
            } else if (result.errors && result.errors.edges) {
              const messages = this.getErrorMessagesFromEdge(result.errors.edges);

              this.setState({
                saving: false,
                saveErrors: messages
              });
            } else {
              this.setState({
                saving: false,
                saveErrors: ['Password change failed']
              });
            }
          }
        },
        onFailure: (transaction) => {
          this.setState({
            saving: false,
            saveErrors: ['Bad request']
          });
          console.log(transaction)},
      },
    )
  }

  render () {
    return (
      <div>
        <ChangePassword
          onSubmit={this.onResetPassword.bind(this)}
          saving={this.state.saving}
          saveErrors={this.state.saveErrors}
        />
      </div>
    )
  }
}

export default Relay.createContainer(
  ChangePasswordPage,
  {
    initialVariables: {
      id: null,
      nodeExists: false,
    },
    prepareVariables: (prevVariables) => Object.assign({}, prevVariables, {
      nodeExists: prevVariables.id !== null,
    }),
    fragments: {
      api: () => Relay.QL`
        fragment on IKlusterKiteNodeApi {
          id
        }
      `,
    },
  },
)
