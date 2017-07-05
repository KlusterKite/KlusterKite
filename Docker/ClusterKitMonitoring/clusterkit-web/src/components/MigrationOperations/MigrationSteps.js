import React from 'react';

import CancelMigration from '../../components/MigrationOperations/CancelMigration'
import FinishMigration from '../../components/MigrationOperations/FinishMigration'
import UpdateNodes from '../../components/MigrationOperations/UpdateNodes'

import './styles.css';

export class MigrationSteps extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      migrationSteps: null,
    };

    this.replacements = {
      NodesUpdating: 'Updating Nodes'
    };
  }

  static propTypes = {
    migrationSteps: React.PropTypes.arrayOf(React.PropTypes.string),
    currentMigrationStep: React.PropTypes.string,
    canUpdateForward: React.PropTypes.bool,
    canUpdateBackward: React.PropTypes.bool,
    onStateChange: React.PropTypes.func.isRequired,
    onError: React.PropTypes.func.isRequired,
    operationIsInProgress: React.PropTypes.bool,
  };

  componentWillReceiveProps(nextProps) {
    if (nextProps.migrationSteps) {
      this.setState({
        migrationSteps: nextProps.migrationSteps
      });
    }
  }

  render() {
    const activeIndex = this.state.migrationSteps ? this.state.migrationSteps.indexOf(this.props.currentMigrationStep) : -1;
    const lastIndex = this.state.migrationSteps ? this.state.migrationSteps.length - 1 : -1;
    const nodesUpdating = this.props.currentMigrationStep === 'NodesUpdating';

    return (
      <div className="panel panel-default">
        <div className="panel-body">

          <ul className="migration-steps">
            {this.state.migrationSteps && this.state.migrationSteps.map((step, index) => {
              const className = index === activeIndex ? 'active' : '';
              const classNameHrLeft = index === 0 ? 'empty' : (index <= activeIndex ? 'active' : '');
              const classNameHrRight = index === lastIndex ? 'empty' : (activeIndex > index ? 'active' : '');
              const title = this.replacements[step] ? this.replacements[step] : step;

              return (
                <li key={step} className={className}>
                  <hr className={classNameHrLeft} />
                  <hr className={classNameHrRight} />
                  <div>
                    <span className="index">{index + 1}</span>
                  </div>
                  <p className="title">{title}</p>

                  {index === 0 &&
                    <div className="migration-controls">
                      <CancelMigration
                        onStateChange={this.props.onStateChange}
                        onError={this.props.onError}
                        canCancelMigration={this.props.canCancelMigration}
                        operationIsInProgress={nodesUpdating || this.props.operationIsInProgress}
                      />

                      <UpdateNodes
                        onStateChange={this.props.onStateChange}
                        onError={this.props.onError}
                        canUpdateBackward={this.props.canUpdateBackward}
                        operationIsInProgress={nodesUpdating || this.props.operationIsInProgress}
                      />
                    </div>
                  }

                  {index === lastIndex &&
                    <div className="migration-controls">
                      <UpdateNodes
                        onStateChange={this.props.onStateChange}
                        onError={this.props.onError}
                        canUpdateForward={this.props.canUpdateForward}
                        operationIsInProgress={nodesUpdating || this.props.operationIsInProgress}
                      />

                      <FinishMigration
                        onStateChange={this.props.onStateChange}
                        onError={this.props.onError}
                        canFinishMigration={this.props.canFinishMigration}
                        operationIsInProgress={nodesUpdating || this.props.operationIsInProgress}
                      />
                    </div>
                  }
                </li>
              )
            })}
          </ul>
        </div>
      </div>
    );
  }
}

export default MigrationSteps
