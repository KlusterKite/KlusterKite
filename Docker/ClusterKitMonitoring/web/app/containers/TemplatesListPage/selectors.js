import { createSelector } from 'reselect';

/**
 * Direct selector to the templatesListPage state domain
 */
const selectTemplatesListPageDomain = () => state => state.get('templatesListPage');

/**
 * Other specific selectors
 */


/**
 * Default selector used by TemplatesListPage
 */

const selectTemplatesListPage = () => createSelector(
  selectTemplatesListPageDomain(),
  (substate) => substate.toJS()
);

export default selectTemplatesListPage;
export {
  selectTemplatesListPageDomain,
};
