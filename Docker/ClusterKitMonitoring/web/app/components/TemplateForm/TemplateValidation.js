import memoize from 'lru-memoize';
import {createValidator, required, integer, moreThan, lessThan, maxValue, moreOrEqualThan, lessOrEqualThan} from '../Forms/validation';
// checked, maxLength, email

const templateValidation = createValidator({
  Code: required,
  Name: required,
  MininmumRequiredInstances: [integer, lessOrEqualThan('MaximumNeededInstances'), required, moreThan(0)],
  MaximumNeededInstances: [integer, moreOrEqualThan('MininmumRequiredInstances')],
  Priority: [integer, maxValue(100)],
  Version: integer,
});
export default memoize(10)(templateValidation);
