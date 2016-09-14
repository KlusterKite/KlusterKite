import { createSelector } from 'reselect';

/**
 * Direct selector to the actorsTreePage state domain
 */
const selectActorsTreePageDomain = () => state => state.get('actorsTreePage');

/**
 * Other specific selectors
 */


/**
 * Default selector used by ActorsTreePage
 */

const selectActorsTreePage = () => createSelector(
  selectActorsTreePageDomain(),
  (substate) => substate.toJS()
);

export default selectActorsTreePage;
export {
  selectActorsTreePageDomain,
};
