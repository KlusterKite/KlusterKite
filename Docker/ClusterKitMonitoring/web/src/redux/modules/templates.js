const LOAD = 'clusterkit-monitoring/templates/LOAD';
const LOAD_SUCCESS = 'clusterkit-monitoring/templates/LOAD_SUCCESS';
const LOAD_FAIL = 'clusterkit-monitoring/templates/LOAD_FAIL';
const SAVE = 'clusterkit-monitoring/templates/SAVE';
const SAVE_SUCCESS = 'clusterkit-monitoring/templates/SAVE_SUCCESS';
const SAVE_FAIL = 'clusterkit-monitoring/templates/SAVE_FAIL';

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
    case SAVE:
      return {
        ...state,
        data: action.data,
        saving: true
      };
    case SAVE_SUCCESS:
      return {
        ...state,
        saving: false,
        saved: true,
        saveError: null
      };
    case SAVE_FAIL:
      console.log(action);
      return {
        ...state,
        saving: false,
        saved: false,
        saveError: 'Saving data error. Sorry!'
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
    promise: (client) => client.get(path)
  };
}

export function loadById(id) {
  const path = '/templates/getById/' + id;
  console.log(`loading data from ${path}`);

  return {
    types: [LOAD, LOAD_SUCCESS, LOAD_FAIL],
    promise: (client) => client.get(path)
  };
}

export function saveData(data) {
  const path = '/templates/update/' + data.Id;
  console.log(`updatiting data to ${path}`);

  return {
    types: [SAVE, SAVE_SUCCESS, SAVE_FAIL],
    promise: (client) => client.patch(path, {
      data: data
    }),
    data: data
  };
}
