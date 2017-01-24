import { createSelector } from 'reselect';

/**
 * Direct selector to the feedPage state domain
 */
const selectFeedPageDomain = () => state => state.get('feedPage');

/**
 * Other specific selectors
 */


/**
 * Default selector used by FeedPage
 */

const selectFeedPage = () => createSelector(
  selectFeedPageDomain(),
  (substate) => substate.toJS()
);

export default selectFeedPage;
export {
  selectFeedPageDomain,
};
