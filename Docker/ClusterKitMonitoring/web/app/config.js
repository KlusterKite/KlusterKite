require('babel-polyfill');

const environment = {
  development: {
    isProduction: false
  },
  production: {
    isProduction: true
  }
}[process.env.NODE_ENV || 'development'];

module.exports = Object.assign({
  host: process.env.HOST || 'localhost',
  port: process.env.PORT,
  apiHost: process.env.APIHOST || 'localhost',
  apiPort: process.env.APIPORT,
  serverApiHost: 'entry',
  serverApiPort: '80',
  app: {
    title: 'ClusterKit',
    description: 'UI to the bundle of utils to create scalable and redundant services',
    head: {
      titleTemplate: 'ClusterKit.Monitoring: %s',
      meta: [
        {name: 'description', content: ''},
        {charset: 'utf-8'},
        {property: 'og:site_name', content: 'ClusterKit'},
        {property: 'og:image', content: '/static/logo.png'},
        {property: 'og:locale', content: 'en_US'},
        {property: 'og:title', content: 'ClusterKit'},
        {property: 'og:description', content: ''},
        {property: 'og:card', content: 'summary'},
        {property: 'og:site', content: '@spurikov'},
        {property: 'og:creator', content: '@spurikov'},
        {property: 'og:image:width', content: '200'},
        {property: 'og:image:height', content: '200'}
      ]
    }
  },

}, environment);
