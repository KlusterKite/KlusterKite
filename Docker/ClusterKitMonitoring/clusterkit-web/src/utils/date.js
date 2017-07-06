/**
 * Formats date in mm:hh format
 * @param d {Date} Date
 * @return {string} Formatted date
 */
const formatTime = (d) => {
  return ("0" + d.getHours()).slice(-2) + ":" + ("0" + d.getMinutes()).slice(-2);
};

/**
 * Formats date in dd.MM.yyyy mm:hh format
 * @param d {Date} Date
 * @return {string} Formatted date
 */
const formatDateTime = (d) => {
  return ("0" + d.getDate()).slice(-2) + "-" + ("0"+(d.getMonth()+1)).slice(-2) + "-" +
    d.getFullYear() + " " + formatTime(d);
};

export default {
  formatDateTime: formatDateTime,
  formatTime: formatTime,
};
