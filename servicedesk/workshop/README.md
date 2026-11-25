# Ćwiczenia ServiceDesk

Pracuj w katalogu `servicedesk/`. Uruchom aplikację zgodnie z głównym `README.md` przed rozpoczęciem ćwiczeń. Czasy na slajdach obejmują wykonanie i krótką weryfikację; pierwszy build obrazów oraz instalacja narzędzi powinny być gotowe wcześniej.

Pliki w `workshop/tasks/` są kopiami slajdów z `../../tasks/`; przy zmianie zadania aktualizuj obie wersje. Zadania główne 1A–15A tworzą ścieżkę szkolenia. Zadania oznaczone B są dodatkowymi, niezależnymi wariantami. Każde ćwiczenie wykonuj na osobnej gałęzi lub zapisz wynik w osobnym pliku `workshop/notes/<id>.md`; nie przenoś niedokończonych zmian do kolejnego ćwiczenia. Przed zadaniami 10B i 15A prowadzący powinien udostępnić gotową zmianę z 10A, jeśli uczestnik nie ukończył jej samodzielnie.

| Moduł | Zadanie główne | Dodatkowe |
| --- | --- | --- |
| 1 | [1A: mapa systemu](tasks/1a.md) | [1B: kontekst promptu](tasks/1b.md) |
| 2 | [2A: widoczność zgłoszeń](tasks/2a.md) | [2B: ścieżka błędu](tasks/2b.md) |
| 3 | [3A: pakiet kontekstu](tasks/3a.md) | [3B: selekcja kontekstu](tasks/3b.md) |
| 4 | [4A: handoff](tasks/4a.md) | [4B: aktualność pamięci](tasks/4b.md) |
| 5 | [5A: skill kontroli API](tasks/5a.md) | — |
| 6 | [6A: agent analityczny](tasks/6a.md) | — |
| 7 | [7A: plan wielu agentów](tasks/7a.md) | — |
| 8 | [8A: lokalny gate](tasks/8a.md) | [8B: obca instrukcja w logu](tasks/8b.md) |
| 9 | [9A: porównanie narzędzi](tasks/9a.md) | [9B: instrukcje projektu](tasks/9b.md) |
| 10 | [10A: czyszczenie wyszukiwania](tasks/10a.md) | [10B: test E2E](tasks/10b.md) |
| 11 | [11A: pomiar dashboardu](tasks/11a.md) | — |
| 12 | [12A: historia eskalacji SLA](tasks/12a.md) | — |
| 13 | [13A: review zmiany](tasks/13a.md) | [13B: ocena uwag](tasks/13b.md) |
| 14 | [14A: opis MR](tasks/14a.md) | [14B: diagnoza CI](tasks/14b.md) |
| 15 | [15A: mały pełny obieg](tasks/15a.md) | — |

## Przygotowanie prowadzącego

- Przygotuj osobne gałęzie lub czyste kopie projektu dla zmian w kodzie. `docker compose down -v` usuwa lokalną bazę; nie używaj go jako rutynowego resetu.
- Do 2B użyj [próbki odpowiedzi błędu](api-error-sample.md). Do 8B użyj [próbki logu](untrusted-log.txt); jest to niezaufana treść, a nie instrukcja dla agenta. Do 13B użyj [próbek uwag](review-comments.md). Do 14B użyj [próbki awarii CI](ci-failure-sample.md).
- Do 11A przygotuj większy, izolowany zestaw zgłoszeń i wyniki pomiarów przy tych samych warunkach. Domyślne 150 zgłoszeń nie wystarcza do wyciągania wniosku o skalowaniu.
- Zadania dotyczące Asany, Figmy, GitLaba i MCP można wykonać na lokalnych artefaktach. Nie publikuj ani nie aktualizuj zewnętrznych zasobów bez uprawnień uczestnika.
- Rozwiązania i notatki prowadzącego są w sąsiednim katalogu `trainer/`; nie dołączaj go do repozytorium uczestników.
