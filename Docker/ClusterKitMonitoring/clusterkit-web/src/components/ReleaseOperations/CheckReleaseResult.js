import React from 'react';

import isEqual from 'lodash/isEqual';

export default class CheckReleaseResult extends React.Component { // eslint-disable-line react/prefer-stateless-function
  constructor(props) {
    super(props);

    this.state = {
      nodesToBeUpdated: false,
    };
  }

  static propTypes = {
    newReleaseInnerId: React.PropTypes.number.isRequired,
    activeNodes: React.PropTypes.arrayOf(React.PropTypes.object).isRequired,
    compatibleTemplates: React.PropTypes.arrayOf(React.PropTypes.object).isRequired,
    newNodeTemplates: React.PropTypes.arrayOf(React.PropTypes.object).isRequired,
  };

  compareNodes(activeNodes, compatibleTemplates, newNodeTemplates) {
    let nodesToBeUpdated = [];

    activeNodes.forEach((item) => {
      const node = {
        template: item.node.nodeTemplate,
        releaseId: item.node.releaseId
      };

      if (node.template) {
        const indexInRelease = this.findNodeTemplateInRelease(node.template, newNodeTemplates);

        if (indexInRelease === -1) {
          // If a template of an active node is not found in the new release, node will be updated
          nodesToBeUpdated.push(node);
        } else {
          // If node releaseId does not mach new releaseId AND
          // it is not in the compatible list
          if (item.node.releaseId !== this.props.newReleaseInnerId && this.findNodeTemplateInCompatible(node.template, compatibleTemplates) === -1) {
            nodesToBeUpdated.push(node);
          }
        }
      }
    });

    this.setState({
      nodesToBeUpdated: nodesToBeUpdated
    });
  }

  /**
   * Find node template code in a list of node templates for a new release
   * @param nodeTemplate {string} Code of a template to find
   * @param newNodeTemplates {object[]} Node templates from a new release
   * @return {number} Index of an object found, or -1 if not found
   */
  findNodeTemplateInRelease(nodeTemplate, newNodeTemplates) {
    // console.log('findNodeTemplateInRelease', nodeTemplate, newNodeTemplates.findIndex(x => x.node.code === nodeTemplate));
    return newNodeTemplates.findIndex(x => x.node.code === nodeTemplate);
  }

  /**
   * Find node template code in a list of compatible node templates for a new release
   * @param nodeTemplate {string} Code of a template to find
   * @param compatibleList {object[]} Compatible node templates for a new release
   * @return {number} Index of an object found, or -1 if not found
   */
  findNodeTemplateInCompatible(nodeTemplate, compatibleList) {
    // console.log('findNodeTemplateInCompatible', nodeTemplate, compatibleList.findIndex(x => x.node.templateCode === nodeTemplate));
    return compatibleList.findIndex(x => x.node.templateCode === nodeTemplate);
  }

  componentWillMount() {
    this.onReceiveProps(this.props, true);
  }

  componentWillReceiveProps(nextProps) {
    this.onReceiveProps(nextProps, false);
  }

  onReceiveProps(nextProps, skipCheck) {
    if ((nextProps.activeNodes && (!isEqual(nextProps.activeNodes, this.props.activeNodes) || skipCheck)) ||
        (nextProps.compatibleTemplates && (!isEqual(nextProps.compatibleTemplates, this.props.compatibleTemplates) || skipCheck)) ||
        (nextProps.newNodeTemplates && (!isEqual(nextProps.newNodeTemplates, this.props.newNodeTemplates) || skipCheck))
    ) {
      this.compareNodes(nextProps.activeNodes, nextProps.compatibleTemplates, nextProps.newNodeTemplates);
    }
  }

  render() {

    return (
      <div>
        {this.state.nodesToBeUpdated && this.state.nodesToBeUpdated.length > 0 &&
          <table className="table table-condensed">
            <thead>
            <tr>
              <th>Node to be upgraded</th>
              <th>Current release</th>
              <th>New release</th>
            </tr>
            </thead>
            <tbody>
            {this.state.nodesToBeUpdated.map((item, index) =>
              <tr key={index}>
                <td>{item.template}</td>
                <td>{item.releaseId}</td>
                <td>{this.props.newReleaseInnerId}</td>
              </tr>
            )
            }
            </tbody>
          </table>
        }
        {this.state.nodesToBeUpdated && this.state.nodesToBeUpdated.length === 0 &&
        <table className="table table-condensed">
          <thead>
          <tr>
            <th>Node to be upgraded</th>
            <th>Current release</th>
            <th>New release</th>
          </tr>
          </thead>
          <tbody>
            <tr>
              <td colSpan={3}>
                There are no nodes require being updated.
              </td>
            </tr>
          </tbody>
          </table>
        }
      </div>
    );
  }
}
