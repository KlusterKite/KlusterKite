import { createSelector } from 'reselect';

/**
 * Direct selector to the packagesListPage state domain
 */
const selectPackagesListPageDomain = () => state => state.get('packagesListPage');

/**
 * Other specific selectors
 */


/**
 * Default selector used by PackagesListPage
 */

const selectPackagesListPage = () => createSelector(
  selectPackagesListPageDomain(),
  (substate) => substate.toJS()
);

export default selectPackagesListPage;
export {
  selectPackagesListPageDomain,
};
