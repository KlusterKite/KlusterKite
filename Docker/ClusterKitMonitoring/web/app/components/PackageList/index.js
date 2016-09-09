import React, {Component} from 'react';
import { Link } from 'react-router'

import styles from './styles.css';


export default class PackagesList extends Component {

  render() {
    const {packages} = this.props;

    return (
      <div className={styles.nodesList}>
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

