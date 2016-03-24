const LOAD = 'clusterkit-monitoring/monitoringSwagger/LOAD';
const LOAD_SUCCESS = 'clusterkit-monitoring/monitoringSwagger/LOAD_SUCCESS';
const LOAD_FAIL = 'clusterkit-monitoring/monitoringSwagger/LOAD_FAIL';

const initialState = {
  loaded: false
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
  return globalState.monitoringSwagger && globalState.monitoringSwagger.loaded;
}

export function load() {
  const path = '/swagger/getList';
  // console.log(`loading data from ${path}`);

  return {
    types: [LOAD, LOAD_SUCCESS, LOAD_FAIL],
    promise: (client) => client.get(path) // params not used, just shown as demonstration
  };
}
