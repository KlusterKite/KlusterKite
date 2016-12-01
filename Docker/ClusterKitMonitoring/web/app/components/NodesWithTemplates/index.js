/**
 * Created by kanto on 01.12.2016.
 */
import React, { PropTypes, Component } from 'react';
import { autobind } from 'core-decorators';

import styles from './styles.css';

export default class NodesWithTemplates extends Component {
  static propTypes = {
    nodes: PropTypes.array.isRequired,
    templates: PropTypes.array.isRequired,
  }

  @autobind
  drawTemplate(template) {
    const { nodes } = this.props;

    const nodesCount = nodes.filter(n => n.NodeTemplate === template.Code).length;
    let color;
    if (nodesCount < template.MinimumRequiredInstances) {
      color = 'label-danger';
    } else if (nodesCount === template.MinimumRequiredInstances) {
      color = 'label-warning';
    } else {
      color = 'label-success';
    }

    return (
      <span title={template.Name} className={'label ' + color }>{template.Code}: {nodesCount} / {template.MinimumRequiredInstances}</span>
    );
  }

  render() {
    const { templates, nodes } = this.props;

    return (
      <div className={styles.templates}>
        <div>
          <span className="label label-default">Total nodes: {nodes.length}</span>
        </div>
        {templates.map((template) =>
          <div key={template.Code}>
            {this.drawTemplate(template)}
          </div>
        )}
      </div>
    );
  }
}

