import React from 'react';
import { storiesOf, action } from '@kadira/storybook';
// import StubContainer from 'react-storybooks-relay-container';

import AuthForm from './index';

storiesOf('Authorization')
  .add('login form', () => {
    return <AuthForm onSubmit={action('submitted')} />;
  })
  .add('login form, authorized', () => {
    return <AuthForm onSubmit={action('submitted')} authorized={true} />;
  })
  .add('login form, error', () => {
    return <AuthForm onSubmit={action('submitted')} authorizationError="Access denied. Have a nice day!" />;
  })
;
