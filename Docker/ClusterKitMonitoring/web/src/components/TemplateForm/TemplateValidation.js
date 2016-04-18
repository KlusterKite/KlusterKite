import memoize from 'lru-memoize';
import {createValidator, required, integer, moreThan, lessThan, maxValue} from 'utils/validation';
// checked, maxLength, email

const templateValidation = createValidator({
  Code: required,
  MininmumRequiredInstances: [integer, lessThan('MaximumNeededInstances')],
  MaximumNeededInstances: [integer, moreThan('MininmumRequiredInstances')],
  Priority: [integer, maxValue(100)],
  Version: integer,
});
export default memoize(10)(templateValidation);
