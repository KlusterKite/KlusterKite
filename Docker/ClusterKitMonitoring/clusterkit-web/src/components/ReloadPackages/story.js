import React from 'react';
import { storiesOf, action } from '@kadira/storybook';
import StubContainer from 'react-storybooks-relay-container';

import ReloadPackages from './index';

storiesOf('Homepage')
  .add('reload packages', () => {
    return <ReloadPackages testMode={true} />;
  })
;
