import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '20s', target: 10 },
    { duration: '20s', target: 25 },
    { duration: '20s', target: 50 },
    { duration: '20s', target: 0 },
  ],
};

const BASE_URL = 'https://localhost:7208/api';

export default function () {
  const res = http.get(`${BASE_URL}/listings`);

  check(res, {
    'status is 200': (r) => r.status === 200,
    'response below 3000ms': (r) => r.timings.duration < 3000,
  });

  sleep(1);
}