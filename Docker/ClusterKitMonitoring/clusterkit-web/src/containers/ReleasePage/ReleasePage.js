import React from 'react'
import Relay from 'react-relay'
import { browserHistory } from 'react-router'

import ReleaseForm from '../../components/ReleaseForm/index'
import FeedsList from '../../components/FeedsList/index'
import TemplatesList from '../../components/TemplatesList/index'

// import CreateFeedMutation from './mutations/CreateFeedMutation'
import UpdateReleaseMutation from './mutations/UpdateReleaseMutation'
// import DeleteFeedMutation from './mutations/DeleteFeedMutation'
import { hasPrivilege } from '../../utils/privileges'

class ReleasePage extends React.Component {

  static propTypes = {
    api: React.PropTypes.object,
    params: React.PropTypes.object,
  }

  static contextTypes = {
    router: React.PropTypes.object,
  }

  constructor (props) {
    super(props)
    this.state = {
      model: null
    }
  }

  componentWillMount() {
    if (this.props.api && this.props.api.__node) {
      this.setState({
        model: this.props.api.__node
      });
    }
  }

  componentWillReceiveProps(nextProps) {
    console.log('componentWillReceiveProps', nextProps);
    if (nextProps.api && nextProps.api.__node) {
      this.setState({
        model: nextProps.api.__node
      });
    }
  }

  isAddNew = () => {
    return !this.props.params.hasOwnProperty('id')
  }

  onSubmit = (releaseModel) => {
    console.log('submitting release', releaseModel);
    console.log('current model', this.state.model);

    const newModel = this.updateStateRelease(releaseModel);

    if (this.isAddNew()){
      this.addNode(newModel);
    } else {
      this.editNode(newModel);
    }
  }

  addNode = (model) => {
    console.log('create', model);
    // Relay.Store.commitUpdate(
    //   new CreateFeedMutation(
    //     {
    //       clusterKitNodesApiId: this.props.api.clusterKitNodesApi.id,
    //       userName: model.userName,
    //       password: model.password,
    //       address: model.address,
    //       type: model.type,
    //     }),
    //   {
    //     onSuccess: () => browserHistory.push('/clusterkit/NugetFeeds'),
    //     onFailure: (transaction) => console.log(transaction),
    //   },
    // )
  }

  editNode = (model) => {
    console.log('edit', model);
    Relay.Store.commitUpdate(
      new UpdateReleaseMutation(
        {
          nodeId: this.props.params.id,
          __id: model.__id,
          majorVersion: model.majorVersion,
          minorVersion: model.minorVersion,
          name: model.name,
          notes: model.notes,
          configuration: model.configuration
        }),
      {
        onSuccess: (transaction) => browserHistory.push('/clusterkit/releases'),
        onFailure: (transaction) => console.log(transaction),
      },
    )
  }

  onDelete = () => {
    console.log('delete', this.props.api.__node.__id);
    // Relay.Store.commitUpdate(
    //   new DeleteFeedMutation({deletedId: this.props.api.__node.__id}),
    //   {
    //     onSuccess: () => this.context.router.replace('/clusterkit/NugetFeeds'),
    //     onFailure: (transaction) => console.log(transaction),
    //   },
    // )
  }

  /**
   * Update model's nugetFeeds list with the new one, save if to the state
   * @param nugetFeeds {Object[]} List of new nuget feeds
   */
  updateStateFeed = (nugetFeeds) => {
    this.setState((prevState, props) => {
      const newModel = Object.assign({}, prevState.model);
      newModel.configuration.nugetFeeds.edges = nugetFeeds;

      return ({
        model: newModel
      });
    });
  };

  /**
   * Update model's release info the new one, save if to the state
   * @param release {Object} Release info
   * @return {Object} new data model
   */
  updateStateRelease = (release) => {
    const newModel = Object.assign({}, this.state.model);
    const keys = Object.keys(release);

    keys.forEach(key => {
      newModel[key] = release[key];
    });

    this.setState({
      model: newModel
    });

    return newModel;
  };

  render () {
    const model = this.state.model;
    return (
      <div>
        {model &&
        <div>
          <ReleaseForm onSubmit={this.onSubmit} onDelete={this.onDelete} initialValues={model}/>
          <FeedsList
            configuration={model.configuration}
            onChange={this.updateStateFeed}
            releaseId={this.props.params.id}
          />
          <TemplatesList
            configuration={model.configuration}
            nodeTemplates={model.configuration.nodeTemplates}
            createNodeTemplatePrivilege={hasPrivilege('ClusterKit.NodeManager.NodeTemplate.Create')}
            getNodeTemplatePrivilege={hasPrivilege('ClusterKit.NodeManager.NodeTemplate.Get')}
            releaseId={this.props.params.id}
          />
        </div>
        }
      </div>
    )
  }
}

export default Relay.createContainer(
  ReleasePage,
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
        fragment on IClusterKitNodeApi {
          id
          __node(id: $id) @include( if: $nodeExists ) {
            ...on IClusterKitNodeApi_Release {
              __id
              name
              notes
              minorVersion
              majorVersion
              state
              configuration {
                ${FeedsList.getFragment('configuration')},
                nodeTemplates {
                  edges {
                    node {
                      id
                      code
                      configuration
                      containerTypes
                      minimumRequiredInstances
                      maximumNeededInstances
                      name
                      packageRequirements {
                        edges {
                          node {
                            __id
                            specificVersion
                          }
                        }
                      }
                      priority
                    }
                  }
                }
                packages {
                  edges {
                    node {
                      name
                      version
                      availableVersions
                      id
                      __id
                    }
                  }
                }
                seedAddresses
                id
              }
            }
          }
        }
      `,
    },
  },
)
