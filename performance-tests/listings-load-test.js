import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  vus: 20,
  duration: '30s',
};

const BASE_URL = 'https://localhost:7208/api';

export default function () {
  const res = http.get(`${BASE_URL}/listings`);

  check(res, {
    'listings status is 200': (r) => r.status === 200,
    'listings response below 1500ms': (r) => r.timings.duration < 1500,
  });

  sleep(1);
}