import expect from 'expect';
import templatesListPageReducer from '../reducer';
import { fromJS } from 'immutable';

describe('templatesListPageReducer', () => {
  it('returns the initial state', () => {
    expect(templatesListPageReducer(undefined, {})).toEqual(fromJS({}));
  });
});
