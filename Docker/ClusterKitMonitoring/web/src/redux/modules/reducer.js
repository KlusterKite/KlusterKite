import { combineReducers } from 'redux';
// import multireducer from 'multireducer';
import { routeReducer } from 'react-router-redux';
import {reducer as reduxAsyncConnect} from 'redux-async-connect';

import auth from './auth';
import monitoringModules from './monitoring-modules';
import monitoringSwagger from './monitoring-swagger';
import {reducer as form} from 'redux-form';
import templates from './templates';

export default combineReducers({
  routing: routeReducer,
  reduxAsyncConnect,
  auth,
  form,
  monitoringModules,
  monitoringSwagger,
  templates
});
