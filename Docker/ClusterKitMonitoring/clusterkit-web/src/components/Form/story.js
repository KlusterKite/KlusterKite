import React from 'react';
import { storiesOf, action } from '@kadira/storybook';
// import StubContainer from 'react-storybooks-relay-container';

import Submit from './submit';

storiesOf('Form')
  .add('submit enabled', () => {
    return <Submit canSubmit={true} />;
  })
  .add('submit can\'t submit', () => {
    return <Submit canSubmit={false} />;
  })
  .add('submit disabled', () => {
    return <Submit canSubmit={true} disabled={true} />;
  })
  .add('submit custom text', () => {
    return <Submit canSubmit={true} buttonText="Place an order" />;
  })
  .add('submit saving', () => {
    return <Submit canSubmit={true} saving={true} />;
  })
  .add('submit saved', () => {
    return <Submit canSubmit={true} saved={true} />;
  })
  .add('submit saved, custom text', () => {
    return <Submit canSubmit={true} saved={true} savedText="Excellent! My work here is done." />;
  })
  .add('submit failed', () => {
    return <Submit canSubmit={true} saveError="Epic fail. How did that happen?!" />;
  })
  .add('submit with delete', () => {
    return <Submit canSubmit={true} onDelete={action('deleted')} />;
  })
;
