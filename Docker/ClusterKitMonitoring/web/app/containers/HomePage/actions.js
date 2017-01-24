/*
 *
 * HomePage actions
 *
 */

import {
  NODE_DESCRIPTIONS_LOAD,
  NODE_DESCRIPTIONS_LOAD_ERROR,
  NODE_DESCRIPTIONS_RECEIVE,
  NODE_UPGRADE,
  NODE_RELOAD_PACKAGES,
} from './constants';

export function nodeDescriptionsLoadAction() {
  return {
    type: NODE_DESCRIPTIONS_LOAD,
  };
}

export function nodeDescriptionsLoadErrorAction() {
  return {
    type: NODE_DESCRIPTIONS_LOAD_ERROR,
  };
}

export function nodeDescriptionsReceiveAction(nodeDescriptions, swaggerLinks, templates) {
  return {
    type: NODE_DESCRIPTIONS_RECEIVE,
    nodeDescriptions,
    swaggerLinks,
    templates,
  };
}

export function nodeUpgradeAction(node) {
  return {
    type: NODE_UPGRADE,
    node,
  };
}

export function nodeReloadPackagesAction() {
  return {
    type: NODE_RELOAD_PACKAGES,
  };
}
