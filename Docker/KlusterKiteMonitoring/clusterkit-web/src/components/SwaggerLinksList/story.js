import React from 'react';
import { storiesOf, action } from '@kadira/storybook';
// import StubContainer from 'react-storybooks-relay-container';

import SwaggerLinksList from './index';

storiesOf('Homepage')
  .add('swagger list', () => {
    const links = getSwaggerList();
    return <SwaggerLinksList links={links} />;
  })
;

let getSwaggerList = function () {
  return ["clusterkit/manager/swagger/ui"];
};
