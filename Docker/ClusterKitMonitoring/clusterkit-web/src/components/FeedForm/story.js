import React from 'react';
import { storiesOf, action } from '@kadira/storybook';
// import StubContainer from 'react-storybooks-relay-container';

import FeedForm from './index';

storiesOf('Feeds')
  .add('edit', () => {
    const feed = getFeed();
    return <FeedForm onSubmit={action('submitted')} onDelete={action('deleted')} initialValues={feed} />;
  })
  .add('create', () => {
    return <FeedForm onSubmit={action('submitted')} />;
  })
;

let getFeed = function () {
  const feed = {"address":"/opt/packageCache","type":"Private","userName":null,"password":null};
  return feed;
};
