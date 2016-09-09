import React, {Component} from 'react';
import { Link } from 'react-router'
import styles from './styles.css';


export default class FeedList extends Component {


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
    const {feeds} = this.props;




    return (
      <div className={styles.feedList}>
        <h2>Nuget feeds list</h2>
        <a href="/clusterkit/nugetfeeds/create/" className="btn btn-primary" role="button">Add a new feed</a>
        <table className="table table-hover">
          <thead>
          <tr>
            <th>Address</th>
            <th>Type</th>
          </tr>
          </thead>
          <tbody>
          {feeds && feeds.length && feeds.map((item) =>
            <tr key={item.Id}>
              <td>
                <Link to={'/clusterkit/nugetfeeds/' + item.Id}>
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

