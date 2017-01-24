import memoize from 'lru-memoize';
import { createValidator, required } from '../Forms/validation';


const feedValidation = createValidator({
  Address: required,
});
export default memoize(10)(feedValidation);
