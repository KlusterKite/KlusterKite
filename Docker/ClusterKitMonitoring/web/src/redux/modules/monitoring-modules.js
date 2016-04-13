const LOAD = 'clusterkit-monitoring/monitoringModules/LOAD';
const LOAD_SUCCESS = 'clusterkit-monitoring/monitoringModules/LOAD_SUCCESS';
const LOAD_FAIL = 'clusterkit-monitoring/monitoringModules/LOAD_FAIL';
const UPGRADE = 'clusterkit-monitoring/monitoringModules/UPGRADE';
const UPGRADE_SUCCESS = 'clusterkit-monitoring/monitoringModules/UPGRADE_SUCCESS';
const UPGRADE_FAIL = 'clusterkit-monitoring/monitoringModules/UPGRADE_FAIL';

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
        upgraded: false,
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
    case UPGRADE:
      return {
        ...state,
        upgrading: true,
        upgradingId: action.id
      };
    case UPGRADE_SUCCESS:
      return {
        ...state,
        upgrading: false,
        upgradingId: null,
        upgraded: true,
        upgradeError: null
      };
    case UPGRADE_FAIL:
      return {
        ...state,
        upgrading: false,
        upgradingId: null,
        upgraded: false,
        upgradeError: action.error
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

export function upgradeNode(id, address) {
  const path = '/nodemanager/upgradeNode';
  console.log('upgrading node ');
  console.log(address);

  return {
    types: [UPGRADE, UPGRADE_SUCCESS, UPGRADE_FAIL],
    promise: (client) => client.post(path, {
      data: address
    }),
    id: id
  };
}
