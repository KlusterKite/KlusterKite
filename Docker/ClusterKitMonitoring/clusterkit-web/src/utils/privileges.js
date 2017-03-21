import Storage from './ttl-storage';

let _privileges = null;

/**
 * Gets privileges list for the current authorized user
 * @return {string[]} List of privileges
 */
const getPrivileges = () => {
  if (_privileges) {
    return _privileges;
  }

  const privileges = Storage.get('privileges');
  if (!privileges) {
    return null;
  }

  _privileges = JSON.parse(privileges);
  return(_privileges);
};

export const hasPrivilege = (id) => {
  const privileges = getPrivileges();

  if (privileges === null) {
    return false;
  }

  if (!Array.isArray(privileges)){
    return false;
  }

  return privileges.includes(id);
};
