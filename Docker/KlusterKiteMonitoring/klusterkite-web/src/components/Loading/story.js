import React from 'react';
import { storiesOf, action } from '@kadira/storybook';
// import StubContainer from 'react-storybooks-relay-container';

import Loading from './index';

storiesOf('Loading')
  .add('loading indicator', () => {
    return <Loading />;
  })
;
