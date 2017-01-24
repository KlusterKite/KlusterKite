/*
 *
 * HomePage reducer
 *
 */

import { fromJS } from 'immutable';
import {
  NODE_DESCRIPTIONS_RECEIVE,
  NODE_DESCRIPTIONS_LOAD_ERROR,
} from './constants';

const initialState = fromJS({
  nodeDescriptions: [],
  swaggerLinks: [],
  templates: [],
  hasError: false,
});

function homePageReducer(state = initialState, action) {
  switch (action.type) {
    case NODE_DESCRIPTIONS_RECEIVE:
      return state
        .set('nodeDescriptions', action.nodeDescriptions)
        .set('swaggerLinks', action.swaggerLinks)
        .set('templates', action.templates)
        .set('hasError', false);
    case NODE_DESCRIPTIONS_LOAD_ERROR:
      return state.set('hasError', true);
    default:
      return state;
  }
}

export default homePageReducer;
