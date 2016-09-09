/**
*
* Pager
*
*/

import React, {PropTypes, Component} from 'react';
import {autobind} from 'core-decorators'

import FlatButton from 'material-ui/FlatButton';
import DropDownMenu from 'material-ui/DropDownMenu';
import MenuItem from 'material-ui/MenuItem';
import Paper from 'material-ui/Paper';



import styles from './styles.css';

export default class Pager extends Component {

  shouldComponentUpdate(nextProps,  nextState) {
    return this.props.total != nextProps.total || this.props.pageSize != nextProps.pageSize || this.props.skip != nextProps.skip
  }


  @autobind
  selectPage(event) {
    if (this.props.onPageChanged) {
      const {total, pageSize, skip} = this.props;
      this.props.onPageChanged((event.currentTarget.value - 1) * pageSize)
    }
  }

  @autobind
  pageSizeChanged(event, index, value) {
    if (this.props.onPageSizedChanged) {
      this.props.onPageSizedChanged(value);
    }
  }



  render() {
    const {total, pageSize, skip} = this.props;

    var skipped = skip;
    if (!skipped) {
      skipped = 0;
    }

    if (!total || !pageSize || total <= pageSize) {
      return null;
    }

    let pagesCount = Math.ceil(total / pageSize);
    let currentPage = Math.floor(skipped / pageSize) + 1;

    var startPage;
    if (currentPage <= 5) {
      startPage = 1
    } else if (currentPage >= pagesCount - 5) {
      if (pagesCount > 10) {
        startPage = pagesCount - 10;
      } else {
        startPage = 1;
      }
    } else {
      startPage = currentPage - 5;
    }

    var endPage;
    if (currentPage >= pagesCount - 5) {
      endPage = pagesCount;
    }
    else if (currentPage <= 5) {
      if (pagesCount > 10) {
        endPage = 10;
      } else {
        endPage = pagesCount;
      }
    } else {
      endPage = currentPage + 5;
    }


    var pages = [];
    for (var page = startPage; page <= endPage; page++) {
      pages.push({num: page, selected: page == currentPage});
    }


    return (
      <Paper className={styles.pager}>
        <DropDownMenu value={pageSize} className={styles.menu} onChange={this.pageSizeChanged}>
          <MenuItem value={10} primaryText="Show by 10"/>
          <MenuItem value={50} primaryText="Show by 50"/>
          <MenuItem value={100} primaryText="Show by 100"/>
        </DropDownMenu>

        <FlatButton label="First" value={1} disabled={currentPage == 1} onTouchTap={this.selectPage}/>
        <FlatButton value={currentPage - 1} disabled={currentPage <= 1} onTouchTap={this.selectPage}>
          <i className="fa fa-caret-left"/>
        </FlatButton>
        {pages.map(page =>
          <FlatButton key={page.num} label={page.num} disabled={page.selected} onTouchTap={this.selectPage}
                        value={page.num}/>
        )}
        <FlatButton value={currentPage + 1} disabled={currentPage == pagesCount} onTouchTap={this.selectPage}>
          <i className="fa fa-caret-right"/>
        </FlatButton>
        <FlatButton label="Last" value={pagesCount} disabled={currentPage == pagesCount}
                      onTouchTap={this.selectPage}/>
      </Paper>
    );
  }
}

