/*
 *
 * TemplatePage reducer
 *
 */

import { fromJS } from 'immutable';
import {
  TEMPLATE_RECEIVE,
  TEMPLATE_UPDATED,
  TEMPLATE_SET_LOADED
} from './constants';

const initialState = fromJS({
  template: {
    Id: '0',
    Code: '',
    Configuration: '{\n}',
    ContainerTypes: [],
    MaximumNeededInstances: null,
    MininmumRequiredInstances: 1,
    Name: '',
    Packages: [],
    Priority: 1.0,
    Version: 0
  },

  updateError: null,
  isLoaded: false
});

function templatePageReducer(state = initialState, action) {
  switch (action.type) {
    case TEMPLATE_RECEIVE:
      return state.set('template', action.template).set('isLoaded', true);
    case TEMPLATE_UPDATED:
      return action.template
        ? state.set('template', action.template).set('updateError', null)
        : state.set('updateError', action.error);
    case TEMPLATE_SET_LOADED:
      return state.set('isLoaded', true);
    default:
      return state;
  }
}

export default templatePageReducer;
