import expect from 'expect';
import templatePageReducer from '../reducer';
import { fromJS } from 'immutable';

describe('templatePageReducer', () => {
  it('returns the initial state', () => {
    expect(templatePageReducer(undefined, {})).toEqual(fromJS({}));
  });
});
