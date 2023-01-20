import http from 'k6/http';
import { sleep, check } from 'k6';

export default function () {
    const res = http.get('http://localhost:8080/test.txt');
    check(res, { 'body was right': (r) => r.body.includes("hello!")});
}