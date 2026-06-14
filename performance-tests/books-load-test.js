import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  vus: 10,
  duration: '30s',
};

const BASE_URL = 'https://localhost:7208/api';

export default function () {
  const res = http.get(`${BASE_URL}/books`);

  check(res, {
    'books status is 200': (r) => r.status === 200,
    'books response below 1000ms': (r) => r.timings.duration < 1000,
  });

  sleep(1);
}