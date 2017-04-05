import React from 'react';

import { Link } from 'react-router';
import FeedForm from '../FeedForm/index';

class FeedList extends React.Component {
  constructor (props) {
    super(props);
    this.state = {
      isEditing: false,
    }
  }

  static propTypes = {
    releaseId: React.PropTypes.string,
    feeds: React.PropTypes.object,
    onChange: React.PropTypes.func,
  };

  /**
   * Shows edit form for the selected feed
   * @param node {Object} Feed data or null (if new)
   */
  showEditForm(node) {
    this.setState({
      isEditing: true,
      editedObject: node
    });
  };

  /**
   * Creates or updates a feed record. Pushes data to the this.props.onChange method.
   * @param model {Object} New feed model
   */
  createOrUpdate(model) {
    const oldFeeds = this.props.feeds && this.props.feeds.edges;
    let newFeeds;

    if (model.id) {
      // updating
      const index = this.findIndex(oldFeeds, model.id);
      const newNode = this.updateNode(oldFeeds[index], model);

      newFeeds = [
        ...oldFeeds.slice(0, index),
        newNode,
        ...oldFeeds.slice(index + 1)
      ];
    } else {
      // creating
      const newNode = {
        node: model
      };
      newFeeds = [...oldFeeds, newNode];
    }

    this.setState({
      isEditing: false,
      editedObject: null
    });
    this.props.onChange(newFeeds);
  }

  /**
   * Updates feed's node in the old feed data with a new model
   * @param oldFeed {Object} Old feed data
   * @param newModel {Object} New feed model
   * @return {Object} New feed data
   */
  updateNode(oldFeed, newModel) {
    const newNode = Object.assign({}, oldFeed.node, newModel);

    return Object.assign({}, oldFeed, {
      node: newNode
    });
  }


  /**
   * Find index of a feed with a known id
   * @param feeds {Object} Feeds list
   * @param id {string} Feed's id to find
   * @return {number} Feed's index or -1, if not found
   */
  findIndex(feeds, id) {
    for (let i = 0; i < feeds.length; i++) {
      if (feeds[i].node.id === id) {
        return i;
      }
    }

    return -1;
  }

  render() {
    const feeds = this.props.feeds && this.props.feeds.edges;

    return (
      <div>
        {!this.state.isEditing &&
          <div>
            <h2>Nuget feeds list</h2>
            <Link to={`/clusterkit/NugetFeeds/${this.props.releaseId}/create`} className="btn btn-primary" role="button">Add a new feed</Link>
            <table className="table table-hover">
              <thead>
                <tr>
                  <th>Address</th>
                  <th>Type</th>
                </tr>
              </thead>
              <tbody>
              {feeds && feeds.length > 0 && feeds.map((item) =>
                <tr key={item.node.id || item.node.address}>
                  <td>
                    <Link to={`/clusterkit/NugetFeeds/${this.props.releaseId}/${encodeURIComponent(item.node.id)}`}>
                      {item.node.address}
                    </Link>
                  </td>
                  <td>
                    {item.node.type}
                  </td>
                </tr>
              )
              }
              </tbody>
            </table>
          </div>
        }
        {this.state.isEditing &&
          <div>
            <FeedForm initialValues={this.state.editedObject} onSubmit={(model) => this.createOrUpdate(model)} />
          </div>
        }
      </div>
    );
  }
}

export default FeedList
