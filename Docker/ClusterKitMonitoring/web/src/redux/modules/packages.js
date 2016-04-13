const LOAD = 'clusterkit-monitoring/packages/LOAD';
const LOAD_SUCCESS = 'clusterkit-monitoring/packages/LOAD_SUCCESS';
const LOAD_FAIL = 'clusterkit-monitoring/packages/LOAD_FAIL';
const RELOAD = 'clusterkit-monitoring/packages/RELOAD';
const RELOAD_SUCCESS = 'clusterkit-monitoring/packages/RELOAD_SUCCESS';
const RELOAD_FAIL = 'clusterkit-monitoring/packages/RELOAD_FAIL';

const initialState = {
  loaded: false,
  loading: false
};

export default function reducer(state = initialState, action = {}) {
  switch (action.type) {
    case LOAD:
      return {
        ...state,
        loading: true
      };
    case LOAD_SUCCESS:
      return {
        ...state,
        loading: false,
        loaded: true,
        data: action.result,
        error: null
      };
    case LOAD_FAIL:
      return {
        ...state,
        loading: false,
        loaded: false,
        data: null,
        error: action.error
      };
    case RELOAD:
      return {
        ...state,
        reloading: true
      };
    case RELOAD_SUCCESS:
      return {
        ...state,
        reloading: false,
        reloaded: true,
        error: null
      };
    case RELOAD_FAIL:
      return {
        ...state,
        reloading: false,
        reloaded: false,
        error: action.error
      };
    default:
      return state;
  }
}

export function isLoaded(globalState) {
  return globalState.templates && globalState.templates.loaded;
}

export function load() {
  const path = '/nodemanager/getPackages';
  console.log(`loading data from ${path}`);

  return {
    types: [LOAD, LOAD_SUCCESS, LOAD_FAIL],
    promise: (client) => client.get(path)
  };
}

export function reload() {
  const path = '/nodemanager/reloadPackages';
  console.log(`reloading data from ${path}`);

  return {
    types: [RELOAD, RELOAD_SUCCESS, RELOAD_FAIL],
    promise: (client) => client.post(path)
  };
}
