import { combineReducers } from 'redux';
import { routerStateReducer } from 'redux-router';

import auth from './auth';
import {reducer as form} from 'redux-form';   // we are using redux-form to store form data in Redux

export default combineReducers({
  router: routerStateReducer,
  auth,
  form
});
