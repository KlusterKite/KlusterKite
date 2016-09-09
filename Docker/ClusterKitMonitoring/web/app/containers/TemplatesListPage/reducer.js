/*
 *
 * TemplatesListPage reducer
 *
 */

import { fromJS } from 'immutable';
import {
  TEMPLATES_RECEIVE
} from './constants';

const initialState = fromJS({
  templates: []
});

function templatesListPageReducer(state = initialState, action) {
  switch (action.type) {
    case TEMPLATES_RECEIVE:
      return state.set('templates', action.templates);
    default:
      return state;
  }
}

export default templatesListPageReducer;
