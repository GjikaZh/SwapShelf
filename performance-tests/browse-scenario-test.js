import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  vus: 15,
  duration: '45s',
};

const BASE_URL = 'https://localhost:7208/api';

export default function () {
  const booksRes = http.get(`${BASE_URL}/books`);
  check(booksRes, {
    'books loaded successfully': (r) => r.status === 200,
    'books response below 1000ms': (r) => r.timings.duration < 1000,
  });

  sleep(1);

  const listingsRes = http.get(`${BASE_URL}/listings`);
  check(listingsRes, {
    'listings loaded successfully': (r) => r.status === 200,
    'listings response below 1500ms': (r) => r.timings.duration < 1500,
  });

  sleep(1);
}