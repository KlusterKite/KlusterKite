const LOAD = 'clusterkit-monitoring/monitoringModules/LOAD';
const LOAD_SUCCESS = 'clusterkit-monitoring/monitoringModules/LOAD_SUCCESS';
const LOAD_FAIL = 'clusterkit-monitoring/monitoringModules/LOAD_FAIL';

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
  return globalState.monitoringModules && globalState.monitoringModules.loaded;
}

export function loadModules() {
  const path = '/nodemanager/getDescriptions';
  // console.log(`loading data from ${path}`);

  return {
    types: [LOAD, LOAD_SUCCESS, LOAD_FAIL],
    promise: (client) => client.get(path)
  };
}
