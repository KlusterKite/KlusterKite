import React from 'react';

export default class SwaggerLinksList extends React.Component { // eslint-disable-line react/prefer-stateless-function

  static propTypes = {
    links: React.PropTypes.array.isRequired,
  }

  render() {
    const { links } = this.props;


    return (
      <div>
        <h3>Swagger API descriptions</h3>

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
