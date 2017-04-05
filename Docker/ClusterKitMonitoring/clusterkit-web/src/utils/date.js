/**
 * Formats to date to dd.MM.yyyy mm:hh format
 * @param d {Date} Date
 * @return {string} Formatted date
 */
const formatDateTime = (d) => {
  return ("0" + d.getDate()).slice(-2) + "-" + ("0"+(d.getMonth()+1)).slice(-2) + "-" +
    d.getFullYear() + " " + ("0" + d.getHours()).slice(-2) + ":" + ("0" + d.getMinutes()).slice(-2);
};

export default {
  formatDateTime: formatDateTime,
};
