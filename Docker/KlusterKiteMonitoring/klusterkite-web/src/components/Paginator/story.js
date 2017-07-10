import React from 'react';
import { storiesOf, action } from '@kadira/storybook';

import Paginator from './Paginator';

storiesOf('Paginator')
  .add('page 1 (10 items), 65 items totals', () => {
    return <Paginator currentPage={1} totalItems={65} itemsPerPage={10} onSelect={action('page selected')} />;
  })
  .add('page 3 (10 items), 65 items totals', () => {
    return <Paginator currentPage={3} totalItems={65} itemsPerPage={10} onSelect={action('page selected')} />;
  })
;
