import React from 'react';
import Relay from 'react-relay'

export class PackagesList extends React.Component { // eslint-disable-line react/prefer-stateless-function

  static propTypes = {
    releaseId: React.PropTypes.string,
    configuration: React.PropTypes.object,
  };

  render() {
    let packages = this.props.configuration && this.props.configuration.packages && this.props.configuration.packages.edges;
    // Sorting on client is disabled
    // packages.sort((a, b) => {
    //   if (a.node.__id.toLowerCase() > b.node.__id.toLowerCase()) {
    //     return 1;
    //   }
    //   if (a.node.__id.toLowerCase() < b.node.__id.toLowerCase()) {
    //     return -1;
    //   }
    //   return 0;
    // });

    return (
      <div>
        {packages && packages.length > 0 &&
          <div>
          <h3>Packages list</h3>
          <table className="table table-hover">
            <thead>
            <tr>
              <th>Id</th>
              <th>Version</th>
            </tr>
            </thead>
            <tbody>
            {packages.map((item) =>
              <tr key={item.node.id}>
                <td>
                  {item.node.__id}
                </td>
                <td>
                  {item.node.version}
                </td>
              </tr>
            )
            }
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

