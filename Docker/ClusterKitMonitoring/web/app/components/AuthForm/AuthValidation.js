import memoize from 'lru-memoize';
import { createValidator, required } from '../Forms/validation';


const authValidation = createValidator({
  Login: required,
  Password: required,
});
export default memoize(10)(authValidation);
