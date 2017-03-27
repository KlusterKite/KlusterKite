import React from 'react';

export default class PackagesList extends React.Component { // eslint-disable-line react/prefer-stateless-function

  static propTypes = {
    packages: React.PropTypes.array.isRequired,
  };

  render() {
    const { packages } = this.props;

    return (
      <div>
        <h2>Packages list</h2>

        <table className="table table-hover">
          <thead>
            <tr>
              <th>Id</th>
              <th>Version</th>
            </tr>
          </thead>
          <tbody>
          {packages && packages.length && packages.map((item) =>
            <tr key={item.Id}>
              <td>{item.Id}</td>
              <td>{item.Version}</td>
            </tr>
          )
          }
          </tbody>
        </table>
      </div>
    );
  }
}

