import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  vus: 5,
  duration: '30s',
};

const BASE_URL = 'https://localhost:7208/api';

export default function () {
  const uniqueEmail = `user_${Date.now()}_${Math.floor(Math.random() * 100000)}@test.com`;

  const payload = JSON.stringify({
    fullName: 'Performance Test User',
    email: uniqueEmail,
    password: 'Password123!'
  });

  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  const res = http.post(`${BASE_URL}/auth/register`, payload, params);

  check(res, {
    'register status is successful': (r) => r.status === 200 || r.status === 201,
    'register response below 2000ms': (r) => r.timings.duration < 2000,
  });

  sleep(1);
}