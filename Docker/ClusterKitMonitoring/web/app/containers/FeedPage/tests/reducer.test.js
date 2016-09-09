import expect from 'expect';
import feedPageReducer from '../reducer';
import { fromJS } from 'immutable';

describe('feedPageReducer', () => {
  it('returns the initial state', () => {
    expect(feedPageReducer(undefined, {})).toEqual(fromJS({}));
  });
});
