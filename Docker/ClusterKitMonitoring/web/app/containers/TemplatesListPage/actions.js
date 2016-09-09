/*
 *
 * TemplatesListPage actions
 *
 */

import {
  TEMPLATES_LOAD,
  TEMPLATES_RECEIVE,
} from './constants';

export function templatesLoadAction() {
  return {
    type: TEMPLATES_LOAD,
  };
}

export function templatesReceiveAction(templates) {
  return {
    type: TEMPLATES_RECEIVE,
    templates
  };
}
