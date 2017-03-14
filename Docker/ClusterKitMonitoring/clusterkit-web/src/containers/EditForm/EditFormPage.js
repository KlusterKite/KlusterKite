import React from 'react'
import { Form, Input, Row } from 'formsy-react-components';
import Relay from 'react-relay'

class EditFormPage extends React.Component {
  static propTypes = {
    viewer: React.PropTypes.object,
    onSubmit: React.PropTypes.func,
  };
  render () {
    return (
      <div>
        <Form onSubmit={(data) => { this.props.onSubmit(data) }}>
          <fieldset>
            <Input
              name="firstname"
              label="What is your first name?"
            />
          </fieldset>
          <fieldset>
            <Row layout="horizontal">
              <input className="btn btn-primary" formNoValidate={true} type="submit" defaultValue="Submit" />
            </Row>
          </fieldset>
        </Form>
      </div>
    )
  }
}

export default Relay.createContainer(
  EditFormPage,
  {
    fragments: {
      viewer: () => Relay.QL`
        fragment on ClusterKitNodeApi_ClusterKitNodeApi {
          nodeManagerData {
            getActiveNodeDescriptions
            {
              nodeId
              nodeTemplate
              nodeTemplateVersion
            }
          }
        }
      `,
    },
  },
)
