# AI-Powered Development z Claude Code i GitHub Copilot

To repozytorium `r-ai-1` zawiera:

- [`servicedesk/`](./servicedesk/) — projekt warsztatowy i [przewodnik po ćwiczeniach](./servicedesk/workshop/README.md);
- [`materials/presentation.pdf`](./materials/presentation.pdf) — prezentacja PDF;
- [`materials/workbook.pdf`](./materials/workbook.pdf) — zadania, pytania po ćwiczeniach i miejsce na notatki;
- [`materials/tasks/`](./materials/tasks/) — treść zadań w plikach Markdown;
- [`materials/lektury-uzupelniajace.md`](./materials/lektury-uzupelniajace.md) — artykuły i dokumentacja do dalszej nauki;
- [`materials/przydatne-narzedzia.md`](./materials/przydatne-narzedzia.md) — narzędzia do wypróbowania po warsztacie.
- [`materials/CLAUDE.md`](./materials/CLAUDE.md) — wzorcowe instrukcje projektu, które harness może umieścić w katalogu projektu po dostosowaniu.

Zacznij od [workbooka](./materials/workbook.pdf). Zawiera zadania, pytania po ćwiczeniach i miejsce na notatki.

## Uruchomienie projektu

Wymagany jest Docker z Compose v2. Po sklonowaniu repozytorium:

```sh
git clone https://github.com/matwjablonski/r-ai-1.git
cd r-ai-1/servicedesk
docker compose up --build -d
docker compose ps
```

Aplikacja jest dostępna pod adresem <http://localhost:3000>. Dalsze instrukcje są w [README ServiceDesk](./servicedesk/README.md).
