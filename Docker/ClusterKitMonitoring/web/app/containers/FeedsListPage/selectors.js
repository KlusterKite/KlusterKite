import { createSelector } from 'reselect';

/**
 * Direct selector to the feedsListPage state domain
 */
const selectFeedsListPageDomain = () => state => state.get('feedsListPage');

/**
 * Other specific selectors
 */


/**
 * Default selector used by FeedsListPage
 */

const selectFeedsListPage = () => createSelector(
  selectFeedsListPageDomain(),
  (substate) => substate.toJS()
);

export default selectFeedsListPage;
export {
  selectFeedsListPageDomain,
};
