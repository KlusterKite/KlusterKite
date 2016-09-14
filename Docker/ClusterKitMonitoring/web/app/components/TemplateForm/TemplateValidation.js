import memoize from 'lru-memoize';
import { createValidator, required, integer, moreThan, maxValue, moreOrEqualThan, lessOrEqualThan } from '../Forms/validation';


const templateValidation = createValidator({
  Code: required,
  Name: required,
  MinimumRequiredInstances: [integer, lessOrEqualThan('MaximumNeededInstances'), required, moreThan(0)],
  MaximumNeededInstances: [integer, moreOrEqualThan('MinimumRequiredInstances')],
  Priority: [integer, maxValue(100)],
  Version: integer,
});
export default memoize(10)(templateValidation);
