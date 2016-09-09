const isEmpty = value => value === undefined || value === null || value === '';
const join = (rules) => (value, data) => rules.map(rule => rule(value, data)).filter(error => !!error)[0 /* first error */ ];

export function email(value) {
  // Let's not start a debate on email regex. This is just for an example app!
  if (!isEmpty(value) && !/^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$/i.test(value)) {
    return 'Invalid email address';
  }
}

export function required(value) {
  if (isEmpty(value)) {
    return 'Required';
  }
}

export function minLength(min) {
  return value => {
    if (!isEmpty(value) && value.length < min) {
      return `Must be at least ${min} characters`;
    }
  };
}

export function maxLength(max) {
  return value => {
    if (!isEmpty(value) && value.length > max) {
      return `Must be no more than ${max} characters`;
    }
  };
}

export function minValue(min) {
  return value => {
    if (!isEmpty(value) && value * 1 < min) {
      return `Must be at least ${min}`;
    }
  };
}

export function maxValue(max) {
  return value => {
    if (!isEmpty(value) && value * 1 > max) {
      return `Must be no more than ${max}`;
    }
  };
}

export function integer(value) {
  if (value != undefined && !Number.isInteger(Number(value))) {
    return 'Must be an integer';
  }
}

export function oneOf(enumeration) {
  return value => {
    if (!~enumeration.indexOf(value)) {
      return `Must be one of: ${enumeration.join(', ')}`;
    }
  };
}

export function match(field) {
  return (value, data) => {
    if (data) {
      if (value !== data.get(field)) {
        return 'Do not match';
      }
    }
  };
}

export function lessOrEqualThan(field) {

  return (value, data) => {
    var valueToCompare = Number.isInteger(Number(field)) ?  field : data.get(field);

    if (value && data && valueToCompare != null && valueToCompare != undefined) {
      if (value * 1 > valueToCompare * 1) {
        return 'Must be less or equal than ' + valueToCompare;
      }
    }
  };
}

export function moreOrEqualThan(field) {

  return (value, data) => {
    var valueToCompare = Number.isInteger(Number(field)) ? field : data.get(field);

    if (value && data && valueToCompare != null && valueToCompare != undefined) {
      if (value * 1 < valueToCompare * 1) {
        return 'Must be more or equal than ' + valueToCompare;
      }
    }
  };
}

export function lessThan(field) {

  return (value, data) => {
    var valueToCompare = Number.isInteger(Number(field)) ?  field : data.get(field);

    if (value && data && valueToCompare != null && valueToCompare != undefined) {
      if (value * 1 >= valueToCompare * 1) {
        return 'Must be less than ' + valueToCompare;
      }
    }
  };
}

export function moreThan(field) {

  return (value, data) => {
    var valueToCompare = Number.isInteger(Number(field)) ? field : data.get(field);

    if (value && data && valueToCompare != null && valueToCompare != undefined) {
      if (value * 1 <= valueToCompare * 1) {
        return 'Must be more than ' + valueToCompare;
      }
    }
  };
}

export function createValidator(rules) {
  return (data = {}, props = {}) => {

    const errors = {};
    Object.keys(rules).forEach((key) => {
      const rule = join([].concat(rules[key])); // concat enables both functions and arrays of functions
      const error = rule(data.get(key), data);
      if (error) {
        errors[key] = error;
      }
    });
    return errors;
  };
}
