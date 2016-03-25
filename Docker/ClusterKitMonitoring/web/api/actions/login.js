export default function login(req) {
  const user = {
    name: req.body.name,
    token: '1d756923-f437-4b16-911b-72a398a4f185'
  };
  req.session.user = user;
  return Promise.resolve(user);
}
