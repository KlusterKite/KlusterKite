export default function update(req) {
  return new Promise((resolve, reject) => {
    // write to database
    setTimeout(() => {
      if (Math.random() < 0.2) {
        reject('Registration error. Please try again.');
      } else {
        // const widgets = load(req);
        const widget = req.body;

        resolve(widget);
      }
    }, 1500); // simulate async db write
  });
}
