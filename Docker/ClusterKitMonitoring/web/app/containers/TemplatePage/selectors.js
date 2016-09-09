import { createSelector } from 'reselect';

/**
 * Direct selector to the templatePage state domain
 */
const selectTemplatePageDomain = () => state => state.get('templatePage');

/**
 * Other specific selectors
 */


/**
 * Default selector used by TemplatePage
 */

const selectTemplatePage = () => createSelector(
  selectTemplatePageDomain(),
  (substate) => substate.toJS()
);

export default selectTemplatePage;
export {
  selectTemplatePageDomain,
};
