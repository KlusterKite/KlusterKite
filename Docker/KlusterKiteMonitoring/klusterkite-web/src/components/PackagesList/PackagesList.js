import React from 'react';
import Relay from 'react-relay'

import './styles.css';

export class PackagesList extends React.Component { // eslint-disable-line react/prefer-stateless-function
  constructor(props) {
    super(props);

    this.state = {
      showUpdated: true,
    };
  }

  static propTypes = {
    configurationId: React.PropTypes.string,
    configuration: React.PropTypes.object,
    activeConfigurationPackages: React.PropTypes.object,
  };

  onUpdatedChange() {
    this.setState((prevState) => ({
      showUpdated: !prevState.showUpdated
    }));
  }

  render() {
    const packages = this.props.configuration && this.props.configuration.packages && this.props.configuration.packages.edges;
    const activeConfigurationPackages = this.props.activeConfigurationPackages.edges;

    let packagesFiltered = [];
    packages.forEach((item) => {
      const isOld = activeConfigurationPackages.some(element => (element.node.__id === item.node.__id && element.node.version === item.node.version));
      if (!isOld || !this.state.showUpdated) {
        packagesFiltered.push({
          id: item.node.id,
          name: item.node.__id,
          version: item.node.version,
          isNew: !isOld
        });
      }
    });

    return (
      <div>
        {packagesFiltered && packagesFiltered.length > 0 &&
        <div>
          <h3>Packages list</h3>

          <label className="checkbox-label"><input type="checkbox" checked={this.state.showUpdated} onChange={this.onUpdatedChange.bind(this)} /> Show changed only</label>
          <table className="table table-hover">
            <thead>
            <tr>
              <th>Id</th>
              <th>Version</th>
              <th>Changed</th>
            </tr>
            </thead>
            <tbody>
            {packagesFiltered.map((item) =>
              <tr key={item.id}>
                <td>
                  {item.name}
                </td>
                <td>
                  {item.version}
                </td>
                <td>
                  {item.isNew.toString()}
                </td>
              </tr>
            )}
            </tbody>
          </table>
        </div>
        }
      </div>
    );
  }
}

export default Relay.createContainer(
  PackagesList,
  {
    fragments: {
      configuration: () => Relay.QL`fragment on IKlusterKiteNodeApi_ConfigurationSettings {
        packages(sort: id_asc) {
          edges {
            node {
              version
              id
              __id
            }
          }
        }
      }
      `,
    },
  },
)

