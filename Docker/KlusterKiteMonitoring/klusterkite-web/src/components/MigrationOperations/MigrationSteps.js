import React from 'react';
import Relay from 'react-relay'

import CancelMigration from '../../components/MigrationOperations/CancelMigration'
import FinishMigration from '../../components/MigrationOperations/FinishMigration'
import UpdateNodes from '../../components/MigrationOperations/UpdateNodes'
import UpdateResources from '../../components/MigrationOperations/UpdateResources'

import './styles.css';

export class MigrationSteps extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      migrationSteps: null,
      currentMigrationStep: null,
    };

    this.replacements = {
      NodesUpdating: 'Updating Nodes',
      NodesUpdated: 'Nodes Updated',
      ResourcesUpdating: 'Updating Resources',
      ResourcesUpdated: 'Resources Updated',
    };
  }

  static propTypes = {
    resourceState: React.PropTypes.object,
    onStateChange: React.PropTypes.func.isRequired,
    onError: React.PropTypes.func.isRequired,
    operationIsInProgress: React.PropTypes.bool,
  };

  componentWillMount() {
    this.onReceiveProps(this.props);
  }

  componentWillReceiveProps(nextProps) {
    this.onReceiveProps(nextProps);
  }

  onReceiveProps(nextProps) {
    // We are mapping those props into state because we want to cache them in case of server downtime
    if (nextProps.resourceState.migrationSteps && nextProps.resourceState.migrationSteps.length > 0) {
      this.setState({
        migrationSteps: nextProps.resourceState.migrationSteps
      });
    }

    if (nextProps.resourceState.currentMigrationStep) {
      this.setState({
        currentMigrationStep: nextProps.resourceState.currentMigrationStep
      });
    }
  }

  render() {
    const migrationSteps = this.state.migrationSteps;
    const currentMigrationStep = this.state.currentMigrationStep;
    const activeIndex = migrationSteps ? migrationSteps.indexOf(currentMigrationStep) : -1;
    const lastIndex = migrationSteps ? migrationSteps.length - 1 : -1;
    const nodesUpdating = currentMigrationStep === 'NodesUpdating';
    const operationIsInProgress = nodesUpdating || this.props.operationIsInProgress || this.props.resourceState.operationIsInProgress;

    return (
      <div>
        {migrationSteps &&
          <div className="panel panel-default">
            <div className="panel-body">

              <ul className="migration-steps">
                {migrationSteps.map((step, index) => {
                  const className = index === activeIndex ? 'active' : '';
                  const classNameHrLeft = index === 0 ? 'empty' : (index <= activeIndex ? 'active' : '');
                  const classNameHrRight = index === lastIndex ? 'empty' : (activeIndex > index ? 'active' : '');
                  const title = this.replacements[step] ? this.replacements[step] : step;

                  return (
                    <li key={step} className={className}>
                      <hr className={classNameHrLeft}/>
                      <hr className={classNameHrRight}/>
                      <div>
                        <span className="index">{index + 1}</span>
                      </div>
                      <p className="title">{title}</p>

                      {index === 0 &&
                      <div className="migration-controls">
                        <CancelMigration
                          onStateChange={this.props.onStateChange}
                          onError={this.props.onError}
                          canCancelMigration={this.props.resourceState.canCancelMigration}
                          operationIsInProgress={operationIsInProgress}
                        />

                        <UpdateNodes
                          onStateChange={this.props.onStateChange}
                          onError={this.props.onError}
                          canUpdateBackward={this.props.resourceState.canUpdateNodesToSource}
                          operationIsInProgress={operationIsInProgress}
                        />
                      </div>
                      }

                      {index === lastIndex &&
                      <div className="migration-controls">
                        <UpdateNodes
                          onStateChange={this.props.onStateChange}
                          onError={this.props.onError}
                          canUpdateForward={this.props.resourceState.canUpdateNodesToDestination}
                          operationIsInProgress={operationIsInProgress}
                        />

                        <FinishMigration
                          onStateChange={this.props.onStateChange}
                          onError={this.props.onError}
                          canFinishMigration={this.props.resourceState.canFinishMigration}
                          operationIsInProgress={operationIsInProgress}
                        />
                      </div>
                      }
                    </li>
                  )
                })}
              </ul>
            </div>
          </div>
        }
        <UpdateResources
          onStateChange={this.props.onStateChange}
          onError={this.props.onError}
          migrationState={this.props.resourceState.migrationState}
          canMigrateResources={this.props.resourceState.canMigrateResources}
          operationIsInProgress={this.props.operationIsInProgress}
        />

      </div>
    );
  }
}

export default Relay.createContainer(
  MigrationSteps,
  {
    fragments: {
      resourceState: () => Relay.QL`fragment on IKlusterKiteNodeApi_ResourceState {
        operationIsInProgress
        canUpdateNodesToDestination
        canUpdateNodesToSource
        canCancelMigration
        canFinishMigration
        canMigrateResources
        migrationSteps
        currentMigrationStep
        migrationState {
          ${UpdateResources.getFragment('migrationState')},
        }
      }
      `,
    },
  },
)

