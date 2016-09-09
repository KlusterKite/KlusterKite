/*
 *
 * HomePage reducer
 *
 */

import { fromJS } from 'immutable';
import {
  NODE_DESCRIPTIONS_RECEIVE,
} from './constants';

const initialState = fromJS({
  nodeDescriptions: []
});

function homePageReducer(state = initialState, action) {
  switch (action.type) {
    case NODE_DESCRIPTIONS_RECEIVE:
      return state.set('nodeDescriptions', action.nodeDescriptions);
    default:
      return state;
  }
}

export default homePageReducer;
