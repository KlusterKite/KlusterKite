/**
*
* ActorsTree
*
*/

import React, { Component, PropTypes } from 'react';
import ReactDom from 'react-dom';
import { autobind } from 'core-decorators';

import {
  select as d3Select,
  tree as d3Tree,
  hierarchy as d3Hhierarchy,
  event as d3Event,
} from 'd3';

import styles from './styles.css';

class ActorsTree extends Component {

  static propTypes = {
    tree: PropTypes.object.isRequired,
  }

  constructor() {
    super();
    this.tooltipDiv = null;
  }

  componentDidMount() {
    this.tooltipDiv = d3Select('body')
      .append('div')
      .attr('class', styles.tooltip)
      .style('opacity', 0);
  }

  componentDidUpdate() {
    this.renderTreeGraf(this.props.tree, ReactDom.findDOMNode(this));
  }

  componentWillUnmount() {
    if (this.tooltipDiv) {
      this.tooltipDiv.remove();
      this.tooltipDiv = null;
    }
  }


  getActorToolTip(node) {
    const result = (
      <div className={`${styles.wellTooltip} well well-sm`}>
        <p><strong>{node.Name}</strong></p>
        {node.ActorType && (<small>Type: {node.ActorType}<br /></small>)}
        {node.DispatcherType && (<small>DispatcherType: {node.DispatcherType}<br /></small>)}
        {node.CurrentMessage && <small>CurrentMessage: {node.CurrentMessage}<br /></small>}
        {node.QueueSize !== undefined && <small>QueueSize: {node.QueueSize}<br /></small>}
        {node.QueueSizeSum !== undefined && <small>QueueSizeSum: {node.QueueSizeSum}<br /></small>}
        {node.MaxQueueSize !== undefined && <small>MaxQueueSize: {node.MaxQueueSize}<br /></small>}
      </div>);

    return result;
  }

  @autobind
  calculateLevels(tree) {
    if (!tree.Children || tree.Children.length === 0) {
      return [];
    }

    const arrays = tree.Children.map(c => this.calculateLevels(c));
    const combinedArray = [];
    arrays.forEach(a => a.forEach((e, index) => {
      if (combinedArray[index] === undefined) {
        combinedArray[index] = e;
      } else {
        combinedArray[index] += e;
      }
    }));

    combinedArray.unshift(tree.Children.length);
    return combinedArray;
  }

  @autobind
  nameToTree(fullName) {
    const name = decodeURIComponent(fullName);
    return name.length > 23 ? `${name.substr(0, 20)}...` : name;
  }

  @autobind
  renderTreeGraf(tree, treeSvg) {
    if (!tree || !tree.Nodes) {
      return;
    }

    const updatedTree = {
      Name: 'Cluster',
      QueueSizeSum: tree.QueueSizeSum,
      MaxQueueSize: tree.MaxQueueSize,
      Children: Object.keys(tree.Nodes).map(key => ({
        ...tree.Nodes[key],
        Name: key,
      })),
    };

    const treeParams = this.calculateLevels(updatedTree);
    const maxDepth = treeParams.length;
    const maxChildren = treeParams.reduce((m, e) => (m > e ? m : e), 0);

    const svg = d3Select(treeSvg);
    svg
      .attr('width', maxDepth * 200)
      .attr('height', maxChildren * 25);

    const width = +svg.attr('width');
    const height = +svg.attr('height');

    svg.selectAll('*').remove();
    const g = svg.append('g').attr('transform', 'translate(40,0)');

    const treeGraph = d3Tree().size([height, width - 160]);

    const root = d3Hhierarchy(updatedTree, n => n.Children);
    treeGraph(root);

    g.selectAll('.link')
      .data(root.descendants().slice(1))
      .enter()
      .append('path')
      .attr('class', (d) => {
       // styles.link
        const classes = [];
        classes.push(styles.link);
        if (d.data.QueueSize > 1) {
          classes.push(styles['link--error']);
        } else if (d.data.MaxQueueSize > 1) {
          classes.push(styles['link--warning']);
        } else {
          classes.push(styles['link--ok']);
        }

        return classes.reduce((i, c) => `${i} ${c}`);
      })
      .attr('d', (d) => `M${d.y},${d.x}C${(d.y + d.parent.y) / 2},${d.x} ${(d.y + d.parent.y) / 2},${d.parent.x} ${d.parent.y},${d.parent.x}`);

    const div = this.tooltipDiv;
    const node = g.selectAll('.node')
      .data(root.descendants())
      .enter()
      .append('g')
      .attr('class', (d) => {
        // circle css class definition based on node's properties
        const classes = [];
        classes.push(styles.node);
        classes.push(d.children ? styles['node--internal'] : styles['node--leaf']);
        if (d.data.QueueSize > 1) {
          classes.push(styles['node--error']);
        } else if (d.data.MaxQueueSize > 1) {
          classes.push(styles['node--warning']);
        } else {
          classes.push(styles['node--ok']);
        }

        return classes.reduce((i, c) => `${i} ${c}`);
      })
      .attr('transform', (d) => `translate(${d.y},${d.x})`)
      .on('mouseover', (d) => {
        div.transition()
          .duration(200)
          .style('opacity', 0.9);

        ReactDom.render(this.getActorToolTip(d.data), div.node());

        const offsetX = 13;
        const offsetY = 10;
        const positionX = d3Event.layerX;
        const positionY = d3Event.layerY;
        const tooltipWidth = div.node().clientWidth;
        const tooltipHeight = div.node().clientHeight;
        const containerWidth = d3Event.path[4].clientWidth;
        const containerHeight = d3Event.path[4].clientHeight;
        const containerOffsetX = d3Event.path[4].offsetLeft;
        const containerOffsetY = d3Event.path[4].offsetTop;

        let tooltipX = offsetX;
        if ((positionX + offsetX + tooltipWidth) < containerWidth) {
          tooltipX = positionX + offsetX;
        } else if ((positionX - offsetX - tooltipWidth) > 0) {
          tooltipX = positionX - offsetX - tooltipWidth;
        }

        tooltipX += containerOffsetX;

        let tooltipY = offsetY;
        if ((positionY - offsetY + tooltipHeight) < containerHeight) {
          tooltipY = positionY - offsetY;
        } else if ((positionY + offsetY - tooltipHeight) > 0 && positionY + offsetY < containerHeight) {
          tooltipY = positionY + offsetY - tooltipHeight;
        } else if ((positionY - tooltipHeight) > 1) {
          tooltipY = positionY - tooltipHeight - 1;
        }

        tooltipY += containerOffsetY;

        div
          .style('left', `${tooltipX}px`)
          .style('top', `${tooltipY}px`);
      })
      .on('mouseout', () => {
        div.transition()
          .duration(500)
          .style('opacity', 0);
      });

    node
      .append('circle')
      .attr('r', 5);

    node
      .append('text')
      .attr('dy', 3)
      .attr('x', () => 8)
      .style('text-anchor', 'start')
      .text((d) => this.nameToTree(d.data.Name));
  }


  render() {
    return (
      <svg width="10" height="10"></svg>
    );
  }
}


export default ActorsTree;
