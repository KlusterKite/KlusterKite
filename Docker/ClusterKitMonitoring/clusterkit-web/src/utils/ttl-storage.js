const now = () => +new Date();

export default class Storage {
  static get(key) {
    const item = localStorage.getItem(key);
    if (item === null) return undefined;

    const entry = JSON.parse(localStorage.getItem(key));
    if (entry.ttl && entry.ttl + entry.now < now()) {
      localStorage.removeItem(key);
      return undefined;
    }

    return entry.value;
  }

  static remove(key) {
    const item = localStorage.getItem(key);
    if (item !== null) {
      localStorage.removeItem(key);
    }
  }

  static set(key, value, ttl) {
    localStorage.setItem(key, JSON.stringify({
      ttl: ttl || 0,
      now: now(),
      value,
    }));
  }
}
