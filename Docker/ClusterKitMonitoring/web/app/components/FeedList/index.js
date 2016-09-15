import React, { Component, PropTypes } from 'react';
import { Link } from 'react-router';


export default class FeedList extends Component {

  static propTypes = {
    feeds: PropTypes.array.isRequired,
  }

  getTypeName(type) {
    switch (type) {
      case 0:
        return 'Public';
      case 1:
        return 'Private';
      default:
        return 'undefined';
    }
  }

  render() {
    const { feeds } = this.props;

    return (
      <div>
        <h2>Nuget feeds list</h2>
        <Link to="/clusterkit/nugetfeeds/create/" className="btn btn-primary" role="button">Add a new feed</Link>
        <table className="table table-hover">
          <thead>
            <tr>
              <th>Address</th>
              <th>Type</th>
            </tr>
          </thead>
          <tbody>
          {feeds && feeds.length > 0 && feeds.map((item) =>
            <tr key={item.Id}>
              <td>
                <Link to={`/clusterkit/nugetfeeds/${item.Id}`}>
                {item.Address}
                </Link>
              </td>
              <td>
                {this.getTypeName(item.Type)}
              </td>
            </tr>
          )
          }
          </tbody>
        </table>


      </div>
    );
  }
}

