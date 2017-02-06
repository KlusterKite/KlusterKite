import { createSelector } from 'reselect';

/**
 * Direct selector to the authPage state domain
 */
const selectAuthPageDomain = () => state => state.get('authPage');

/**
 * Other specific selectors
 */


/**
 * Default selector used by AuthPage
 */

const selectAuthPage = () => createSelector(
  selectAuthPageDomain(),
  (substate) => substate.toJS()
);

export default selectAuthPage;
export {
  selectAuthPageDomain,
};
