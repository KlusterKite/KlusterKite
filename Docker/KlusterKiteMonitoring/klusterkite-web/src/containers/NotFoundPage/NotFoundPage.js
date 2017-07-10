import React from 'react'

export default class NotFoundPage extends React.Component {
  static propTypes = {
    viewer: React.PropTypes.object,
  };
  render () {
    return (
      <div className="container">
        <h1>Page not found</h1>
        <h3>Uh oh! Looks like something broke! :(</h3>
      </div>
    )
  }
}