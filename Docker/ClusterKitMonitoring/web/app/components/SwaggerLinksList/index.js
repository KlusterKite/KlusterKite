/**
*
* SwaggerLinksList
*
*/

import React, { Component, PropTypes } from 'react';

export default class SwaggerLinksList extends Component { // eslint-disable-line react/prefer-stateless-function

  static propTypes = {
    links: PropTypes.array.isRequired,
  }

  render() {
    const { links } = this.props;


    return (
      <div>
        <h2>Swagger API descriptions</h2>

        <table className="table table-hover">
          <thead>
            <tr>
              <th>Address</th>
            </tr>
          </thead>
          <tbody>
          {links && links.map((link, index) =>
            <tr key={`link${index}`}>
              <td><a href={`/${link}/index`} target="_blank" >{link}</a></td>
            </tr>
          )
          }
          </tbody>
        </table>

      </div>
    );
  }
}

