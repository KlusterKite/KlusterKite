// These are the pages you can go to.
// They are all wrapped in the App component, which should contain the navbar etc
// See http://blog.mxstbr.com/2016/01/react-apps-with-pages for more information
// about the code splitting business
import { getAsyncInjectors } from 'utils/asyncInjectors';

const errorLoading = (err) => {
  console.error('Dynamic page loading failed', err); // eslint-disable-line no-console
};

const loadModule = (cb) => (componentModule) => {
  cb(null, componentModule.default);
};

export default function createRoutes(store) {
  // Create reusable async injectors using getAsyncInjectors factory
  const { injectReducer, injectSagas } = getAsyncInjectors(store);

  return [
    {
      path: '/clusterkit',
      name: 'homePage',
      getComponent(nextState, cb) {
        const importModules = Promise.all([
          System.import('containers/HomePage/reducer'),
          System.import('containers/HomePage/sagas'),
          System.import('containers/HomePage'),
        ]);

        const renderRoute = loadModule(cb);

        importModules.then(([reducer, sagas, component]) => {
          injectReducer('homePage', reducer.default);
          injectSagas(sagas.default);
          renderRoute(component);
        });

        importModules.catch(errorLoading);
      },
    },

    {
      path: '/clusterkit/templates',
      name: 'templates',
      getComponent(nextState, cb) {
        const importModules = Promise.all([
          System.import('containers/TemplatesListPage/reducer'),
          System.import('containers/TemplatesListPage/sagas'),
          System.import('containers/TemplatesListPage'),
        ]);

        const renderRoute = loadModule(cb);

        importModules.then(([reducer, sagas, component]) => {
          injectReducer('templatesListPage', reducer.default);
          injectSagas(sagas.default);
          renderRoute(component);
        });

        importModules.catch(errorLoading);
      },
    },
    {
      path: '/clusterkit/templates/:id',
      name: 'templatePage',
      getComponent(nextState, cb) {
        const importModules = Promise.all([
          System.import('containers/TemplatePage/reducer'),
          System.import('containers/TemplatePage/sagas'),
          System.import('containers/TemplatePage'),
        ]);

        const renderRoute = loadModule(cb);

        importModules.then(([reducer, sagas, component]) => {
          injectReducer('templatePage', reducer.default);
          injectSagas(sagas.default);
          renderRoute(component);
        });

        importModules.catch(errorLoading);
      },
    },
    {
      path: '/clusterkit/nugetfeeds',
      name: 'feedsListPage',
      getComponent(nextState, cb) {
        const importModules = Promise.all([
          System.import('containers/FeedsListPage/reducer'),
          System.import('containers/FeedsListPage/sagas'),
          System.import('containers/FeedsListPage'),
        ]);

        const renderRoute = loadModule(cb);

        importModules.then(([reducer, sagas, component]) => {
          injectReducer('feedsListPage', reducer.default);
          injectSagas(sagas.default);
          renderRoute(component);
        });

        importModules.catch(errorLoading);
      },
    },
    {
      path: '/clusterkit/nugetfeeds/:id',
      name: 'feedPage',
      getComponent(nextState, cb) {
        const importModules = Promise.all([
          System.import('containers/FeedPage/reducer'),
          System.import('containers/FeedPage/sagas'),
          System.import('containers/FeedPage'),
        ]);

        const renderRoute = loadModule(cb);

        importModules.then(([reducer, sagas, component]) => {
          injectReducer('feedPage', reducer.default);
          injectSagas(sagas.default);
          renderRoute(component);
        });

        importModules.catch(errorLoading);
      },
    },
    {
      path: '/clusterkit/packages',
      name: 'packagesListPage',
      getComponent(nextState, cb) {
        const importModules = Promise.all([
          System.import('containers/PackagesListPage/reducer'),
          System.import('containers/PackagesListPage/sagas'),
          System.import('containers/PackagesListPage'),
        ]);

        const renderRoute = loadModule(cb);

        importModules.then(([reducer, sagas, component]) => {
          injectReducer('packagesListPage', reducer.default);
          injectSagas(sagas.default);
          renderRoute(component);
        });

        importModules.catch(errorLoading);
      },
    },
    {
      path: '/clusterkit/actorsTree',
      name: 'actorsTreePage',
      getComponent(nextState, cb) {
        const importModules = Promise.all([
          System.import('containers/ActorsTreePage/reducer'),
          System.import('containers/ActorsTreePage/sagas'),
          System.import('containers/ActorsTreePage'),
        ]);

        const renderRoute = loadModule(cb);

        importModules.then(([reducer, sagas, component]) => {
          injectReducer('actorsTreePage', reducer.default);
          injectSagas(sagas.default);
          renderRoute(component);
        });

        importModules.catch(errorLoading);
      },
    },
    {
      path: '/clusterkit/auth',
      name: 'authPage',
      getComponent(nextState, cb) {
        const importModules = Promise.all([
          System.import('containers/AuthPage/reducer'),
          System.import('containers/AuthPage/sagas'),
          System.import('containers/AuthPage'),
        ]);

        const renderRoute = loadModule(cb);

        importModules.then(([reducer, sagas, component]) => {
          injectReducer('authPage', reducer.default);
          injectSagas(sagas.default);
          renderRoute(component);
        });

        importModules.catch(errorLoading);
      },
    },
    {
      path: '/clusterkit/logout',
      name: 'authPage',
      getComponent(nextState, cb) {
        const importModules = Promise.all([
          System.import('containers/LogoutPage'),
        ]);

        const renderRoute = loadModule(cb);

        importModules.then(([component]) => {
          renderRoute(component);
        });

        importModules.catch(errorLoading);
      },
    },
    {
      path: '*',


      name: 'notfound',
      getComponent(nextState, cb) {
        System.import('containers/NotFoundPage')
          .then(loadModule(cb))
          .catch(errorLoading);
      },
    },
  ];
}
