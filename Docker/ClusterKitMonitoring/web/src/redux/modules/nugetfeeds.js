const LOAD = 'clusterkit-monitoring/nugetFeed/LOAD';
const LOAD_SUCCESS = 'clusterkit-monitoring/nugetFeed/LOAD_SUCCESS';
const LOAD_FAIL = 'clusterkit-monitoring/nugetFeed/LOAD_FAIL';
const SAVE = 'clusterkit-monitoring/nugetFeed/SAVE';
const SAVE_SUCCESS = 'clusterkit-monitoring/nugetFeed/SAVE_SUCCESS';
const SAVE_FAIL = 'clusterkit-monitoring/nugetFeed/SAVE_FAIL';
const REDIRECT_START = 'clusterkit-monitoring/nugetFeed/REDIRECT_START';

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
        saveError: null,
        createId: action.result.Id
      };
    case SAVE_FAIL:
      console.log(action);
      return {
        ...state,
        saving: false,
        saved: false,
        saveError: 'Saving data error. Sorry!'
      };
    case REDIRECT_START:
      return {
        ...state,
        createId: null
      };
    default:
      return state;
  }
}

export function isLoaded(globalState) {
  return globalState.nugetFeed && globalState.nugetFeed.loaded;
}

export function load() {
  const path = '/nugetfeed/get';
  console.log(`loading data from ${path}`);

  return {
    types: [LOAD, LOAD_SUCCESS, LOAD_FAIL],
    promise: (client) => client.get(path)
  };
}

export function loadById(id) {
  const path = '/nugetfeed/getById/' + id;
  console.log(`loading data from ${path}`);

  return {
    types: [LOAD, LOAD_SUCCESS, LOAD_FAIL],
    promise: (client) => client.get(path)
  };
}

export function saveData(data) {
  const path = '/nugetfeed/update/' + data.Id;
  console.log(`updatiting data to ${path}`);

  return {
    types: [SAVE, SAVE_SUCCESS, SAVE_FAIL],
    promise: (client) => client.patch(path, {
      data: data
    }),
    data: data
  };
}

export function createRecord(data) {
  const path = '/nugetfeed/create/';
  console.log(`saving data to ${path}`);

  return {
    types: [SAVE, SAVE_SUCCESS, SAVE_FAIL],
    promise: (client) => client.put(path, {
      data: data
    }),
    data: data
  };
}

export function onRedirectStart() {
  return {
    type: REDIRECT_START
  };
}
