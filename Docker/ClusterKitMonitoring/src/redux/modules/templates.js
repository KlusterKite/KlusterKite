const LOAD = 'clusterkit-monitoring/templates/LOAD';
const LOAD_SUCCESS = 'clusterkit-monitoring/templates/LOAD_SUCCESS';
const LOAD_FAIL = 'clusterkit-monitoring/templates/LOAD_FAIL';

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
    default:
      return state;
  }
}

export function isLoaded(globalState) {
  return globalState.templates && globalState.templates.loaded;
}

export function load() {
  const path = '/templates/get';
  console.log(`loading data from ${path}`);

  return {
    types: [LOAD, LOAD_SUCCESS, LOAD_FAIL],
    promise: (client) => client.get(path) // params not used, just shown as demonstration
  };
}

export function loadById(id) {
  const path = '/templates/getById/' + id;
  console.log(`loading data from ${path}`);

  return {
    types: [LOAD, LOAD_SUCCESS, LOAD_FAIL],
    promise: (client) => client.get(path) // params not used, just shown as demonstration
  };
}
