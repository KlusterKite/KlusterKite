/*
 *
 * TemplatePage actions
 *
 */

import {
  TEMPLATE_CREATE,
  TEMPLATE_LOAD,
  TEMPLATE_NEW,
  TEMPLATE_RECEIVE,
  TEMPLATE_UPDATE,
  TEMPLATE_UPDATED,
} from './constants';

export function templateCreateAction(template) {
  return {
    type: TEMPLATE_CREATE,
    template,
  };
}

export function templateUpdateAction(template) {
  return {
    type: TEMPLATE_UPDATE,
    template,
  };
}

export function templateLoadAction(id) {
  return {
    type: TEMPLATE_LOAD,
    id,
  };
}

export function templateSetLoadedAction() {
  return {
    type: TEMPLATE_NEW,
  };
}

export function templateReceiveAction(template) {
  return {
    type: TEMPLATE_RECEIVE,
    template,
  };
}

export function templateUpdatedAction(template, error) {
  return {
    type: TEMPLATE_UPDATED,
    template,
    error,
  };
}
