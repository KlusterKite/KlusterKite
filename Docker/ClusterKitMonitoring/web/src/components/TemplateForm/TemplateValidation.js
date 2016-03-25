import memoize from 'lru-memoize';
import {createValidator, required} from 'utils/validation';
// checked, maxLength, email

const templateValidation = createValidator({
  Code: required
});
export default memoize(10)(templateValidation);
