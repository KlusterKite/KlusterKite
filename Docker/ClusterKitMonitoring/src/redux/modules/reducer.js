import { combineReducers } from 'redux';
import { routerStateReducer } from 'redux-router';

import auth from './auth';
import monitoringModules from './monitoring-modules';
import monitoringSwagger from './monitoring-swagger';
import {reducer as form} from 'redux-form';   // we are using redux-form to store form data in Redux
import templates from './templates';

export default combineReducers({
  router: routerStateReducer,
  auth,
  form,
  monitoringModules,
  monitoringSwagger,
  templates
});
