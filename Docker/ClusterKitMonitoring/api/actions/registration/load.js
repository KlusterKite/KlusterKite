const initialData =
  {
    history: [
      {
        date: '31 дек',
        login: 'a_sayanova',
        callResult: 2,
        talkResult: 0,
        comment: 'перезвонить на следующий день'
      },
      {
        date: '1 янв',
        login: 'a_sayanova',
        callResult: 4,
        talkResult: 1,
        comment: ''
      },
      {
        date: '3 янв',
        login: 's_kotovalov',
        callResult: 4,
        talkResult: 1,
        comment: ''
      }
    ]
  };

export function getData(req) {
  return initialData;
}

export default function load(req) {
  return new Promise((resolve, reject) => {
    // make async call to database
    setTimeout(() => {
      resolve(getData(req));
    }, 0); // simulate async load
  });
}
