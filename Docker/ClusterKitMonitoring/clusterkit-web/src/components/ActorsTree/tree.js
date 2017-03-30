import React  from 'react';
import ReactDom from 'react-dom';
import isEqual from 'lodash/isEqual';

import {
  select as d3Select,
  tree as d3Tree,
  hierarchy as d3Hhierarchy,
  event as d3Event,
} from 'd3';

import './styles.css';

class ActorsTree extends React.Component {
  constructor(props) {
    super(props);

    this.calculateLevels = this.calculateLevels.bind(this);
    this.nameToTree = this.nameToTree.bind(this);
    this.renderTreeGraf = this.renderTreeGraf.bind(this);

    this.tooltipDiv = null;
  }

  static propTypes = {
    tree: React.PropTypes.arrayOf(React.PropTypes.object),
  };

  componentDidMount() {
    this.tooltipDiv = d3Select('body')
      .append('div')
      .attr('class', 'tooltip')
      .style('opacity', 0);

    if (this.props.tree) {
      this.renderTreeGraf(this.props.tree, ReactDom.findDOMNode(this));
    }
  }

  componentWillReceiveProps(nextProps) {
    if (nextProps.tree && !isEqual(nextProps.tree, this.props.tree)) {
      this.renderTreeGraf(nextProps.tree, ReactDom.findDOMNode(this));
    }
  }

  componentWillUnmount() {
    if (this.tooltipDiv) {
      this.tooltipDiv.remove();
      this.tooltipDiv = null;
    }
  }


  getActorToolTip(node) {
    const result = (
      <div className="wellTooltip well well-sm">
        <p><strong>{decodeURIComponent(node.name)}</strong></p>
        {node.actorType && (<small>Type: {node.actorType}<br /></small>)}
        {node.dispatcherType && (<small>DispatcherType: {node.dispatcherType}<br /></small>)}
        {node.currentMessage && <small>CurrentMessage: {node.currentMessage}<br /></small>}
        {node.queueSize !== undefined && <small>QueueSize: {node.queueSize}<br /></small>}
        {node.queueSizeSum !== undefined && <small>QueueSizeSum: {node.queueSizeSum}<br /></small>}
        {node.maxQueueSize !== undefined && <small>MaxQueueSize: {node.maxQueueSize}<br /></small>}
      </div>);

    return result;
  }

  calculateLevels(tree) {
    let children;
    if (tree.children && tree.children.edges && tree.children.edges.length > 0) {
      children = tree.children.edges.map(x => x.node);
    } else if (tree.children && tree.children.length > 0) {
      children = tree.children;
    }
    else return [];

    const arrays = children.map(c => this.calculateLevels(c));

    const combinedArray = [];
    arrays.forEach(a => a.forEach((e, index) => {
      if (combinedArray[index] === undefined) {
        combinedArray[index] = e;
      } else {
        combinedArray[index] += e;
      }
    }));

    combinedArray.unshift(children.length);
    return combinedArray;
  }

  nameToTree(fullName) {
    const name = decodeURIComponent(fullName);
    return name.length > 23 ? `${name.substr(0, 20)}...` : name;
  }

  renderTreeGraf(tree, treeSvg) {
    if (!tree) {
      return;
    }

    const updatedTree = {
      name: 'Cluster',
      queueSizeSum: 0,
      maxQueueSize: 0,
      children: tree.map(c => c.node.value),
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

    const root = d3Hhierarchy(updatedTree, n => {
      if (n.children && n.children.edges) {
        const nodes = n.children.edges.map(x => x.node);
        return nodes;
      }
      return n.children;
    });
    treeGraph(root);

    g.selectAll('.link')
      .data(root.descendants().slice(1))
      .enter()
      .append('path')
      .attr('class', (d) => {
        const classes = [];
        classes.push("link");
        if (d.data.QueueSize > 1) {
          classes.push("link--error");
        } else if (d.data.MaxQueueSize > 1) {
          classes.push("link--warning");
        } else {
          classes.push("link--ok");
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
        classes.push("node");
        classes.push(d.children ? 'node--internal' : 'node--leaf');
        if (d.data.queueSize > 1) {
          classes.push('node--error');
        } else if (d.data.maxQueueSize > 1) {
          classes.push('node--warning');
        } else {
          classes.push('node--ok');
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
      .text((d) => this.nameToTree(d.data.name));
  }

  render() {
    return (
      <svg width="10" height="10"></svg>
    );
  }
}


export default ActorsTree;
