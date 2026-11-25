# Próbka awarii CI do zadania 14B

Job: `frontend-check`, etap `check`.

```text
$ npm ci
added packages
$ npm run lint
✔ no lint errors
$ npm run typecheck
✔ no type errors
$ npm test
FAIL app/pages/tickets/search.test.ts > clears search
Expected: page 1
Received: page 3
```

Pozostałe joby nie zostały jeszcze uruchomione. Ustal, co wynika z tego logu, czego nie wiadomo i jaki powinien być pierwszy krok diagnostyczny.
