import expect from 'expect';
import homePageReducer from '../reducer';
import { fromJS } from 'immutable';

describe('homePageReducer', () => {
  it('returns the initial state', () => {
    expect(homePageReducer(undefined, {})).toEqual(fromJS({}));
  });
});
