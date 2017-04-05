import React from 'react';
import Autosuggest from 'react-autosuggest';

import isEqual from 'lodash/isEqual';

import './autosuggest.css';

export default class PackagesSelector extends React.Component {
  constructor() {
    super();

    // Autosuggest is a controlled component.
    // This means that you need to provide an input value
    // and an onChange handler that updates this value (see below).
    // Suggestions also need to be provided to the Autosuggest,
    // and they are initially empty because the Autosuggest is closed.
    this.state = {
      packageValue: '',
      packageSuggestions: [],
      packagesNames: [],
      versionValue: '',
      versionSuggestions: [],
      packageVersions: []
    };
  }

  componentWillMount() {
    console.log('componentWillMount', this.props);
    if (this.props.packages) {
      const nodes = this.props.packages.edges.map(x => x.node);
      const packagesNames = nodes.map(node => node.name);

      this.setState({
        packagesNames: packagesNames
      });
    }
  }

  componentWillReceiveProps(nextProps) {
    if (nextProps.packages && !isEqual(nextProps.packages, this.props.packages)) {
      const nodes = nextProps.packages.edges.map(x => x.node);
      const packagesNames = nodes.map(node => node.name);

      this.setState({
        packagesNames: packagesNames
      });
    }
  }

  onPackageChange = (event, { newValue }) => {
    const nodes = this.props.packages.edges.map(x => x.node);
    const node = nodes.find(x => x.name === newValue);

    if (node) {
      this.setState({
        packageValue: newValue,
        versionValue: '',
        packageVersions: node.availableVersions
      });
    } else {
      this.setState({
        packageValue: newValue,
        versionValue: '',
        packageVersions: []
      });
    }
  };

  onVersionChange = (event, { newValue }) => {
    this.setState({
      versionValue: newValue
    });
  };

  // Autosuggest will call this function every time you need to update suggestions.
  // You already implemented this logic above, so just use it.
  onPackageSuggestionsFetchRequested = ({ value }) => {
    this.setState({
      packageSuggestions: this.getSuggestions(value, this.state.packagesNames)
    });
  };

  // Autosuggest will call this function every time you need to clear suggestions.
  onPackageSuggestionsClearRequested = () => {
    this.setState({
      packageSuggestions: []
    });
  };

  // Autosuggest will call this function every time you need to update suggestions.
  // You already implemented this logic above, so just use it.
  onVersionSuggestionsFetchRequested = ({ value }) => {
    this.setState({
      versionSuggestions: this.getSuggestions(value, this.state.packageVersions)
    });
  };

  // Autosuggest will call this function every time you need to clear suggestions.
  onVersionSuggestionsClearRequested = () => {
    this.setState({
      versionSuggestions: []
    });
  };

  // Teach Autosuggest how to calculate suggestions for any given input value.
  getSuggestions = (value, source) => {
    const inputValue = value.trim().toLowerCase();
    const inputLength = inputValue.length;

    return inputLength === 0 ? [] : source.filter(lang =>
      lang.toLowerCase().indexOf(inputValue) !== -1
//      lang.toLowerCase().slice(0, inputLength) === inputValue
    ).slice(0, 10);
  };

  // When suggestion is clicked, Autosuggest needs to populate the input element
  // based on the clicked suggestion. Teach Autosuggest how to calculate the
  // input value for every given suggestion.
  getSuggestionValue = (suggestion) => suggestion;

  // Use your imagination to render suggestions.
  renderSuggestion = (suggestion) => (
    <div>
      {suggestion}
    </div>
  );


  static propTypes = {
    packages: React.PropTypes.object.isRequired,
  };

  render() {
    const { packageValue, packageSuggestions, versionValue, versionSuggestions } = this.state;

    const inputPropsPackage = {
      placeholder: 'Package',
      value: packageValue,
      onChange: this.onPackageChange
    };

    const inputPropsVersion = {
      placeholder: 'Version',
      value: versionValue,
      onChange: this.onVersionChange
    };

    return (
      <div>
        <div className="row">
          <div className="col-xs-6 col-sm-6 col-md-6 col-lg-6">
            <Autosuggest
              suggestions={packageSuggestions}
              onSuggestionsFetchRequested={this.onPackageSuggestionsFetchRequested}
              onSuggestionsClearRequested={this.onPackageSuggestionsClearRequested}
              getSuggestionValue={this.getSuggestionValue}
              renderSuggestion={this.renderSuggestion}
              inputProps={inputPropsPackage}
            />
          </div>
          <div className="col-xs-4 col-sm-4 col-md-4 col-lg-4">
            <Autosuggest
              suggestions={versionSuggestions}
              onSuggestionsFetchRequested={this.onVersionSuggestionsFetchRequested}
              onSuggestionsClearRequested={this.onVersionSuggestionsClearRequested}
              getSuggestionValue={this.getSuggestionValue}
              renderSuggestion={this.renderSuggestion}
              inputProps={inputPropsVersion}
            />
          </div>
        </div>
      </div>
    );
  }
}

