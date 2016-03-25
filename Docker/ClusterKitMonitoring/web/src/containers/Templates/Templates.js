import React, { Component } from 'react';
import TemplatesList from './TemplatesList';

export default class Templates extends Component {
  render() {
    return (
        <div className="container">
          <h1>Templates</h1>
          <TemplatesList />
        </div>
    );
  }
}
